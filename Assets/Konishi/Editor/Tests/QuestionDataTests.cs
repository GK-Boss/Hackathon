using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// 問題データそのものの検査（指示書 §30）。
/// </summary>
public class QuestionDataTests
{
    [Test]
    public void 選択肢は6個()
    {
        Assert.AreEqual(6, QuestionRequest.DefaultChoiceCount, "企画書の正式仕様は6択です。");
        Assert.AreEqual(6, TestQuestions.Valid().choices.Length);
    }

    [Test]
    public void 正解インデックスが有効()
    {
        var question = TestQuestions.Valid();
        Assert.GreaterOrEqual(question.correctIndex, 0);
        Assert.Less(question.correctIndex, question.choices.Length);
        Assert.AreEqual("心拍数増加", question.CorrectChoice);
    }

    [Test]
    public void 正解インデックスが範囲外なら空文字を返す()
    {
        var question = TestQuestions.Valid();
        question.correctIndex = 99;
        Assert.AreEqual("", question.CorrectChoice);
    }

    [Test]
    public void 関連語を複数保持できる()
    {
        var question = TestQuestions.Valid();
        Assert.AreEqual(3, question.relatedTerms.Count);
        CollectionAssert.Contains(question.TermNames(), "β1受容体");
    }

    [Test]
    public void 関連度1から5を保持できる()
    {
        var question = new QuestionData { relatedTerms = new List<RelatedTerm>() };
        for (int relevance = RelatedTerm.MinRelevance; relevance <= RelatedTerm.MaxRelevance; relevance++)
        {
            var term = new RelatedTerm("語" + relevance, relevance);
            Assert.IsTrue(term.IsValid, "関連度" + relevance + "は有効なはずです。");
            question.relatedTerms.Add(term);
        }
        Assert.AreEqual(5, question.relatedTerms.Count);
    }

    [TestCase(0)]
    [TestCase(6)]
    [TestCase(-1)]
    public void 範囲外の関連度は無効(int relevance)
    {
        Assert.IsFalse(new RelatedTerm("語", relevance).IsValid);
    }

    [Test]
    public void 関連語を持っているか調べられる()
    {
        var question = TestQuestions.Valid();
        Assert.IsTrue(question.HasTerm("アドレナリン"));
        Assert.IsTrue(question.HasTerm(" アドレナリン "), "前後の空白は無視されるべきです。");
        Assert.IsFalse(question.HasTerm("インスリン"));
    }

    [Test]
    public void 問題文が同じなら同じ問題とみなす()
    {
        var a = TestQuestions.Valid("Unityとは何か");
        var b = TestQuestions.Valid("Unity とは 何か");
        Assert.AreEqual(a.IdentityKey, b.IdentityKey);
    }
}
