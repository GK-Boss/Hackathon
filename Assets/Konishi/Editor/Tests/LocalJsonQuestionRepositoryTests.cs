using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

/// <summary>
/// 問題の保存・取得の検査（指示書 §13）。
///
/// Tomo注: 「questions_seed.json が読めること」が特に重要です。
///         ここが落ちると、デモで1問も出題できません。
///
///         保存先はテスト用の一時ファイルに差し替えているので、
///         端末の本番データ（persistentDataPath）は汚しません。
/// </summary>
public class LocalJsonQuestionRepositoryTests
{
    private string tempPath;
    private LocalJsonQuestionRepository repository;

    [SetUp]
    public void SetUp()
    {
        tempPath = Path.Combine(Path.GetTempPath(), "question_repo_test_" + System.Guid.NewGuid().ToString("N") + ".json");
        repository = new LocalJsonQuestionRepository(tempPath);
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(tempPath)) File.Delete(tempPath);
    }

    private static QuestionData Make(string text, string subject = "薬理学") => TestQuestions.Valid(text, subject);

    // ---- 追加・重複 ----

    [Test]
    public void 追加した問題が取り出せる()
    {
        var question = Make("問1");
        Assert.IsTrue(repository.SaveQuestionAsync(question).GetAwaiter().GetResult());
        Assert.AreEqual(question.id, repository.GetQuestionAsync(question.id).GetAwaiter().GetResult().id);
    }

    [Test]
    public void 同じ問題文は追加されない()
    {
        repository.Add(Make("問1"));
        Assert.IsFalse(repository.Add(Make("問1")));
        Assert.AreEqual(1, repository.Count);
    }

    [Test]
    public void idが空なら自動で付く()
    {
        var question = Make("問1");
        question.id = null;
        repository.Add(question);
        Assert.IsNotEmpty(question.id);
    }

    // ---- 関連語での検索（1/NルールのN） ----

    [Test]
    public void 関連語で問題を引ける()
    {
        repository.Add(Make("問1"));
        repository.Add(Make("問2"));

        var found = repository.GetQuestionsByRelatedTermAsync("アドレナリン").GetAwaiter().GetResult();
        Assert.AreEqual(2, found.Count);
    }

    [Test]
    public void 関連語に紐づく問題数を数えられる()
    {
        repository.Add(Make("問1"));
        repository.Add(Make("問2"));

        Assert.AreEqual(2, repository.CountQuestionsByRelatedTermAsync("アドレナリン").GetAwaiter().GetResult());
        Assert.AreEqual(0, repository.CountQuestionsByRelatedTermAsync("存在しない語").GetAwaiter().GetResult());
    }

    [Test]
    public void 関連語の検索は分野で絞れる()
    {
        repository.Add(Make("問1", "薬理学"));
        repository.Add(Make("問2", "生化学"));

        Assert.AreEqual(1, repository.CountQuestionsByRelatedTermAsync("アドレナリン", "薬理学").GetAwaiter().GetResult());
    }

    // ---- 分野 ----

    [Test]
    public void 分野で絞り込める()
    {
        repository.Add(Make("問1", "薬理学"));
        repository.Add(Make("問2", "生化学"));

        Assert.AreEqual(1, repository.GetQuestionsBySubjectAsync("薬理学").GetAwaiter().GetResult().Count);
        Assert.AreEqual(2, repository.GetQuestionsBySubjectAsync(null).GetAwaiter().GetResult().Count);
    }

    [Test]
    public void 検証済みだけに絞り込める()
    {
        var verified = Make("検証済みの問題");
        verified.verified = true;
        repository.Add(verified);
        repository.Add(Make("AIが作った問題"));

        Assert.AreEqual(1, repository.GetQuestionsBySubjectAsync("薬理学", true).GetAwaiter().GetResult().Count);
    }

    [Test]
    public void 分野の一覧が取れる()
    {
        repository.Add(Make("問1", "薬理学"));
        repository.Add(Make("問2", "生化学"));
        repository.Add(Make("問3", "薬理学"));

        Assert.AreEqual(2, repository.GetSubjectsAsync().GetAwaiter().GetResult().Count);
    }

    // ---- JSON ----

    [Test]
    public void JSONに書き出して読み戻せる()
    {
        var original = new List<QuestionData> { Make("問1"), Make("問2") };
        var restored = LocalJsonQuestionRepository.FromJson(LocalJsonQuestionRepository.ToJson(original));

        Assert.AreEqual(2, restored.Count);
        Assert.AreEqual(6, restored[0].choices.Length);
        Assert.AreEqual(3, restored[0].relatedTerms.Count, "関連語がJSONを往復できていません。");
        Assert.AreEqual(5, restored[0].relatedTerms[0].relevance);
    }

    [Test]
    public void 壊れたJSONでも落ちない()
    {
        Assert.AreEqual(0, LocalJsonQuestionRepository.FromJson("").Count);
        Assert.AreEqual(0, LocalJsonQuestionRepository.FromJson(null).Count);
    }

    [Test]
    public void 生成した問題はファイルに保存される()
    {
        repository.SaveQuestionAsync(Make("AIが作った問題")).GetAwaiter().GetResult();
        Assert.IsTrue(File.Exists(tempPath), "保存ファイルが作られていません。");

        var reloaded = new LocalJsonQuestionRepository(tempPath);
        Assert.AreEqual(1, reloaded.LoadFromDisk());
    }

    [Test]
    public void 検証済みの問題はファイルに書き出さない()
    {
        // 種データはgit管理なので、生成分のファイルには混ぜない
        var verified = Make("種データの問題");
        verified.verified = true;
        repository.SaveQuestionAsync(verified).GetAwaiter().GetResult();

        var reloaded = new LocalJsonQuestionRepository(tempPath);
        Assert.AreEqual(0, reloaded.LoadFromDisk());
    }

    // ---- 種データ ----

    [Test]
    public void 種データを読み込める()
    {
        Assert.Greater(repository.LoadSeed(), 0, "questions_seed.json が読み込めません。デモで出題できなくなります。");
    }

    [Test]
    public void 種データはすべて6択で検証に通る()
    {
        repository.LoadSeed();

        foreach (var question in repository.All)
        {
            Assert.IsTrue(question.verified, "種データは検証済みにしてください: " + question.questionText);
            Assert.AreEqual(QuestionRequest.DefaultChoiceCount, question.choices.Length,
                "種データが6択になっていません: " + question.questionText);
            Assert.IsTrue(AIQuestionValidator.Validate(question, QuestionRequest.DefaultChoiceCount, out string reason),
                "種データが検証に通りません: " + question.questionText + " / " + reason);
        }
    }

    [Test]
    public void 種データの全問に関連語がある()
    {
        repository.LoadSeed();

        foreach (var question in repository.All)
        {
            Assert.IsNotEmpty(question.relatedTerms, "関連語がありません: " + question.questionText);
        }
    }

    [Test]
    public void 種データに分野が2つ以上ある()
    {
        repository.LoadSeed();
        Assert.GreaterOrEqual(repository.GetSubjectsAsync().GetAwaiter().GetResult().Count, 2);
    }
}
