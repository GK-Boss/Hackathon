using System;
using System.Collections.Generic;

/// <summary>
/// 「どんな問題を作ってほしいか」の注文。AIQuestionGenerator に渡す。
///
/// Tomo注: 生成の条件をここに集めることで、呼び出し側は1行で注文できます。
///         条件を増やしたくなったらこのクラスにフィールドを足してください。
///         PromptTemplates がこれを読んでプロンプト文に変換します。
/// </summary>
[Serializable]
public class QuestionRequest
{
    /// <summary>
    /// 選択肢の数。企画書(1005版)どおり6択。
    /// Tomo注: UI・問題データ・プロンプト・検証はすべてこの値を見ています。
    ///         ここを変えたら Create Scene UI を実行し直し、テストを回してください。
    /// </summary>
    public const int DefaultChoiceCount = 6;

    public const int MinDifficulty = 1;
    public const int MaxDifficulty = 4;

    /// ボス問題・最終問題の難易度。通常問題より難しくする。
    public const int BossDifficulty = MaxDifficulty;

    /// 分野名。必須。「薬理の問題から突然物理の問題」にならないよう、必ず指定する。
    public string subject;

    /// 難易度 1〜4。
    public int difficulty = MinDifficulty;

    /// 選択肢の数。
    public int choiceCount = DefaultChoiceCount;

    /// 手がかりにする関連語。空なら分野の基礎から出題する。
    public string relatedTerm;

    /// 参考として渡す現在の問題文。文脈を保つために使う。
    public string currentQuestionText;

    /// 作らないでほしい問題文（既出のもの）。重複を避けるために渡す。
    public List<string> avoidQuestions = new List<string>();

    /// <summary>
    /// ボス問題かどうか。true のとき、そのプレイ中に触れた知識から新規に難問を作る。
    /// </summary>
    public bool isBoss;

    /// <summary>
    /// ボス問題用。そのプレイ中に登場した関連語と出現回数（例: アドレナリン×3）。
    /// 企画書の指定により、ボス生成時は関連度ではなく出現回数を渡す。
    /// </summary>
    public Dictionary<string, int> knowledgeCounts = new Dictionary<string, int>();

    public QuestionRequest() { }

    public QuestionRequest(string subject, int difficulty, string relatedTerm = null)
    {
        this.subject = subject;
        this.difficulty = ClampDifficulty(difficulty);
        this.relatedTerm = relatedTerm;
    }

    /// <summary>通常問題の注文を、Chain数から難易度を決めて作る。</summary>
    public static QuestionRequest FromChain(string subject, int chain, string relatedTerm = null)
    {
        return new QuestionRequest(subject, DifficultyForChain(chain), relatedTerm);
    }

    /// <summary>
    /// ボス問題の注文を作る。そのプレイ中に触れた知識とその回数を渡す。
    /// 関連度（1〜5）はここでは渡さない。企画書の指定。
    /// </summary>
    public static QuestionRequest ForBoss(string subject, Dictionary<string, int> knowledgeCounts)
    {
        return new QuestionRequest(subject, BossDifficulty)
        {
            isBoss = true,
            knowledgeCounts = knowledgeCounts ?? new Dictionary<string, int>()
        };
    }

    /// Chain数 → 難易度1〜4。企画書の「Chainが増えるにつれ段階的に難化」に対応。
    public static int DifficultyForChain(int chain)
    {
        if (chain < 3) return 1;
        if (chain < 6) return 2;
        if (chain < 10) return 3;
        return MaxDifficulty;
    }

    public static int ClampDifficulty(int difficulty)
    {
        if (difficulty < MinDifficulty) return MinDifficulty;
        if (difficulty > MaxDifficulty) return MaxDifficulty;
        return difficulty;
    }

    /// 注文として成立しているか。
    public bool IsValid(out string reason)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            reason = "分野(subject)が指定されていません。";
            return false;
        }
        if (choiceCount < 2)
        {
            reason = "選択肢の数が少なすぎます: " + choiceCount;
            return false;
        }
        if (isBoss && (knowledgeCounts == null || knowledgeCounts.Count == 0))
        {
            reason = "ボス問題にはそのプレイ中の知識が必要ですが、1つも記録されていません。";
            return false;
        }
        reason = null;
        return true;
    }
}
