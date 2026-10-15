using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 問題をJSONで保存・読み込みする。
///
/// Tomo注: 問題は2か所から来ます。
///           1. 種データ  Assets/Konishi/Resources/questions_seed.json（人が目視検証した固定問題。gitに入れる）
///           2. 生成分    Application.persistentDataPath/questions.json（AIが作った分。端末ごと）
///
///         デモでは種データだけを使えば、APIが落ちても安定して動きます。
///         問題を増やすときは questions_seed.json をテキストエディタで直接編集してください。
///         Unityを開かなくても編集できます。
/// </summary>
public class QuestionDatabase
{
    /// Resources から読む種データの名前（拡張子なし）。
    public const string SeedResourceName = "questions_seed";

    /// 生成した問題を書き出すファイル名。
    public const string SaveFileName = "questions.json";

    private readonly List<QuestionData> questions = new List<QuestionData>();

    /// 重複判定を速くするための索引。questions と必ず同期させること。
    private readonly HashSet<string> identityKeys = new HashSet<string>();

    public IReadOnlyList<QuestionData> All => questions;
    public int Count => questions.Count;

    /// 生成分の保存先。端末によって変わるので、ログに出して確認できるようにしている。
    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    // ------------------------------------------------------------------
    // 読み込み
    // ------------------------------------------------------------------

    /// <summary>
    /// 種データと保存済みデータの両方を読み込む。ゲーム開始時はこれを呼ぶ。
    /// </summary>
    public void LoadAll()
    {
        Clear();
        LoadSeed();
        LoadFromDisk();
    }

    /// <summary>Resources の questions_seed.json を読む。無くてもエラーにしない。</summary>
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

    /// <summary>保存済みの生成問題を読む。初回は存在しないので0件で正常。</summary>
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
    // 保存
    // ------------------------------------------------------------------

    /// <summary>
    /// 種データ以外（AIが生成した分）をJSONで書き出す。
    /// 種データはgit管理なので、上書きしないよう除外している。
    /// </summary>
    public bool SaveGeneratedToDisk()
    {
        var generated = questions.FindAll(q => !q.verified);
        try
        {
            File.WriteAllText(SavePath, ToJson(generated));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Question] 保存に失敗しました: " + e.Message);
            return false;
        }
    }

    // ------------------------------------------------------------------
    // 追加・検索
    // ------------------------------------------------------------------

    /// <summary>
    /// 問題を1件追加する。すでに同じ問題があれば false を返して追加しない。
    /// </summary>
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

    /// すでに同じ問題を持っているか。QuestionValidator の重複チェックから呼ばれる。
    public bool Contains(QuestionData question)
    {
        return question != null && identityKeys.Contains(question.IdentityKey);
    }

    /// <summary>
    /// 条件に合う問題を集める。
    /// </summary>
    /// <param name="subject">分野。null なら全分野。</param>
    /// <param name="verifiedOnly">true なら人が検証済みの問題だけ（デモ用）。</param>
    public List<QuestionData> Find(string subject, bool verifiedOnly = false)
    {
        return questions.FindAll(q =>
            (string.IsNullOrEmpty(subject) || q.subject == subject) &&
            (!verifiedOnly || q.verified));
    }

    /// この分野に問題が1問でもあるか。
    public bool HasSubject(string subject) => Find(subject).Count > 0;

    /// 登録されている分野名の一覧。分野選択画面を自動で作るときに使える。
    public List<string> Subjects()
    {
        var result = new List<string>();
        foreach (var q in questions)
        {
            if (!string.IsNullOrEmpty(q.subject) && !result.Contains(q.subject)) result.Add(q.subject);
        }
        return result;
    }

    public void Clear()
    {
        questions.Clear();
        identityKeys.Clear();
    }

    // ------------------------------------------------------------------
    // JSON 変換
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

    /// 今持っている全問題をJSON文字列にする。デバッグや書き出しに使う。
    public string ToJson() => ToJson(questions);
}
