using NUnit.Framework;

/// <summary>
/// 生成の注文の検査。
///
/// Tomo注: 「引数で作る問題をある程度決められる」部分です。
///         Chain数から難易度への変換がここで決まります。
/// </summary>
public class QuestionRequestTests
{
    [Test]
    public void 分野が空なら注文として不正()
    {
        var request = new QuestionRequest("", 1);
        Assert.IsFalse(request.IsValid(out _));
    }

    [Test]
    public void 分野があれば注文として成立する()
    {
        var request = new QuestionRequest("Unity", 1);
        Assert.IsTrue(request.IsValid(out string reason), reason);
    }

    [Test]
    public void 選択肢が少なすぎたら不正()
    {
        var request = new QuestionRequest("Unity", 1) { choiceCount = 1 };
        Assert.IsFalse(request.IsValid(out _));
    }

    [TestCase(0, 1)]
    [TestCase(2, 1)]
    [TestCase(3, 2)]
    [TestCase(5, 2)]
    [TestCase(6, 3)]
    [TestCase(9, 3)]
    [TestCase(10, 4)]
    [TestCase(99, 4)]
    public void Chainが増えると難易度が上がる(int chain, int expected)
    {
        Assert.AreEqual(expected, QuestionRequest.DifficultyForChain(chain));
    }

    [Test]
    public void 難易度は1から4に収まる()
    {
        Assert.AreEqual(1, QuestionRequest.ClampDifficulty(-5));
        Assert.AreEqual(4, QuestionRequest.ClampDifficulty(99));
        Assert.AreEqual(3, QuestionRequest.ClampDifficulty(3));
    }

    [Test]
    public void Chainから注文を作れる()
    {
        var request = QuestionRequest.FromChain("Unity", 7, "SpriteRenderer");
        Assert.AreEqual("Unity", request.subject);
        Assert.AreEqual(3, request.difficulty);
        Assert.AreEqual("SpriteRenderer", request.relatedTerm);
        Assert.AreEqual(6, request.choiceCount, "企画書の正式仕様は6択です。");
    }

    [Test]
    public void 注文の内容がプロンプトに反映される()
    {
        var request = QuestionRequest.FromChain("薬理学", 10, "アドレナリン");
        string prompt = PromptTemplates.BuildQuestionPrompt(request);

        StringAssert.Contains("薬理学", prompt);
        StringAssert.Contains("アドレナリン", prompt);
        StringAssert.Contains("発展", prompt);
        StringAssert.Contains(AIQuestionValidator.MaxChoiceLength.ToString(), prompt);
        StringAssert.Contains("6択", prompt);
    }

    [Test]
    public void 重複を避けたい問題をプロンプトに渡せる()
    {
        var request = new QuestionRequest("Unity", 1)
        {
            avoidQuestions = new System.Collections.Generic.List<string> { "すでに出した問題" }
        };
        StringAssert.Contains("すでに出した問題", PromptTemplates.BuildQuestionPrompt(request));
    }

    [Test]
    public void 通常問題のプロンプトには関連語の指定が入る()
    {
        string prompt = PromptTemplates.BuildQuestionPrompt(new QuestionRequest("薬理学", 1));
        StringAssert.Contains("関連度", prompt);
        StringAssert.Contains("relatedTerms", prompt);
    }

    // ---- ボス問題 ----

    [Test]
    public void ボスの注文には知識が必要()
    {
        var empty = QuestionRequest.ForBoss("薬理学", new System.Collections.Generic.Dictionary<string, int>());
        Assert.IsFalse(empty.IsValid(out string reason));
        StringAssert.Contains("知識", reason);
    }

    [Test]
    public void ボスの注文は最高難易度になる()
    {
        var counts = new System.Collections.Generic.Dictionary<string, int> { { "アドレナリン", 3 } };
        var request = QuestionRequest.ForBoss("薬理学", counts);

        Assert.IsTrue(request.isBoss);
        Assert.AreEqual(QuestionRequest.MaxDifficulty, request.difficulty);
        Assert.IsTrue(request.IsValid(out string reason), reason);
    }

    [Test]
    public void ボスのプロンプトには出現回数が入り関連度は入らない()
    {
        // 企画書の指定: ボス生成時は関連度の数値をAIに渡さない
        var counts = new System.Collections.Generic.Dictionary<string, int>
        {
            { "アドレナリン", 3 },
            { "β1受容体", 2 }
        };
        string prompt = PromptTemplates.BuildQuestionPrompt(QuestionRequest.ForBoss("薬理学", counts));

        StringAssert.Contains("アドレナリン × 3", prompt);
        StringAssert.Contains("β1受容体 × 2", prompt);
        StringAssert.Contains("組み合わせて", prompt);
        StringAssert.DoesNotContain("手がかりにする関連語", prompt);
    }

    [Test]
    public void 修正依頼のプロンプトには落ちた理由が入る()
    {
        var request = new QuestionRequest("薬理学", 2);
        string prompt = PromptTemplates.BuildRepairPrompt(request, "{壊れたJSON}", "選択肢が6個ではありません。");

        StringAssert.Contains("選択肢が6個ではありません。", prompt);
        StringAssert.Contains("{壊れたJSON}", prompt);
    }
}
