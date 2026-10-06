using System;
using UnityEngine;

/// <summary>
/// ChatGPTが返すJSONをそのまま受け取るための入れ物。
///
/// Tomo注: AIの出力は壊れていることがあるので、ゲーム側の QuestionData とは分けています。
///         ここで検証(Validate)を通したものだけを QuestionData に変換します。
///         JSONのキー名はフィールド名と一致させる必要があります（PromptTemplates の出力形式と対応）。
/// </summary>
[Serializable]
public class AIQuestionDto
{
    public string questionText;
    public string[] choices;
    public int correctIndex;
    public string explanation;
    public string relatedTerm;

    /// <summary>
    /// ゲームで使える内容かを検証する。企画書「AI検証」の最小版。
    /// </summary>
    /// <param name="expectedChoiceCount">期待する選択肢数（PromptTemplates.ChoiceCount）。</param>
    /// <param name="reason">失敗した理由。ログに出して原因を追えるようにする。</param>
    public bool Validate(int expectedChoiceCount, out string reason)
    {
        if (string.IsNullOrWhiteSpace(questionText))
        {
            reason = "問題文が空です。";
            return false;
        }

        if (choices == null || choices.Length != expectedChoiceCount)
        {
            reason = "選択肢が" + expectedChoiceCount + "個ではありません（" + (choices == null ? 0 : choices.Length) + "個）。";
            return false;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(choices[i]))
            {
                reason = (i + 1) + "番目の選択肢が空です。";
                return false;
            }
        }

        // 同じ選択肢が複数あると正解が一意に決まらないので弾く
        for (int i = 0; i < choices.Length; i++)
        {
            for (int j = i + 1; j < choices.Length; j++)
            {
                if (choices[i].Trim() == choices[j].Trim())
                {
                    reason = "同じ選択肢が重複しています: " + choices[i];
                    return false;
                }
            }
        }

        if (correctIndex < 0 || correctIndex >= choices.Length)
        {
            reason = "correctIndexが範囲外です: " + correctIndex;
            return false;
        }

        if (string.IsNullOrWhiteSpace(explanation))
        {
            reason = "解説が空です。";
            return false;
        }

        reason = null;
        return true;
    }

    /// <summary>検証済みのDTOをゲーム内で使う QuestionData に変換する。</summary>
    public QuestionData ToQuestionData()
    {
        // relatedTerm が空でも落ちないように、空なら問題文の代わりを入れておく
        string term = string.IsNullOrWhiteSpace(relatedTerm) ? "" : relatedTerm.Trim();
        return new QuestionData(questionText.Trim(), choices, correctIndex, explanation.Trim(), term);
    }

    /// <summary>
    /// AIの応答文字列からDTOを復元する。失敗したら null。
    /// コードブロック(```json ... ```)で包まれて返ってくることがあるため、JSON部分だけ切り出してからパースする。
    /// </summary>
    public static AIQuestionDto Parse(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent)) return null;

        int start = rawContent.IndexOf('{');
        int end = rawContent.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        string json = rawContent.Substring(start, end - start + 1);
        try
        {
            return JsonUtility.FromJson<AIQuestionDto>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[AI] JSONの解析に失敗しました: " + e.Message);
            return null;
        }
    }
}
