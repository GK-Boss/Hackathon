using System;
using System.Collections.Generic;

/// <summary>
/// 次の通常問題を「どう決めるか」だけを持つクラス。
///
/// Tomo注: 企画書の次の2つを実装しています。
///           1. 関連語を関連度に応じて選ぶ（完全ランダムにはしない）
///           2. その関連語に紐づく問題数を N として、1/N の確率で新規生成
///
///         乱数を外から渡せるようにしてあるので、テストでは結果を固定できます。
///         （乱数が入ったままだとテストが「たまに落ちる」状態になるため）
/// </summary>
public class QuestionSelector
{
    /// 0.0以上1.0未満を返す乱数。テストでは固定値を渡す。
    private readonly Func<double> random;

    public QuestionSelector(Func<double> randomSource = null)
    {
        random = randomSource ?? NewDefaultRandom();
    }

    private static Func<double> NewDefaultRandom()
    {
        var rng = new Random();
        return rng.NextDouble;
    }

    /// <summary>
    /// 関連度で重み付けして関連語を1つ選ぶ。
    /// 関連度5の語は関連度1の語より5倍選ばれやすい。
    /// </summary>
    public RelatedTerm ChooseRelatedTerm(IReadOnlyList<RelatedTerm> terms)
    {
        if (terms == null || terms.Count == 0) return null;

        int totalWeight = 0;
        foreach (var term in terms)
        {
            if (term != null && term.IsValid) totalWeight += term.relevance;
        }
        if (totalWeight <= 0) return null;

        // 0〜totalWeight の範囲を、関連度の大きさぶんだけ各語に割り当てて抽選する
        double roll = random() * totalWeight;
        int cumulative = 0;

        foreach (var term in terms)
        {
            if (term == null || !term.IsValid) continue;

            cumulative += term.relevance;
            if (roll < cumulative) return term;
        }

        // 丸め誤差で最後まで抜けた場合の保険
        return terms[terms.Count - 1];
    }

    /// <summary>
    /// 新規生成すべきかを決める。企画書の1/Nルール。
    ///
    ///   N = 0 → 必ず新規生成（その関連語の問題がまだ無い）
    ///   N = 1 → 1/1 = 100% 新規生成
    ///   N = 2 → 1/2
    ///   N = 3 → 1/3
    /// </summary>
    public bool ShouldGenerateNew(int existingCount)
    {
        if (existingCount <= 0) return true;
        return random() < 1.0 / existingCount;
    }

    /// <summary>
    /// 既存問題から1問選ぶ。まだ出していない問題を優先する。
    /// すべて出し終えていたら null を返す（呼び出し側が出題済みを解除する）。
    /// </summary>
    public QuestionData ChooseExisting(IReadOnlyList<QuestionData> candidates, ICollection<string> usedIds)
    {
        if (candidates == null || candidates.Count == 0) return null;

        var unused = new List<QuestionData>();
        foreach (var question in candidates)
        {
            if (question != null && (usedIds == null || !usedIds.Contains(question.id))) unused.Add(question);
        }

        if (unused.Count == 0) return null;

        int index = (int)(random() * unused.Count);
        if (index >= unused.Count) index = unused.Count - 1;   // random() が 1.0 を返した場合の保険
        return unused[index];
    }
}
