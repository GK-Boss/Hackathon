using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// IQuestionRepository のローカル実装。サーバーができるまでの仮置き。
///
/// Tomo注: 問題は2か所から来ます。
///           1. 種データ  Assets/Konishi/Resources/questions_seed.json（人が検証した固定問題。gitに入れる）
///           2. 生成分    Application.persistentDataPath/questions.json（AIが作った分。端末ごと）
///
///         サーバーができたら ServerQuestionRepository を作って差し替えてください。
///         QuestionManager 側は IQuestionRepository しか見ていないので、修正は不要です。
/// </summary>
public class LocalJsonQuestionRepository : IQuestionRepository
{
    public const string SeedResourceName = "questions_seed";
    public const string SaveFileName = "questions.json";

    private readonly List<QuestionData> questions = new List<QuestionData>();
    private readonly HashSet<string> identityKeys = new HashSet<string>();
    private bool initialized;

    /// テストから差し替えられるように、保存先を上書きできるようにしている。
    private readonly string savePathOverride;

    public LocalJsonQuestionRepository(string savePathOverride = null)
    {
        this.savePathOverride = savePathOverride;
    }

    public string SavePath => savePathOverride ?? Path.Combine(Application.persistentDataPath, SaveFileName);

    public IReadOnlyList<QuestionData> All => questions;
    public int Count => questions.Count;

    // ------------------------------------------------------------------
    // 読み込み
    // ------------------------------------------------------------------

    public Task InitializeAsync()
    {
        if (initialized) return Task.CompletedTask;
        initialized = true;

        LoadSeed();
        LoadFromDisk();
        return Task.CompletedTask;
    }

    /// Resources の questions_seed.json を読む。無くてもエラーにしない。
    public int LoadSeed()
    {
        var asset = Resources.Load<TextAsset>(SeedResourceName);
        if (asset == null)
        {
            Debug.LogWarning("[Question] 種データ " + SeedResourceName + ".json が見つかりません。");
            return 0;
        }
        return AddRange(FromJson(asset.text));
    }

    /// 保存済みの生成問題を読む。初回は存在しないので0件で正常。
    public int LoadFromDisk()
    {
        if (!File.Exists(SavePath)) return 0;

        try
        {
            return AddRange(FromJson(File.ReadAllText(SavePath)));
        }
        catch (Exception e)
        {
            // 壊れていてもゲームは続行する。種データだけで動く。
            Debug.LogWarning("[Question] 保存データを読めませんでした: " + e.Message);
            return 0;
        }
    }

    // ------------------------------------------------------------------
    // IQuestionRepository
    // ------------------------------------------------------------------

    public Task<QuestionData> GetQuestionAsync(string id)
    {
        return Task.FromResult(questions.Find(q => q.id == id));
    }

    public Task<List<QuestionData>> GetQuestionsByRelatedTermAsync(string relatedTerm, string subject = null)
    {
        return Task.FromResult(questions.FindAll(q =>
            q.HasTerm(relatedTerm) && (string.IsNullOrEmpty(subject) || q.subject == subject)));
    }

    public Task<int> CountQuestionsByRelatedTermAsync(string relatedTerm, string subject = null)
    {
        int count = questions.FindAll(q =>
            q.HasTerm(relatedTerm) && (string.IsNullOrEmpty(subject) || q.subject == subject)).Count;
        return Task.FromResult(count);
    }

    public Task<List<QuestionData>> GetQuestionsBySubjectAsync(string subject, bool verifiedOnly = false)
    {
        return Task.FromResult(questions.FindAll(q =>
            (string.IsNullOrEmpty(subject) || q.subject == subject) && (!verifiedOnly || q.verified)));
    }

    public Task<bool> SaveQuestionAsync(QuestionData question)
    {
        if (!Add(question)) return Task.FromResult(false);

        WriteGeneratedToDisk();
        return Task.FromResult(true);
    }

    public Task<List<string>> GetSubjectsAsync()
    {
        var result = new List<string>();
        foreach (var q in questions)
        {
            if (!string.IsNullOrEmpty(q.subject) && !result.Contains(q.subject)) result.Add(q.subject);
        }
        return Task.FromResult(result);
    }

    // ------------------------------------------------------------------
    // 同期版（テストと内部処理用）
    // ------------------------------------------------------------------

    /// <summary>問題を1件追加する。すでに同じ問題があれば false。</summary>
    public bool Add(QuestionData question)
    {
        if (question == null) return false;

        string key = question.IdentityKey;
        if (string.IsNullOrEmpty(key) || identityKeys.Contains(key)) return false;

        if (string.IsNullOrEmpty(question.id)) question.id = key.GetHashCode().ToString("x8");

        questions.Add(question);
        identityKeys.Add(key);
        return true;
    }

    public int AddRange(IEnumerable<QuestionData> items)
    {
        if (items == null) return 0;

        int added = 0;
        foreach (var item in items)
        {
            if (Add(item)) added++;
        }
        return added;
    }

    public bool Contains(QuestionData question) =>
        question != null && identityKeys.Contains(question.IdentityKey);

    public void Clear()
    {
        questions.Clear();
        identityKeys.Clear();
        initialized = false;
    }

    /// <summary>
    /// 種データ以外（AIが生成した分）をJSONで書き出す。
    /// 種データはgit管理なので上書きしない。
    /// </summary>
    public bool WriteGeneratedToDisk()
    {
        try
        {
            File.WriteAllText(SavePath, ToJson(questions.FindAll(q => !q.verified)));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Question] 保存に失敗しました: " + e.Message);
            return false;
        }
    }

    // ------------------------------------------------------------------
    // JSON 変換（サーバーとのやりとりでも同じ形式を使う）
    // ------------------------------------------------------------------

    /// JsonUtility はリストをそのまま扱えないので、入れ物で包む。
    [Serializable]
    private class QuestionList
    {
        public List<QuestionData> questions = new List<QuestionData>();
    }

    public static string ToJson(List<QuestionData> items)
    {
        return JsonUtility.ToJson(new QuestionList { questions = items ?? new List<QuestionData>() }, true);
    }

    public static List<QuestionData> FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<QuestionData>();

        var parsed = JsonUtility.FromJson<QuestionList>(json);
        return parsed != null && parsed.questions != null ? parsed.questions : new List<QuestionData>();
    }
}
