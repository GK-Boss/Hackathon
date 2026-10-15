using System;

/// <summary>
/// 問題に紐づく関連語と、その関連度。
///
/// Tomo注: 企画書の「各関連語の5段階関連度」に対応します。
///         次の問題を選ぶとき、関連度が高い語ほど選ばれやすくなります（QuestionSelector）。
///         ボス問題の生成時には関連度を渡しません（企画書の指定）。
/// </summary>
[Serializable]
public class RelatedTerm
{
    public const int MinRelevance = 1;
    public const int MaxRelevance = 5;

    /// 関連語そのもの（例: "アドレナリン"）。
    public string term;

    /// 関連度 1〜5。5がもっとも近い。
    public int relevance = MinRelevance;

    public RelatedTerm() { }

    public RelatedTerm(string term, int relevance)
    {
        this.term = term;
        this.relevance = relevance;
    }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(term) && relevance >= MinRelevance && relevance <= MaxRelevance;

    public override string ToString() => term + " : " + relevance;
}
