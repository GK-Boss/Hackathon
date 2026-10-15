using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// 次の問題の選び方の検査（指示書 §30）。
///
/// Tomo注: 乱数を固定値で差し替えてテストしています。
///         本物の乱数のままだと「たまに落ちるテスト」になり、信用できなくなるためです。
/// </summary>
public class QuestionSelectorTests
{
    /// 決まった値を順に返す「乱数」。
    private static System.Func<double> Rolls(params double[] values)
    {
        int index = 0;
        return () => values[index++ % values.Length];
    }

    private static List<RelatedTerm> Terms() => new List<RelatedTerm>
    {
        new RelatedTerm("アドレナリン", 5),
        new RelatedTerm("β1受容体", 4),
        new RelatedTerm("カテコールアミン", 1)
    };

    // ---- 関連語の選択（関連度で重み付け） ----

    [Test]
    public void 関連度の重みどおりに関連語が選ばれる()
    {
        // 重みの合計は 5+4+1 = 10。
        // 0.0〜0.5 → アドレナリン、0.5〜0.9 → β1受容体、0.9〜1.0 → カテコールアミン
        Assert.AreEqual("アドレナリン", new QuestionSelector(Rolls(0.0)).ChooseRelatedTerm(Terms()).term);
        Assert.AreEqual("アドレナリン", new QuestionSelector(Rolls(0.49)).ChooseRelatedTerm(Terms()).term);
        Assert.AreEqual("β1受容体", new QuestionSelector(Rolls(0.5)).ChooseRelatedTerm(Terms()).term);
        Assert.AreEqual("β1受容体", new QuestionSelector(Rolls(0.89)).ChooseRelatedTerm(Terms()).term);
        Assert.AreEqual("カテコールアミン", new QuestionSelector(Rolls(0.95)).ChooseRelatedTerm(Terms()).term);
    }

    [Test]
    public void 関連度が高い語ほど選ばれやすい()
    {
        // 完全ランダムになっていないことの確認。企画書の「単純な完全ランダムにはしない」に対応。
        var selector = new QuestionSelector(Rolls(0.0, 0.1, 0.2, 0.3, 0.4, 0.95));
        int adrenaline = 0;
        int catechol = 0;

        for (int i = 0; i < 6; i++)
        {
            string term = selector.ChooseRelatedTerm(Terms()).term;
            if (term == "アドレナリン") adrenaline++;
            if (term == "カテコールアミン") catechol++;
        }

        Assert.Greater(adrenaline, catechol);
    }

    [Test]
    public void 関連語が無ければnull()
    {
        Assert.IsNull(new QuestionSelector(Rolls(0.5)).ChooseRelatedTerm(null));
        Assert.IsNull(new QuestionSelector(Rolls(0.5)).ChooseRelatedTerm(new List<RelatedTerm>()));
    }

    [Test]
    public void 関連度が不正な語は選ばれない()
    {
        var terms = new List<RelatedTerm> { new RelatedTerm("壊れた語", 0), new RelatedTerm("正しい語", 3) };
        Assert.AreEqual("正しい語", new QuestionSelector(Rolls(0.0)).ChooseRelatedTerm(terms).term);
    }

    // ---- 1/Nルール（指示書 §28） ----

    [Test]
    public void N0なら必ず新規生成()
    {
        Assert.IsTrue(new QuestionSelector(Rolls(0.99)).ShouldGenerateNew(0));
    }

    [Test]
    public void N1なら必ず新規生成()
    {
        // 1/1 = 100%
        Assert.IsTrue(new QuestionSelector(Rolls(0.99)).ShouldGenerateNew(1));
        Assert.IsTrue(new QuestionSelector(Rolls(0.0)).ShouldGenerateNew(1));
    }

    [Test]
    public void N2なら2分の1で新規生成()
    {
        Assert.IsTrue(new QuestionSelector(Rolls(0.49)).ShouldGenerateNew(2));
        Assert.IsFalse(new QuestionSelector(Rolls(0.5)).ShouldGenerateNew(2));
        Assert.IsFalse(new QuestionSelector(Rolls(0.99)).ShouldGenerateNew(2));
    }

    [Test]
    public void N3なら3分の1で新規生成()
    {
        Assert.IsTrue(new QuestionSelector(Rolls(0.32)).ShouldGenerateNew(3));
        Assert.IsFalse(new QuestionSelector(Rolls(0.34)).ShouldGenerateNew(3));
    }

    [Test]
    public void Nが大きいほど新規生成されにくい()
    {
        var selector = new QuestionSelector(Rolls(0.2));
        Assert.IsTrue(selector.ShouldGenerateNew(3), "1/3 = 0.333 > 0.2 なので生成する");

        selector = new QuestionSelector(Rolls(0.2));
        Assert.IsFalse(selector.ShouldGenerateNew(10), "1/10 = 0.1 < 0.2 なので生成しない");
    }

    // ---- 既存問題の選択 ----

    [Test]
    public void 未出題の問題から選ばれる()
    {
        var a = TestQuestions.Valid("問A");
        a.id = "a";
        var b = TestQuestions.Valid("問B");
        b.id = "b";

        var chosen = new QuestionSelector(Rolls(0.0)).ChooseExisting(new[] { a, b }, new HashSet<string> { "a" });
        Assert.AreEqual("b", chosen.id);
    }

    [Test]
    public void すべて出題済みならnull()
    {
        var a = TestQuestions.Valid("問A");
        a.id = "a";

        Assert.IsNull(new QuestionSelector(Rolls(0.0)).ChooseExisting(new[] { a }, new HashSet<string> { "a" }));
    }

    [Test]
    public void 候補が空ならnull()
    {
        Assert.IsNull(new QuestionSelector(Rolls(0.0)).ChooseExisting(null, new HashSet<string>()));
        Assert.IsNull(new QuestionSelector(Rolls(0.0)).ChooseExisting(new QuestionData[0], new HashSet<string>()));
    }
}
