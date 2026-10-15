
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// AI通信の動作確認用。
/// 開発用のDirectOpenAIモードで問題を1問生成し、結果を検証する。
/// </summary>
public class QuestionSystemDebugTest : MonoBehaviour
{
    [Header("AI生成テスト")]
    [SerializeField] private string subject = "薬理学";
    [SerializeField] private string relatedTerm = "アドレナリン";
    [SerializeField, Range(1, 4)] private int difficulty = 1;

    private CancellationTokenSource cancellation;

    private async void Start()
    {
        cancellation = new CancellationTokenSource();

        try
        {
            await RunTestAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[AIテスト] テストをキャンセルしました。");
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[AIテスト] 予期しないエラーが発生しました。\n" + e
            );
        }
    }

    private async Task RunTestAsync(CancellationToken token)
    {
        Debug.Log("[AIテスト] 開始");

        // 1. 接続設定を読み込む
        AIClientSettings settings = AIClientSettings.LoadOrNull();

        if (settings == null)
        {
            Debug.LogError(
                "[AIテスト] AIClientSettingsが見つかりません。\n" +
                "Assets/Konishi/Resources/AIClientSettings.asset を確認してください。"
            );
            return;
        }

        // 2. 開発用の直接通信モードか確認
        if (settings.mode != AIClientSettings.Mode.DirectOpenAI_DevelopmentOnly)
        {
            Debug.LogError(
                "[AIテスト] DirectOpenAI_DevelopmentOnlyに設定してください。\n" +
                "現在のモード: " + settings.mode
            );
            return;
        }

        if (!settings.IsConfigured)
        {
            Debug.LogError(
                "[AIテスト] OpenAI APIキーが設定されていません。\n" +
                "InspectorのAIClientSettingsを確認してください。"
            );
            return;
        }

        // 3. 既存の通信クラスと生成クラスを利用する
        IQuestionAIClient client = settings.CreateClient();

        if (client == null || !client.IsAvailable)
        {
            Debug.LogError("[AIテスト] AIクライアントを利用できません。");
            return;
        }

        var generator = new AIQuestionGenerator(client, true);

        Debug.Log("[AIテスト] OpenAIへのリクエストを開始します。");
        Debug.Log(
            $"[AIテスト] 分野: {subject} / 関連語: {relatedTerm} / 難易度: {difficulty}"
        );

        // 4. AIに問題を1問生成させる
        // 形式検証に失敗した場合、Generator側で修正依頼を1回行うことがあります。
        QuestionData question = await generator.GenerateQuestion(
            subject,
            relatedTerm,
            difficulty,
            token
        );

        if (token.IsCancellationRequested)
            return;

        if (question == null)
        {
            Debug.LogError(
                "[AIテスト] 問題を生成できませんでした。\n" +
                "Console上部の[AI]・[Question]ログも確認してください。"
            );
            return;
        }

        // 5. 生成結果を検証する
        if (!AIQuestionValidator.Validate(
                question,
                QuestionRequest.DefaultChoiceCount,
                subject,
                null,
                out string reason))
        {
            Debug.LogError("[AIテスト] 問題の検証に失敗しました: " + reason);
            return;
        }

        // 6. 結果を表示する
        Debug.Log("[AIテスト] 問題の生成と形式検証に成功しました！");
        Debug.Log("[AIテスト] 問題文: " + question.questionText);

        for (int i = 0; i < question.choices.Length; i++)
        {
            string mark = i == question.correctIndex ? " ← 正解" : "";
            Debug.Log($"[AIテスト] 選択肢{i + 1}: {question.choices[i]}{mark}");
        }

        Debug.Log("[AIテスト] 解説: " + question.explanation);
        Debug.Log("[AIテスト] 関連語数: " + question.relatedTerms.Count);
        Debug.Log("[AIテスト] 難易度: " + question.difficulty);
        Debug.Log("[AIテスト] テスト完了");
    }

    private void OnDestroy()
    {
        if (cancellation == null)
            return;

        cancellation.Cancel();
        cancellation.Dispose();
        cancellation = null;
    }
}
