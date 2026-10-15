using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 問題システムの窓口の検査（指示書 §30、§34）。
///
/// Tomo注: ここが見ているのは2つです。
///           1. ゲーム側が QuestionManager だけで問題を取得できること
///           2. AIが無くても（APIキー未設定でも）デモが回ること
///
///         デモ当日にAPIが落ちても遊べる、という前提がここで守られています。
/// </summary>
public class QuestionManagerTests
{
    private GameObject host;
    private QuestionManager manager;
    private LocalJsonQuestionRepository repository;
    private string tempPath;

    [SetUp]
    public void SetUp()
    {
        tempPath = Path.Combine(Path.GetTempPath(), "qm_test_" + System.Guid.NewGuid().ToString("N") + ".json");

        host = new GameObject("QuestionManagerHost");
        manager = host.AddComponent<QuestionManager>();

        // 端末に残っている生成済み問題に左右されないよう、種データだけに揃える
        repository = new LocalJsonQuestionRepository(tempPath);
        repository.LoadSeed();

        Configure(aiClient: null);
    }

    /// 部品を差し替えて初期化する。乱数は固定して結果をぶれさせない。
    private void Configure(IQuestionAIClient aiClient)
    {
        manager.Configure(
            repository,
            new QuestionSelector(() => 0.0),
            new AIQuestionGenerator(aiClient, false),
            new KnowledgeTracker());
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(host);
        if (File.Exists(tempPath)) File.Delete(tempPath);
    }

    // ---- 通常問題 ----

    [TestCase("Unity")]
    [TestCase("一般教養")]
    public void AIなしでも問題を取得できる(string subject)
    {
        manager.StartRun(subject);
        var question = manager.GetNextQuestion();

        Assert.IsNotNull(question);
        Assert.IsNotEmpty(question.questionText);
        Assert.AreEqual(subject, question.subject);
    }

    [Test]
    public void 取得した問題はそのまま表示判定できる形になっている()
    {
        // 指示書「ゲーム側がQuestionDataを受け取れば、表示・判定・解説表示できる」
        manager.StartRun("Unity");
        var question = manager.GetNextQuestion();

        Assert.AreEqual(QuestionRequest.DefaultChoiceCount, question.choices.Length);
        Assert.IsNotEmpty(question.CorrectChoice);
        Assert.IsNotEmpty(question.explanation);
        Assert.IsNotEmpty(question.relatedTerms);
    }

    [Test]
    public void 進み続けても問題が尽きない()
    {
        manager.StartRun("Unity");
        for (int i = 0; i < 30; i++)
        {
            Assert.IsNotNull(manager.GetNextQuestion(), i + "問目で問題が無くなりました。");
        }
    }

    [Test]
    public void 出題される問題はすべて検証を通る()
    {
        manager.StartRun("Unity");
        for (int i = 0; i < 20; i++)
        {
            var question = manager.GetNextQuestion();
            Assert.IsTrue(AIQuestionValidator.Validate(question, QuestionRequest.DefaultChoiceCount, out string reason),
                "出題できない問題が出ました: " + question + " / " + reason);
        }
    }

    [Test]
    public void 一周するまで同じ問題は出ない()
    {
        manager.StartRun("Unity");
        int total = repository.GetQuestionsBySubjectAsync("Unity").GetAwaiter().GetResult().Count;
        Assert.Greater(total, 1, "検査には2問以上必要です。");

        var seen = new HashSet<string>();
        for (int i = 0; i < total; i++)
        {
            Assert.IsTrue(seen.Add(manager.GetNextQuestion().id), "一周する前に同じ問題が出ました。");
        }
    }

    [Test]
    public void 攻略を始め直すと出題済みがリセットされる()
    {
        manager.StartRun("Unity");
        manager.GetNextQuestion();
        manager.GetNextQuestion();
        Assert.AreEqual(2, manager.AskedCount);

        manager.StartRun("Unity");
        Assert.AreEqual(0, manager.AskedCount);
    }

    [Test]
    public void 存在しない分野でも問題が出る()
    {
        // デモ中に分野名を打ち間違えても、何も出ないより全分野から出したほうがよい
        manager.StartRun("存在しない分野");
        Assert.IsNotNull(manager.GetNextQuestion());
    }

    [Test]
    public void StartRunから戻った時点で出題できる状態になっている()
    {
        // Tomo注: 読み込みの待ち方を変えたときに、ここが最初に落ちます。
        //         ローカル実装は同期で終わるので、待たなくても候補が揃っているはず。
        manager.StartRun("Unity");

        Assert.IsTrue(manager.IsReady, "出題候補の読み込みが終わっていません。");
        Assert.IsNotNull(manager.GetNextQuestion());
    }

    [Test]
    public void 攻略を何度も始め直しても問題が取得できる()
    {
        for (int i = 0; i < 5; i++)
        {
            manager.StartRun("Unity");
            Assert.IsNotNull(manager.GetNextQuestion(), (i + 1) + "回目の攻略で問題が出ませんでした。");
        }
    }

    // ---- 知識の記録 ----

    [Test]
    public void 回答すると知識が記録される()
    {
        manager.StartRun("Unity");
        var question = manager.GetNextQuestion();
        manager.RegisterQuestionAnswered(question, true);

        Assert.Greater(manager.KnowledgeCounts.Count, 0);
    }

    [Test]
    public void 不正解でも知識として記録される()
    {
        // 企画書どおり「正解したか」ではなく「触れたか」で記録する
        manager.StartRun("Unity");
        var question = manager.GetNextQuestion();
        manager.RegisterQuestionAnswered(question, false);

        Assert.Greater(manager.KnowledgeCounts.Count, 0);
    }

    [Test]
    public void Reloadでは知識が二重カウントされない()
    {
        manager.StartRun("Unity");
        var question = manager.GetNextQuestion();
        string term = question.TermNames()[0];

        manager.RegisterQuestionAnswered(question, false);
        manager.RegisterQuestionReloaded(question);
        manager.RegisterQuestionAnswered(question, true);

        Assert.AreEqual(1, manager.KnowledgeCounts[term]);
    }

    [Test]
    public void 攻略を始め直すと知識が消える()
    {
        manager.StartRun("Unity");
        manager.RegisterQuestionAnswered(manager.GetNextQuestion(), true);
        Assert.Greater(manager.KnowledgeCounts.Count, 0);

        manager.StartRun("Unity");
        Assert.AreEqual(0, manager.KnowledgeCounts.Count);
    }

    // ---- ボス問題 ----

    [Test]
    public void 知識が無ければボス問題は作れない()
    {
        Configure(new FakeAIClient(TestQuestions.ValidJson));
        manager.StartRun("薬理学");

        Assert.IsNull(manager.GenerateBossQuestion().GetAwaiter().GetResult());
    }

    [Test]
    public void 今回の知識からボス問題を生成できる()
    {
        Configure(new FakeAIClient(TestQuestions.ValidJson));
        manager.StartRun("薬理学");

        // 知識を貯める
        var played = TestQuestions.Valid("今回解いた問題", "薬理学");
        played.id = "played-1";
        manager.RegisterQuestionAnswered(played, true);

        var boss = manager.GenerateBossQuestion().GetAwaiter().GetResult();

        Assert.IsNotNull(boss, "ボス問題が生成されませんでした。");
        Assert.AreEqual(QuestionRequest.DefaultChoiceCount, boss.choices.Length);
        Assert.AreEqual("薬理学", boss.subject);
    }

    [Test]
    public void ボス問題は通常問題の使い回しではない()
    {
        Configure(new FakeAIClient(TestQuestions.ValidJson));
        manager.StartRun("薬理学");

        var played = TestQuestions.Valid("今回解いた問題", "薬理学");
        played.id = "played-1";
        manager.RegisterQuestionAnswered(played, true);

        var boss = manager.GenerateBossQuestion().GetAwaiter().GetResult();
        Assert.AreNotEqual(played.questionText, boss.questionText);
    }

    [Test]
    public void 生成に失敗してもボス問題はnullで返りゲームは止まらない()
    {
        Configure(new FakeAIClient("作れませんでした", "やはり作れません"));
        manager.StartRun("薬理学");

        var played = TestQuestions.Valid("今回解いた問題", "薬理学");
        played.id = "played-1";
        manager.RegisterQuestionAnswered(played, true);

        Assert.IsNull(manager.GenerateBossQuestion().GetAwaiter().GetResult());
    }

    [Test]
    public void 生成したボス問題は保存される()
    {
        // 企画書どおり、使っても使わなくてもDBに残す
        Configure(new FakeAIClient(TestQuestions.ValidJson));
        manager.StartRun("薬理学");

        var played = TestQuestions.Valid("今回解いた問題", "薬理学");
        played.id = "played-1";
        manager.RegisterQuestionAnswered(played, true);

        int before = repository.Count;
        manager.GenerateBossQuestion().GetAwaiter().GetResult();

        Assert.AreEqual(before + 1, repository.Count, "生成したボス問題が保存されていません。");
    }
}
