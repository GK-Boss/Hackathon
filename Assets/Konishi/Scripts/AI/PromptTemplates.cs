using System.Collections.Generic;
using System.Text;

/// <summary>
/// AIへ送るプロンプトを組み立てる。
///
/// Tomo注: プロンプト文字列はすべてここに集約する。
///         AIQuestionGenerator など他のスクリプトに直接プロンプトを書かないこと。
///         （文言を直したいときに探す場所を1つに保つため）
///
///         出力形式を変えたら、AIQuestionGenerator.ParseResponse と AIQuestionValidator も
///         同時に直すこと。3つは必ずセットで動く。
/// </summary>
public static class PromptTemplates
{
    /// AIの役割と出力ルールを定義する固定文。毎リクエスト同じ内容を送る。
    public static string SystemPrompt =>
        "あなたは学習クイズの作問者です。日本語で、事実として正しい6択問題を作ります。\n" +
        "必ずJSONオブジェクトのみを返し、前後に説明文やマークダウンの装飾を付けません。";

    /// <summary>注文から出題プロンプトを組み立てる。通常問題とボス問題の両方に対応。</summary>
    public static string BuildQuestionPrompt(QuestionRequest request)
    {
        return request.isBoss ? BuildBossPrompt(request) : BuildNormalPrompt(request);
    }

    // ------------------------------------------------------------------
    // 通常問題
    // ------------------------------------------------------------------

    private static string BuildNormalPrompt(QuestionRequest request)
    {
        var builder = new StringBuilder();
        int choiceCount = request.choiceCount;

        builder.AppendLine("次の条件で、" + choiceCount + "択の単一選択問題を1問だけ作ってください。");
        builder.AppendLine();

        builder.AppendLine("# 条件");
        builder.AppendLine("- 分野: " + request.subject);
        builder.AppendLine("- 難易度: " + DescribeDifficulty(request.difficulty) + "（" + request.difficulty + "/4）");

        if (!string.IsNullOrWhiteSpace(request.relatedTerm))
        {
            builder.AppendLine("- 手がかりにする関連語: " + request.relatedTerm);
            builder.AppendLine("- この関連語に隣接する知識から出題してください。");
        }
        else
        {
            builder.AppendLine("- この分野の基礎的な内容から出題してください。");
        }

        if (!string.IsNullOrWhiteSpace(request.currentQuestionText))
        {
            builder.AppendLine("- 直前の問題（文脈の参考。同じ問題は作らないこと）: " + request.currentQuestionText);
        }

        // 「薬理の問題から突然物理の問題」を防ぐための念押し
        builder.AppendLine("- 分野「" + request.subject + "」から大きく外れた内容にしないでください。");

        AppendAvoidList(builder, request.avoidQuestions);
        builder.AppendLine();

        AppendCommonRules(builder, choiceCount);
        AppendOutputFormat(builder, choiceCount);

        return builder.ToString();
    }

    // ------------------------------------------------------------------
    // ボス問題
    // ------------------------------------------------------------------

    private static string BuildBossPrompt(QuestionRequest request)
    {
        var builder = new StringBuilder();
        int choiceCount = request.choiceCount;

        builder.AppendLine("次の条件で、" + choiceCount + "択の**難しい総合問題**を1問だけ作ってください。");
        builder.AppendLine("これはボス戦の問題です。通常問題より明確に難しくしてください。");
        builder.AppendLine();

        builder.AppendLine("# 条件");
        builder.AppendLine("- 分野: " + request.subject);
        builder.AppendLine("- 難易度: " + DescribeDifficulty(request.difficulty) + "（" + request.difficulty + "/4）");
        builder.AppendLine();

        // 企画書の指定どおり、関連度（1〜5）は渡さず、出現回数だけを渡す
        builder.AppendLine("# プレイヤーが今回触れた知識（語 × 出現回数）");
        builder.AppendLine("出現回数が多い語ほど、今回のプレイで中心だった知識です。");
        foreach (var pair in SortByCountDescending(request.knowledgeCounts))
        {
            builder.AppendLine("- " + pair.Key + " × " + pair.Value);
        }
        builder.AppendLine();

        builder.AppendLine("# ボス問題の作り方");
        builder.AppendLine("- 上の知識のうち**複数を組み合わせて**、1問で問う総合問題にしてください。");
        builder.AppendLine("- 1つの語の意味を聞くだけの問題にはしないでください。");
        builder.AppendLine("- 上に出ていない知識だけで作らないでください。");
        builder.AppendLine("- 既に出題された問題の焼き直しにしないでください。");

        AppendAvoidList(builder, request.avoidQuestions);
        builder.AppendLine();

        AppendCommonRules(builder, choiceCount);
        AppendOutputFormat(builder, choiceCount);

        return builder.ToString();
    }

    // ------------------------------------------------------------------
    // 修正依頼（検証に落ちたとき、1回だけ使う）
    // ------------------------------------------------------------------

    /// <summary>
    /// 検証に落ちた問題を、理由を伝えて直してもらうプロンプト。
    /// 企画書どおり、修正は1回だけ。もう一度落ちたら破棄する。
    /// </summary>
    public static string BuildRepairPrompt(QuestionRequest request, string brokenJson, string reason)
    {
        var builder = new StringBuilder();

        builder.AppendLine("先ほど作ってもらった問題が、次の理由で不合格でした。");
        builder.AppendLine();
        builder.AppendLine("# 不合格の理由");
        builder.AppendLine(reason);
        builder.AppendLine();
        builder.AppendLine("# 作り直す対象");
        builder.AppendLine(brokenJson);
        builder.AppendLine();
        builder.AppendLine("この理由だけを直して、同じ形式のJSONで作り直してください。");
        builder.AppendLine("問題の題材はできるだけ変えないでください。");
        builder.AppendLine();

        AppendCommonRules(builder, request.choiceCount);
        AppendOutputFormat(builder, request.choiceCount);

        return builder.ToString();
    }

    // ------------------------------------------------------------------
    // 共通部分
    // ------------------------------------------------------------------

    private static void AppendAvoidList(StringBuilder builder, List<string> avoidQuestions)
    {
        if (avoidQuestions == null || avoidQuestions.Count == 0) return;

        builder.AppendLine("- 次の問題と重複しないようにしてください。");
        foreach (string avoid in avoidQuestions)
        {
            if (!string.IsNullOrWhiteSpace(avoid)) builder.AppendLine("  - " + avoid);
        }
    }

    private static void AppendCommonRules(StringBuilder builder, int choiceCount)
    {
        builder.AppendLine("# 作問ルール");
        builder.AppendLine("- 正解はちょうど1つにしてください。残り" + (choiceCount - 1) + "個は明確に誤りにしてください。");
        builder.AppendLine("- 選択肢はちょうど" + choiceCount + "個、すべて異なる内容にしてください。");
        builder.AppendLine("- 「すべて正しい」「正解なし」のような選択肢は使わないでください。");
        builder.AppendLine("- 誤答も、もっともらしい内容にしてください。");
        builder.AppendLine("- 解説は2〜3文で、なぜその答えになるかを説明してください。");
        builder.AppendLine();

        builder.AppendLine("# 関連語（必須）");
        builder.AppendLine("- この問題に関係する語を2〜5個挙げ、それぞれに関連度を"
                           + RelatedTerm.MinRelevance + "〜" + RelatedTerm.MaxRelevance + "で付けてください。");
        builder.AppendLine("- 関連度" + RelatedTerm.MaxRelevance + "はこの問題の中心となる語、"
                           + RelatedTerm.MinRelevance + "は周辺的な語です。");
        builder.AppendLine("- 同じ語を2回入れないでください。");
        builder.AppendLine("- 次の問題はこの関連語から選ばれるので、学習がつながる語を選んでください。");
        builder.AppendLine();

        builder.AppendLine("# 選択肢の長さ（重要）");
        builder.AppendLine("- 選択肢は**" + AIQuestionValidator.MaxChoiceLength + "文字以内**の短い語句にしてください。単語が理想です。");
        builder.AppendLine("- 文章での選択が避けられない場合だけ、次の形にしてください。");
        builder.AppendLine("  1. 問題文の中に「A. 〜」「B. 〜」と選択肢の内容を書く");
        builder.AppendLine("  2. choices には \"A\" \"B\" \"C\" のような記号だけを入れる");
        builder.AppendLine("- どちらの形でも、" + AIQuestionValidator.MaxChoiceLength + "文字を超える選択肢は不合格になります。");
        builder.AppendLine();
    }

    private static void AppendOutputFormat(StringBuilder builder, int choiceCount)
    {
        builder.AppendLine("# 出力形式");
        builder.AppendLine("以下のキーを持つJSONオブジェクトだけを返してください。");
        builder.AppendLine("{");
        builder.AppendLine("  \"questionText\": \"問題文\",");
        builder.AppendLine("  \"choices\": [" + BuildChoicePlaceholders(choiceCount) + "],");
        builder.AppendLine("  \"correctIndex\": 0,");
        builder.AppendLine("  \"explanation\": \"解説\",");
        builder.AppendLine("  \"relatedTerms\": [");
        builder.AppendLine("    { \"term\": \"関連語1\", \"relevance\": 5 },");
        builder.AppendLine("    { \"term\": \"関連語2\", \"relevance\": 3 }");
        builder.AppendLine("  ]");
        builder.AppendLine("}");
        builder.AppendLine("correctIndex は choices の0始まりの添字です。");
    }

    /// 難易度の数値を言葉に置き換える。企画書の「Chainが増えるにつれ段階的に難化」に対応。
    public static string DescribeDifficulty(int difficulty)
    {
        switch (QuestionRequest.ClampDifficulty(difficulty))
        {
            case 1: return "入門（その分野を学び始めた人が答えられる程度）";
            case 2: return "基礎（教科書の基本事項レベル）";
            case 3: return "標準（基本を理解した人向け。応用を1段階含む）";
            default: return "発展（深い理解が必要。複数の知識を組み合わせる）";
        }
    }

    private static List<KeyValuePair<string, int>> SortByCountDescending(Dictionary<string, int> counts)
    {
        var list = new List<KeyValuePair<string, int>>();
        if (counts == null) return list;

        list.AddRange(counts);
        list.Sort((a, b) =>
        {
            int diff = b.Value.CompareTo(a.Value);
            return diff != 0 ? diff : string.Compare(a.Key, b.Key, System.StringComparison.Ordinal);
        });
        return list;
    }

    private static string BuildChoicePlaceholders(int choiceCount)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < choiceCount; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append("\"選択肢").Append(i + 1).Append("\"");
        }
        return builder.ToString();
    }
}
