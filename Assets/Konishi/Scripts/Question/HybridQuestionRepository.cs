
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ローカル問題とSupabase問題を統合するRepository。
///
/// ローカルの種データ・AI生成データを維持しながら、
/// Supabaseの検証済み問題も出題候補に追加する。
/// </summary>
public class HybridQuestionRepository : IQuestionRepository
{
    [Serializable]
    private class QuestionListResponse
    {
        public List<QuestionData> questions;
        public int count;
    }

    private readonly IQuestionRepository localRepository;
    private readonly AIClientSettings settings;

    private readonly List<QuestionData> allQuestions =
        new List<QuestionData>();

    private readonly HashSet<string> identityKeys =
        new HashSet<string>();

    private bool initialized;

    public HybridQuestionRepository(
        IQuestionRepository localRepository,
        AIClientSettings settings)
    {
        this.localRepository = localRepository;
        this.settings = settings;
    }

    /// <summary>
    /// ローカル問題とSupabase問題を読み込む。
    /// Supabaseに接続できなくてもローカル問題で続行する。
    /// </summary>
    public async Task InitializeAsync()
    {
        if (initialized) return;

        initialized = true;

        // 先にローカル問題を読み込む
        await localRepository.InitializeAsync();

        allQuestions.Clear();
        identityKeys.Clear();

        var localQuestions =
            await localRepository.GetQuestionsBySubjectAsync(null);

        AddRange(localQuestions);

        Debug.Log(
            "[Question] ローカル問題を読み込みました: " +
            allQuestions.Count + "問");

        // Supabaseから検証済み問題を取得
        await LoadSupabaseQuestionsAsync();

        Debug.Log(
            "[Question] 統合された問題数: " +
            allQuestions.Count + "問");
    }

    /// <summary>
    /// Supabaseから問題一覧を取得する。
    /// </summary>
    private async Task LoadSupabaseQuestionsAsync()
    {
        if (settings == null)
        {
            Debug.LogWarning(
                "[Question] AIClientSettingsが見つからないため、" +
                "Supabase取得をスキップします。");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.getQuestionsUrl))
        {
            Debug.LogWarning(
                "[Question] getQuestionsUrlが未設定です。");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.serverToken))
        {
            Debug.LogWarning(
                "[Question] serverTokenが未設定です。");
            return;
        }

        try
        {
            byte[] body = Encoding.UTF8.GetBytes("{}");

            using (var request = new UnityWebRequest(
                settings.getQuestionsUrl,
                UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json");

                request.SetRequestHeader(
                    "Authorization",
                    "Bearer " + settings.serverToken);

                request.timeout = settings.timeoutSeconds;

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        "[Question] Supabase取得失敗: " +
                        request.responseCode + " / " +
                        request.error + " / " +
                        request.downloadHandler.text);

                    return;
                }

                var response =
                    JsonUtility.FromJson<QuestionListResponse>(
                        request.downloadHandler.text);

                if (response == null || response.questions == null)
                {
                    Debug.LogWarning(
                        "[Question] Supabaseの応答を解析できませんでした。");
                    return;
                }

                int added = 0;

                foreach (var question in response.questions)
                {
                    if (question == null) continue;

                    // サーバーから取得したものは検証済み
                    question.verified = true;

                    if (Add(question))
                    {
                        added++;
                    }
                }

                Debug.Log(
                    "[Question] Supabaseから " +
                    response.questions.Count + "問取得。新規追加: " +
                    added + "問");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "[Question] Supabase取得中に例外が発生しました: " +
                e.Message);
        }
    }

    /// <summary>
    /// 統合リストに問題を追加する。
    /// 問題文が同じものは重複登録しない。
    /// </summary>
    private bool Add(QuestionData question)
    {
        if (question == null) return false;

        string key = question.IdentityKey;

        if (string.IsNullOrEmpty(key)) return false;
        if (identityKeys.Contains(key)) return false;

        allQuestions.Add(question);
        identityKeys.Add(key);

        return true;
    }

    private void AddRange(IEnumerable<QuestionData> questions)
    {
        if (questions == null) return;

        foreach (var question in questions)
        {
            Add(question);
        }
    }

    // --------------------------------------------------
    // IQuestionRepository
    // --------------------------------------------------

    public Task<QuestionData> GetQuestionAsync(string id)
    {
        return Task.FromResult(
            allQuestions.Find(q => q.id == id));
    }

    public Task<List<QuestionData>> GetQuestionsByRelatedTermAsync(
        string relatedTerm,
        string subject = null)
    {
        var result = allQuestions.FindAll(q =>
            q.HasTerm(relatedTerm) &&
            (string.IsNullOrEmpty(subject) || q.subject == subject));

        return Task.FromResult(result);
    }

    public Task<int> CountQuestionsByRelatedTermAsync(
        string relatedTerm,
        string subject = null)
    {
        int count = allQuestions.FindAll(q =>
            q.HasTerm(relatedTerm) &&
            (string.IsNullOrEmpty(subject) || q.subject == subject)
        ).Count;

        return Task.FromResult(count);
    }

    public Task<List<QuestionData>> GetQuestionsBySubjectAsync(
        string subject,
        bool verifiedOnly = false)
    {
        var result = allQuestions.FindAll(q =>
            (string.IsNullOrEmpty(subject) || q.subject == subject) &&
            (!verifiedOnly || q.verified));

        return Task.FromResult(result);
    }

    public async Task<bool> SaveQuestionAsync(QuestionData question)
    {
        // ローカルへの保存は今までどおり維持する
        bool saved = await localRepository.SaveQuestionAsync(question);

        // 統合リストにも追加
        Add(question);

        return saved;
    }

    public Task<List<string>> GetSubjectsAsync()
    {
        var result = new List<string>();

        foreach (var question in allQuestions)
        {
            if (string.IsNullOrWhiteSpace(question.subject))
                continue;

            if (!result.Contains(question.subject))
            {
                result.Add(question.subject);
            }
        }

        return Task.FromResult(result);
    }
}
