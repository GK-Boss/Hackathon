
using System;
using System.Threading.Tasks;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public static QuizManager instance;

    [Header("UI")]
    [SerializeField] private QuestionUI questionUI;

    public static bool success = false;
    public static bool giveUp = false;
    public static RewardType reward;

    // 現在、画面に表示している問題
    private QuestionData currentQuestion;

    // 次回表示するために準備しておいた問題
    private QuestionData preparedQuestion;

    // 次の問題を準備している非同期処理
    private Task<QuestionData> preparationTask;

    // ChangeQuestionの多重実行を防ぐ
    private bool isChangingQuestion;

    // 次の問題をボス問題にするかどうか
    private bool prepareBossQuestion;

    public QuestionData CurrentQuestion => currentQuestion;
    public QuestionData PreparedQuestion => preparedQuestion;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[QuizManager] インスタンスが重複しています。");
            return;
        }

        instance = this;
    }

    /// <summary>
    /// 問題を切り替える。
    ///
    /// 1. 準備済みの問題を取得して表示
    /// 2. 今回の報酬を決定
    /// 3. 報酬に応じて次の問題をバックグラウンドで準備
    ///
    /// 呼び出し側は従来どおり ChangeQuestion() を呼ぶだけでよい。
    /// </summary>
    public async void ChangeQuestion()
    {
        if (isChangingQuestion)
        {
            Debug.LogWarning("[QuizManager] 問題切り替え中のため、重複呼び出しを無視しました。");
            return;
        }

        isChangingQuestion = true;

        try
        {
            // 1. 次に表示する問題を取得する
            QuestionData nextQuestion = await GetPreparedQuestionAsync();

            if (nextQuestion == null)
            {
                Debug.LogError(
                    "[QuizManager] 次の問題を取得できませんでした。"
                    + "QuestionManagerの出題候補や問題生成を確認してください。"
                );
                return;
            }

            currentQuestion = nextQuestion;

            // 問題UIに表示する
            if (questionUI == null)
            {
                Debug.LogError("[QuizManager] QuestionUIがInspectorに設定されていません。");
                return;
            }

            questionUI.SetData(currentQuestion);

            // 表示した問題に関連する知識を記録する。
            // QuestionManager側は正誤にかかわらず、触れた知識を記録する仕様。
            if (QuestionManager.instance != null)
            {
                QuestionManager.instance.RegisterQuestionAnswered(
                    currentQuestion,
                    true
                );
            }
            else
            {
                Debug.LogError("[QuizManager] QuestionManager.instanceがありません。");
                return;
            }

            // 2. 今回の報酬を決定する
            if (RewardManager.instance == null)
            {
                Debug.LogError("[QuizManager] RewardManager.instanceがありません。");
                return;
            }

            reward = prepareBossQuestion ? RewardManager.instance.SelectReward_Challenge() : RewardManager.instance.SelectRewardType();

            Debug.Log($"[QuizManager] 今回の報酬: {reward}");

            // 3. 報酬に応じて次の問題を準備する
            prepareBossQuestion = reward == RewardType.ReloadChallenge;

            preparationTask = PrepareNextQuestionAsync(prepareBossQuestion);
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuizManager] 問題切り替え中に例外が発生しました。\n{e}");
        }
        finally
        {
            isChangingQuestion = false;
        }
    }

    /// <summary>
    /// 準備済みの問題を取得する。
    /// 準備がまだ終わっていなければ、完了するまで非同期で待つ。
    /// </summary>
    private async Task<QuestionData> GetPreparedQuestionAsync()
    {
        // すでに問題が準備されている場合
        if (preparedQuestion != null)
        {
            QuestionData question = preparedQuestion;
            preparedQuestion = null;
            return question;
        }

        // バックグラウンド生成中なら、その結果を待つ
        if (preparationTask != null)
        {
            Task<QuestionData> task = preparationTask;
            preparationTask = null;

            preparedQuestion = await task;

            if (preparedQuestion != null)
            {
                QuestionData question = preparedQuestion;
                preparedQuestion = null;
                return question;
            }

            Debug.LogWarning(
                "[QuizManager] 次の問題の準備に失敗しました。通常問題を取得します。"
            );
        }

        // 初回、または生成失敗時は通常問題を取得する
        return await GetNextNormalQuestionAsync();
    }

    /// <summary>
    /// 次の問題をバックグラウンドで準備する。
    /// ここではUIを変更しない。
    /// </summary>
    private async Task<QuestionData> PrepareNextQuestionAsync(bool isBoss)
    {
        if (QuestionManager.instance == null)
        {
            Debug.LogError("[QuizManager] QuestionManager.instanceがありません。");
            return null;
        }

        try
        {
            if (isBoss)
            {
                QuestionData bossQuestion = await QuestionManager.instance.GenerateBossQuestion();

                if (bossQuestion != null)
                {
                    return bossQuestion;
                }

                Debug.LogWarning(
                    "[QuizManager] ボス問題を生成できませんでした。"
                    + "通常問題に切り替えます。"
                );
            }

            // ボス問題以外、またはボス生成に失敗した場合
            return await GetNextNormalQuestionAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuizManager] 次の問題の準備に失敗しました。\n{e}");
            return null;
        }
    }

    /// <summary>
    /// 通常問題を取得する。
    /// 出題候補の読み込みが完了していなければ、完了を待つ。
    /// </summary>
    private async Task<QuestionData> GetNextNormalQuestionAsync()
    {
        QuestionManager manager = QuestionManager.instance;

        if (manager == null)
        {
            Debug.LogError("[QuizManager] QuestionManager.instanceがありません。");
            return null;
        }

        // 出題候補の読み込みを待つ
        const int maxWaitFrames = 600;

        int waitedFrames = 0;

        while (!manager.IsReady && waitedFrames < maxWaitFrames)
        {
            await Task.Yield();
            waitedFrames++;
        }

        if (!manager.IsReady)
        {
            Debug.LogError(
                "[QuizManager] 出題候補の読み込みがタイムアウトしました。"
            );
            return null;
        }

        QuestionData question = manager.GetNextQuestion();

        if (question == null)
        {
            Debug.LogError("[QuizManager] 通常問題を取得できませんでした。");
            return null;
        }

        return question;
    }
}

public enum RewardType
{
    Chain_Normal,
    Chain_Rare,
    Chain_Boss,
    ReloadChallenge,
    Reload,
    ReloadRush
}
