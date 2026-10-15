using System;
using System.Collections.Generic;

/// <summary>
/// 問題そのもののデータ。
///
/// Tomo注: MonoBehaviourでもScriptableObjectでもない、ただのクラスです。
///         サーバーとのやりとりと保存はJSONで行うので、フィールドはすべてpublicにしてあります
///         （UnityのJsonUtilityの制約）。
///
///         ここにロジックを足さないこと。
///           検証 → AIQuestionValidator
///           保存 → IQuestionRepository
///           出題順 → QuestionSelector / QuestionManager
///
///         ゲーム側はこのクラスを受け取れば、表示・判定・解説表示ができます。
/// </summary>
[Serializable]
public class QuestionData
{
    /// 問題の識別子。重複判定と保存に使う。空なら保存時に自動で付く。
    public string id;

    /// 分野名（例: "薬理学"）。
    public string subject;

    /// 難易度 1〜4。1〜3が通常進行、4は高難度。
    public int difficulty = 1;

    public string questionText;

    /// <summary>
    /// 選択肢。企画書どおり必ず6個（QuestionRequest.DefaultChoiceCount）。
    /// 短く保つこと（AIQuestionValidator.MaxChoiceLength 文字以内）。
    /// 長い文が必要な場合は、問題文に「A. 〜 / B. 〜」と書き、ここには "A" "B" の記号だけを入れる。
    /// </summary>
    public string[] choices;

    /// choices の0始まりの添字。
    public int correctIndex;

    /// <summary>
    /// 関連語とその関連度。次の問題を選ぶ手がかりになる。
    /// 最低1つ必要（AIQuestionValidator が検証する）。
    /// </summary>
    public List<RelatedTerm> relatedTerms = new List<RelatedTerm>();

    public string explanation;

    /// 人が目視で正しさを確認したか。内容の正しさはコードでは判定できないため、人が立てる。
    public bool verified;

    /// JsonUtility 用。引数なしのコンストラクタを消さないこと。
    public QuestionData() { }

    public QuestionData(string questionText, string[] choices, int correctIndex, string explanation, params RelatedTerm[] relatedTerms)
    {
        this.questionText = questionText;
        this.choices = choices;
        this.correctIndex = correctIndex;
        this.explanation = explanation;
        this.relatedTerms = relatedTerms != null ? new List<RelatedTerm>(relatedTerms) : new List<RelatedTerm>();
    }

    /// 正解の選択肢の文字列。範囲外なら空文字。
    public string CorrectChoice =>
        choices != null && correctIndex >= 0 && correctIndex < choices.Length ? choices[correctIndex] : "";

    /// 関連語の文字列だけを取り出す。KnowledgeTracker が使う。
    public List<string> TermNames()
    {
        var names = new List<string>();
        if (relatedTerms == null) return names;

        foreach (var related in relatedTerms)
        {
            if (related != null && !string.IsNullOrWhiteSpace(related.term)) names.Add(related.term.Trim());
        }
        return names;
    }

    /// 指定した関連語を持っているか。
    public bool HasTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || relatedTerms == null) return false;

        foreach (var related in relatedTerms)
        {
            if (related != null && string.Equals(related.term?.Trim(), term.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    /// <summary>
    /// 重複判定に使うキー。問題文の空白を詰めて小文字化したもの。
    /// 「同じ内容を別の言い回しで作ってしまった」までは検出できない点に注意。
    /// </summary>
    public string IdentityKey =>
        string.IsNullOrWhiteSpace(questionText) ? "" : questionText.Replace(" ", "").Replace("　", "").Trim().ToLowerInvariant();

    public override string ToString() => "[" + subject + "/Lv" + difficulty + "] " + questionText;
}
