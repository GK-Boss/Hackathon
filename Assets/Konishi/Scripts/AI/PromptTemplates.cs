using System.Text;

/// <summary>
/// ChatGPTへ送るプロンプトを組み立てる。
///
/// Tomo注: プロンプト文字列はすべてここに集約する。
///         AIQuestionGenerator など他のスクリプトに直接プロンプトを書かないこと。
///         （文言を直したいときに探す場所を1つに保つため）
/// </summary>
public static class PromptTemplates
{
    /// <summary>
    /// 1問あたりの選択肢数。
    /// Tomo注: 現行UI(QuestionUI / SceneUIBuilder)が5択前提のため5固定。
    ///         企画書(10/05版)の6択へ移行する際は、ここと QuestionUI・SceneUIBuilder を同時に変更すること。
    /// </summary>
    public const int ChoiceCount = 5;

    /// <summary>AIの役割と出力ルールを定義する固定文。毎リクエスト同じ内容を送る。</summary>
    public static string SystemPrompt =>
        "あなたは学習クイズの作問者です。日本語で、事実として正しい4択〜6択問題を作ります。\n" +
        "必ずJSONオブジェクトのみを返し、前後に説明文やマークダウンの装飾を付けません。";

    /// <summary>
    /// 出題プロンプトを作る。
    /// </summary>
    /// <param name="subject">分野名（例: "Unity"）。</param>
    /// <param name="chain">現在のChain数。大きいほど難しい問題を要求する。</param>
    /// <param name="relatedTerm">直前の問題の関連語。これを手がかりに知識を横に広げる。空なら分野の基礎から出題。</param>
    public static string BuildQuestionPrompt(string subject, int chain, string relatedTerm)
    {
        var builder = new StringBuilder();

        // 1. 何をしてほしいかを最初に1〜2行で書く（AIのブレを減らすため）
        builder.AppendLine("次の条件で、" + ChoiceCount + "択の単一選択問題を1問だけ作ってください。");
        builder.AppendLine();

        // 2. 出題の文脈
        builder.AppendLine("# 条件");
        builder.AppendLine("- 分野: " + subject);
        builder.AppendLine("- 難易度: " + DescribeDifficulty(chain) + "（現在のChain = " + chain + "）");
        if (!string.IsNullOrWhiteSpace(relatedTerm))
        {
            builder.AppendLine("- 直前の問題の関連語: " + relatedTerm);
            builder.AppendLine("- この関連語に隣接する知識から出題し、同じ問題の繰り返しは避けてください。");
        }
        else
        {
            builder.AppendLine("- この分野の基礎的な内容から出題してください。");
        }
        builder.AppendLine();

        // 3. 中身のルール
        builder.AppendLine("# 作問ルール");
        builder.AppendLine("- 正解はちょうど1つにしてください。");
        builder.AppendLine("- 選択肢はちょうど" + ChoiceCount + "個、すべて異なる内容にしてください。");
        builder.AppendLine("- 「すべて正しい」「正解なし」のような選択肢は使わないでください。");
        builder.AppendLine("- 誤答も、もっともらしい内容にしてください。");
        builder.AppendLine("- 解説は2〜3文で、なぜその答えになるかを説明してください。");
        builder.AppendLine("- relatedTerm には、次の問題につなげられる関連キーワードを1つだけ入れてください。");
        builder.AppendLine();

        // 4. 出力形式（JsonUtilityでパースするのでキー名は固定）
        builder.AppendLine("# 出力形式");
        builder.AppendLine("以下のキーを持つJSONオブジェクトだけを返してください。");
        builder.AppendLine("{");
        builder.AppendLine("  \"questionText\": \"問題文\",");
        builder.AppendLine("  \"choices\": [" + BuildChoicePlaceholders() + "],");
        builder.AppendLine("  \"correctIndex\": 0,");
        builder.AppendLine("  \"explanation\": \"解説\",");
        builder.AppendLine("  \"relatedTerm\": \"関連キーワード\"");
        builder.AppendLine("}");
        builder.AppendLine("correctIndex は choices の0始まりの添字です。");

        return builder.ToString();
    }

    /// Chain数を難易度の言葉に置き換える。企画書の「Chainが増えるにつれ段階的に難化」に対応。
    private static string DescribeDifficulty(int chain)
    {
        if (chain < 3) return "入門（その分野を学び始めた人が答えられる程度）";
        if (chain < 6) return "基礎（教科書の基本事項レベル）";
        if (chain < 10) return "標準（基本を理解した人向け。応用を1段階含む）";
        return "発展（深い理解が必要。細部まで問う）";
    }

    /// 出力形式の例に並べる選択肢のプレースホルダ("選択肢1", "選択肢2", ...)を作る。
    private static string BuildChoicePlaceholders()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < ChoiceCount; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append("\"選択肢").Append(i + 1).Append("\"");
        }
        return builder.ToString();
    }
}
