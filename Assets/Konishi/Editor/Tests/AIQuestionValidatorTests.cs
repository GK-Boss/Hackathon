using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// 生成された問題の検証ルールの検査（指示書 §30）。
///
/// Tomo注: ここが素通しになると、壊れた問題がそのまま画面に出ます。
///         検証ルールを足したら、必ずここにもケースを足してください。
///
///         なお、ここで見ているのは【形式】だけです。
///         「書いてある内容が正しいか」はコードでは判定できないので、人が確認します。
/// </summary>
public class AIQuestionValidatorTests
{
    private const int ChoiceCount = TestQuestions.ChoiceCount;

    private static bool Validate(QuestionData question, out string reason) =>
        AIQuestionValidator.Validate(question, ChoiceCount, out reason);

    [Test]
    public void 正常な問題は通る()
    {
        Assert.IsTrue(Validate(TestQuestions.Valid(), out string reason), reason);
    }

    [Test]
    public void nullは弾く()
    {
        Assert.IsFalse(Validate(null, out _));
    }

    // ---- 6択 ----

    [TestCase(5)]
    [TestCase(4)]
    [TestCase(7)]
    public void 選択肢が6個でなければ弾く(int count)
    {
        var question = TestQuestions.Valid();
        question.choices = new string[count];
        for (int i = 0; i < count; i++) question.choices[i] = "選択肢" + i;

        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("6個ではありません", reason);
    }

    // ---- 選択肢 ----

    [Test]
    public void 選択肢が空なら弾く()
    {
        var question = TestQuestions.Valid();
        question.choices[2] = "   ";
        Assert.IsFalse(Validate(question, out _));
    }

    [Test]
    public void 選択肢が重複していたら弾く()
    {
        var question = TestQuestions.Valid();
        question.choices[3] = question.choices[0];
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("重複", reason);
    }

    [Test]
    public void 長すぎる選択肢は弾く()
    {
        var question = TestQuestions.Valid();
        question.choices[1] = new string('あ', AIQuestionValidator.MaxChoiceLength + 1);
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("長すぎます", reason);
    }

    [Test]
    public void 上限ちょうどの選択肢は通る()
    {
        var question = TestQuestions.Valid();
        question.choices[1] = new string('あ', AIQuestionValidator.MaxChoiceLength);
        Assert.IsTrue(Validate(question, out string reason), reason);
    }

    [Test]
    public void 記号だけの選択肢は長さ制限を受けない()
    {
        var question = TestQuestions.Valid();
        question.questionText = "正しいのはどれ？ A. 心拍数が上がる B. 下がる C. 変化なし D. 不定 E. 停止 F. 不明";
        question.choices = new[] { "A", "B", "C", "D", "E", "F" };
        question.correctIndex = 0;

        Assert.IsTrue(Validate(question, out string reason), reason);
    }

    [Test]
    public void 記号だけなのに問題文に説明が無ければ弾く()
    {
        var question = TestQuestions.Valid();
        question.questionText = "正しいのはどれか";
        question.choices = new[] { "A", "B", "C", "D", "E", "F" };

        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("問題文に", reason);
    }

    // ---- 正解 ----

    [TestCase(-1)]
    [TestCase(6)]
    [TestCase(99)]
    public void 不正なcorrectIndexを弾く(int index)
    {
        var question = TestQuestions.Valid();
        question.correctIndex = index;
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("correctIndex", reason);
    }

    // ---- 問題文・解説 ----

    [Test]
    public void 問題文が空なら弾く()
    {
        var question = TestQuestions.Valid();
        question.questionText = "  ";
        Assert.IsFalse(Validate(question, out _));
    }

    [Test]
    public void 問題文が短すぎれば弾く()
    {
        var question = TestQuestions.Valid();
        question.questionText = "は？";
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("成立していません", reason);
    }

    [Test]
    public void 解説なしを弾く()
    {
        var question = TestQuestions.Valid();
        question.explanation = null;
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("解説", reason);
    }

    // ---- 関連語 ----

    [Test]
    public void 関連語なしを弾く()
    {
        var question = TestQuestions.Valid();
        question.relatedTerms = new List<RelatedTerm>();
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("関連語", reason);
    }

    [Test]
    public void 関連語がnullなら弾く()
    {
        var question = TestQuestions.Valid();
        question.relatedTerms = null;
        Assert.IsFalse(Validate(question, out _));
    }

    [TestCase(0)]
    [TestCase(6)]
    [TestCase(-1)]
    public void 不正な関連度を弾く(int relevance)
    {
        var question = TestQuestions.Valid();
        question.relatedTerms[1].relevance = relevance;
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("関連度", reason);
    }

    [Test]
    public void 関連語の重複を弾く()
    {
        var question = TestQuestions.Valid();
        question.relatedTerms[1].term = question.relatedTerms[0].term;
        Assert.IsFalse(Validate(question, out string reason));
        StringAssert.Contains("関連語が重複", reason);
    }

    [Test]
    public void 空の関連語を弾く()
    {
        var question = TestQuestions.Valid();
        question.relatedTerms[0].term = "  ";
        Assert.IsFalse(Validate(question, out _));
    }

    // ---- 分野 ----

    [Test]
    public void 分野が空なら弾く()
    {
        var question = TestQuestions.Valid();
        question.subject = "";
        Assert.IsFalse(Validate(question, out _));
    }

    [Test]
    public void 注文と違う分野を弾く()
    {
        // 「薬理の問題から突然物理の問題」を防ぐ
        var question = TestQuestions.Valid(subject: "物理学");
        Assert.IsFalse(AIQuestionValidator.Validate(question, ChoiceCount, "薬理学", null, out string reason));
        StringAssert.Contains("分野が注文と違います", reason);
    }

    [Test]
    public void 注文どおりの分野は通る()
    {
        var question = TestQuestions.Valid(subject: "薬理学");
        Assert.IsTrue(AIQuestionValidator.Validate(question, ChoiceCount, "薬理学", null, out string reason), reason);
    }

    // ---- 重複 ----

    [Test]
    public void 既存問題と重複していたら弾く()
    {
        var existing = new List<string> { TestQuestions.Valid().IdentityKey };
        Assert.IsFalse(AIQuestionValidator.Validate(TestQuestions.Valid(), ChoiceCount, null, existing, out string reason));
        StringAssert.Contains("重複", reason);
    }

    [Test]
    public void 既存に無い問題は通る()
    {
        var existing = new List<string> { TestQuestions.Valid("別の問題文です").IdentityKey };
        Assert.IsTrue(AIQuestionValidator.Validate(TestQuestions.Valid(), ChoiceCount, null, existing, out string reason), reason);
    }
}
