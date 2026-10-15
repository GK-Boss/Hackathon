using System.Collections.Generic;

/// <summary>
/// そのプレイ中にどんな知識に触れたかを記録する。ボス問題の生成材料になる。
///
/// Tomo注: 記録するのは「正解したか」ではなく「その問題に触れたか」です。
///         企画書どおり、Reloadで同じ問題をもう一度出しても二重カウントしません。
///
///         例）アドレナリン×3、β1受容体×2 のように貯まり、
///             ボス問題生成時にこの回数をAIへ渡します（関連度は渡しません）。
/// </summary>
public class KnowledgeTracker
{
    /// 関連語 → 出現回数。
    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    /// すでに数えた問題のid。Reloadでの二重カウントを防ぐ。
    private readonly HashSet<string> countedQuestionIds = new HashSet<string>();

    /// この攻略で触れた問題数。
    public int QuestionCount => countedQuestionIds.Count;

    /// <summary>
    /// 問題に触れたことを記録する。
    /// 同じ問題を2回渡しても、2回目は数えない（Reload対策）。
    /// </summary>
    public void RegisterQuestion(QuestionData question)
    {
        if (question == null) return;

        string key = string.IsNullOrEmpty(question.id) ? question.IdentityKey : question.id;
        if (string.IsNullOrEmpty(key)) return;

        // すでに数えた問題なら何もしない。これがReloadの二重カウント防止そのもの。
        if (!countedQuestionIds.Add(key)) return;

        foreach (string term in question.TermNames())
        {
            counts.TryGetValue(term, out int current);
            counts[term] = current + 1;
        }
    }

    /// <summary>
    /// Reloadで同じ問題を再挑戦したときに呼ぶ。
    /// 企画書どおり、追加の知識取得としては数えないので、記録は増えない。
    /// </summary>
    public void RegisterReload(QuestionData question)
    {
        // 何もしない。RegisterQuestion がidで重複を弾くため、呼んでも結果は変わらない。
        // 「呼び忘れではなく、意図して何もしていない」ことを示すためにメソッドを残している。
    }

    /// <summary>関連語ごとの出現回数。ボス問題生成へ渡す。</summary>
    public Dictionary<string, int> GetKnowledgeCounts() => new Dictionary<string, int>(counts);

    /// 出現回数の多い順に関連語を返す。プロンプトに並べるときに使う。
    public List<string> TermsByFrequency()
    {
        var terms = new List<string>(counts.Keys);
        terms.Sort((a, b) =>
        {
            int diff = counts[b].CompareTo(counts[a]);
            return diff != 0 ? diff : string.Compare(a, b, System.StringComparison.Ordinal);
        });
        return terms;
    }

    public int CountOf(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return 0;
        counts.TryGetValue(term.Trim(), out int value);
        return value;
    }

    /// 攻略が終わったらリセットする。知識は1回の攻略の中だけで持つ。
    public void Clear()
    {
        counts.Clear();
        countedQuestionIds.Clear();
    }
}
