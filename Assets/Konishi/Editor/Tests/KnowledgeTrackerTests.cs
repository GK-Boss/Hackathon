using NUnit.Framework;

/// <summary>
/// プレイ中の知識の記録の検査（指示書 §30）。
///
/// Tomo注: ボス問題の材料になるので、ここが狂うとボス問題の中身が変わります。
/// </summary>
public class KnowledgeTrackerTests
{
    private KnowledgeTracker tracker;

    [SetUp]
    public void SetUp() => tracker = new KnowledgeTracker();

    [Test]
    public void 問題に含まれる関連語を記録できる()
    {
        var question = TestQuestions.Valid();
        question.id = "q1";
        tracker.RegisterQuestion(question);

        var counts = tracker.GetKnowledgeCounts();
        Assert.AreEqual(1, counts["アドレナリン"]);
        Assert.AreEqual(1, counts["β1受容体"]);
        Assert.AreEqual(3, counts.Count);
    }

    [Test]
    public void 複数問題から知識を集計できる()
    {
        var q1 = TestQuestions.Valid("問1");
        q1.id = "q1";
        var q2 = TestQuestions.Valid("問2");
        q2.id = "q2";

        tracker.RegisterQuestion(q1);
        tracker.RegisterQuestion(q2);

        // 両方に「アドレナリン」が含まれるので2回
        Assert.AreEqual(2, tracker.CountOf("アドレナリン"));
        Assert.AreEqual(2, tracker.QuestionCount);
    }

    [Test]
    public void 同じ問題をもう一度渡しても二重カウントしない()
    {
        // 企画書どおり、Reloadで同じ問題を再挑戦しても知識は増えない
        var question = TestQuestions.Valid();
        question.id = "q1";

        tracker.RegisterQuestion(question);
        tracker.RegisterQuestion(question);

        Assert.AreEqual(1, tracker.CountOf("アドレナリン"));
        Assert.AreEqual(1, tracker.QuestionCount);
    }

    [Test]
    public void Reloadを記録しても知識は増えない()
    {
        var question = TestQuestions.Valid();
        question.id = "q1";

        tracker.RegisterQuestion(question);
        tracker.RegisterReload(question);
        tracker.RegisterQuestion(question);

        Assert.AreEqual(1, tracker.CountOf("アドレナリン"));
    }

    [Test]
    public void 出現回数の多い順に並べられる()
    {
        var q1 = TestQuestions.Valid("問1");
        q1.id = "q1";

        var q2 = TestQuestions.Valid("問2");
        q2.id = "q2";
        q2.relatedTerms = new System.Collections.Generic.List<RelatedTerm> { new RelatedTerm("アドレナリン", 5) };

        tracker.RegisterQuestion(q1);
        tracker.RegisterQuestion(q2);

        Assert.AreEqual("アドレナリン", tracker.TermsByFrequency()[0]);
    }

    [Test]
    public void Clearで知識が消える()
    {
        var question = TestQuestions.Valid();
        question.id = "q1";
        tracker.RegisterQuestion(question);

        tracker.Clear();

        Assert.AreEqual(0, tracker.GetKnowledgeCounts().Count);
        Assert.AreEqual(0, tracker.QuestionCount);
    }

    [Test]
    public void nullを渡しても落ちない()
    {
        tracker.RegisterQuestion(null);
        tracker.RegisterReload(null);
        Assert.AreEqual(0, tracker.QuestionCount);
    }
}
