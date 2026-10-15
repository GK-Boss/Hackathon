using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// AIが生成した問題が、そのまま出題できる状態かを検証する。
///
/// Tomo注: AIは必ず壊れた問題を返してきます。ここが素通しになると、
///         選択肢が5個しかない問題や、正解が一意に決まらない問題がそのまま出ます。
///         「AIに判断させず、コードで検査する」ための砦です。
///
///         ここで検査できるのは【形式】だけです。
///         「書いてある内容が医学的・科学的に正しいか」はコードでは判定できません。
///         内容の正しさは人が確認し、QuestionData.verified を立ててください。
///
///         ルールを足したら、必ず AIQuestionValidatorTests にもケースを足すこと。
/// </summary>
public static class AIQuestionValidator
{
    /// <summary>
    /// 選択肢の最大文字数。ボタンに収まる長さに保つための上限。
    /// これを超える内容を出したい場合は、問題文に「A. 〜 / B. 〜」と書き、
    /// 選択肢は "A" "B" の記号にする（PromptTemplates がAIにそう指示している）。
    /// </summary>
    public const int MaxChoiceLength = 20;

    /// 問題文として成立する最低の長さ。
    public const int MinQuestionTextLength = 5;

    /// 「A." や "B)" のような記号だけの選択肢。この形式なら長さ制限を気にしなくてよい。
    private static readonly Regex LabelOnlyPattern = new Regex(@"^[A-Za-zア-ンａ-ｚＡ-Ｚ1-9１-９][\.\)．）]?$");

    /// <summary>問題単体を検証する。</summary>
    public static bool Validate(QuestionData question, int expectedChoiceCount, out string reason)
    {
        return Validate(question, expectedChoiceCount, null, null, out reason);
    }

    /// <summary>
    /// 問題を検証する。
    /// </summary>
    /// <param name="question">検証する問題。</param>
    /// <param name="expectedChoiceCount">期待する選択肢数（企画書どおり6）。</param>
    /// <param name="expectedSubject">期待する分野。渡すと分野違いを弾く。</param>
    /// <param name="existingKeys">既存問題の IdentityKey。渡すと重複を弾く。</param>
    /// <param name="reason">落ちた理由。AIへの修正指示とBUGS.mdにそのまま使えるよう具体的に返す。</param>
    public static bool Validate(
        QuestionData question,
        int expectedChoiceCount,
        string expectedSubject,
        ICollection<string> existingKeys,
        out string reason)
    {
        if (question == null)
        {
            reason = "問題がnullです。";
            return false;
        }

        // ---- 問題文 ----
        if (string.IsNullOrWhiteSpace(question.questionText))
        {
            reason = "問題文が空です。";
            return false;
        }
        if (question.questionText.Trim().Length < MinQuestionTextLength)
        {
            reason = "問題文が短すぎて問題として成立していません: " + question.questionText;
            return false;
        }

        // ---- 選択肢 ----
        if (question.choices == null || question.choices.Length != expectedChoiceCount)
        {
            reason = "選択肢が" + expectedChoiceCount + "個ではありません（"
                     + (question.choices == null ? 0 : question.choices.Length) + "個）。";
            return false;
        }

        for (int i = 0; i < question.choices.Length; i++)
        {
            string choice = question.choices[i];

            if (string.IsNullOrWhiteSpace(choice))
            {
                reason = (i + 1) + "番目の選択肢が空です。";
                return false;
            }

            if (!IsShortEnough(choice))
            {
                reason = (i + 1) + "番目の選択肢が長すぎます（" + choice.Trim().Length + "文字、上限"
                         + MaxChoiceLength + "文字）。問題文に「A. 〜」の形で書き、選択肢は記号にしてください: " + choice;
                return false;
            }
        }

        // 同じ選択肢が複数あると正解が一意に決まらない
        for (int i = 0; i < question.choices.Length; i++)
        {
            for (int j = i + 1; j < question.choices.Length; j++)
            {
                if (Normalize(question.choices[i]) == Normalize(question.choices[j]))
                {
                    reason = "同じ選択肢が重複しています: " + question.choices[i];
                    return false;
                }
            }
        }

        // ---- 正解 ----
        if (question.correctIndex < 0 || question.correctIndex >= question.choices.Length)
        {
            reason = "correctIndexが範囲外です: " + question.correctIndex;
            return false;
        }

        // ---- 解説 ----
        if (string.IsNullOrWhiteSpace(question.explanation))
        {
            reason = "解説が空です。";
            return false;
        }

        // ---- 関連語 ----
        if (question.relatedTerms == null || question.relatedTerms.Count == 0)
        {
            reason = "関連語が1つもありません。次の問題を選べなくなります。";
            return false;
        }

        var seenTerms = new HashSet<string>();
        foreach (var related in question.relatedTerms)
        {
            if (related == null || string.IsNullOrWhiteSpace(related.term))
            {
                reason = "空の関連語が含まれています。";
                return false;
            }
            if (related.relevance < RelatedTerm.MinRelevance || related.relevance > RelatedTerm.MaxRelevance)
            {
                reason = "関連度が" + RelatedTerm.MinRelevance + "〜" + RelatedTerm.MaxRelevance
                         + "の範囲外です: " + related.term + " = " + related.relevance;
                return false;
            }
            if (!seenTerms.Add(Normalize(related.term)))
            {
                reason = "同じ関連語が重複しています: " + related.term;
                return false;
            }
        }

        // ---- 分野 ----
        if (string.IsNullOrWhiteSpace(question.subject))
        {
            reason = "分野が空です。";
            return false;
        }
        if (!string.IsNullOrWhiteSpace(expectedSubject) && question.subject.Trim() != expectedSubject.Trim())
        {
            // 「薬理の問題から突然物理の問題」を防ぐ
            reason = "分野が注文と違います。注文: " + expectedSubject + " / 生成: " + question.subject;
            return false;
        }

        // ---- 記号形式の整合 ----
        if (IsLabelStyle(question.choices) && !HasLabelsInQuestionText(question))
        {
            reason = "選択肢が記号だけですが、問題文に「A. 〜」の説明がありません。";
            return false;
        }

        // ---- 既存との重複 ----
        if (existingKeys != null && existingKeys.Contains(question.IdentityKey))
        {
            reason = "既存の問題と重複しています: " + question.questionText;
            return false;
        }

        reason = null;
        return true;
    }

    /// 選択肢が上限内か。記号だけの選択肢は常に通す。
    public static bool IsShortEnough(string choice)
    {
        if (string.IsNullOrWhiteSpace(choice)) return false;

        string trimmed = choice.Trim();
        return LabelOnlyPattern.IsMatch(trimmed) || trimmed.Length <= MaxChoiceLength;
    }

    /// すべての選択肢が "A" "B" のような記号かどうか。
    public static bool IsLabelStyle(string[] choices)
    {
        if (choices == null || choices.Length == 0) return false;

        foreach (string choice in choices)
        {
            if (string.IsNullOrWhiteSpace(choice)) return false;
            if (!LabelOnlyPattern.IsMatch(choice.Trim())) return false;
        }
        return true;
    }

    /// 問題文の中に、各選択肢の記号（A. など）が登場するか。
    private static bool HasLabelsInQuestionText(QuestionData question)
    {
        foreach (string choice in question.choices)
        {
            string label = choice.Trim().TrimEnd('.', ')', '．', '）');
            if (question.questionText.IndexOf(label, System.StringComparison.OrdinalIgnoreCase) < 0) return false;
        }
        return true;
    }

    private static string Normalize(string value)
    {
        return value == null ? "" : value.Replace(" ", "").Replace("　", "").Trim().ToLowerInvariant();
    }
}
