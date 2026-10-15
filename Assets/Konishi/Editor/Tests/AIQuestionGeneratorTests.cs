using NUnit.Framework;

/// <summary>
/// AI生成の検査（指示書 §11、§17）。
///
/// Tomo注: 通信そのものはテストしていません（APIキーが要り、毎回結果が変わるため）。
///         FakeAIClient で「AIがこう返したらどうなるか」だけを見ています。
///
///         いちばん大事なのは「無限リトライしないこと」です。
///         失敗を繰り返すとAPI料金がかさみ、デモ中に固まります。
/// </summary>
public class AIQuestionGeneratorTests
{
    private static QuestionRequest Request() => new QuestionRequest("薬理学", 2, "アドレナリン");

    // ---- 応答の解釈 ----

    [Test]
    public void 素のJSONを読める()
    {
        var question = AIQuestionGenerator.ParseResponse(TestQuestions.ValidJson, Request());
        Assert.IsNotNull(question);
        Assert.AreEqual(6, question.choices.Length);
        Assert.AreEqual("心拍数増加", question.CorrectChoice);
        Assert.AreEqual(2, question.relatedTerms.Count);
    }

    [Test]
    public void コードブロックで包まれていても読める()
    {
        var question = AIQuestionGenerator.ParseResponse("```json\n" + TestQuestions.ValidJson + "\n```", Request());
        Assert.IsNotNull(question);
    }

    [Test]
    public void 前後に文章が付いていても読める()
    {
        var question = AIQuestionGenerator.ParseResponse("はい、どうぞ。\n" + TestQuestions.ValidJson + "\n以上です。", Request());
        Assert.IsNotNull(question);
    }

    [Test]
    public void JSONでない応答はnull()
    {
        Assert.IsNull(AIQuestionGenerator.ParseResponse("すみません、作れませんでした。", Request()));
        Assert.IsNull(AIQuestionGenerator.ParseResponse("", Request()));
        Assert.IsNull(AIQuestionGenerator.ParseResponse(null, Request()));
    }

    [Test]
    public void 分野と難易度は注文の値で上書きされる()
    {
        // AIの自己申告は当てにしない
        string lying = "{\"subject\":\"物理学\",\"difficulty\":9," + TestQuestions.ValidJson.Substring(1);
        var question = AIQuestionGenerator.ParseResponse(lying, Request());

        Assert.AreEqual("薬理学", question.subject);
        Assert.AreEqual(2, question.difficulty);
    }

    [Test]
    public void 生成した問題は未検証として扱われる()
    {
        // 人が目視していないので verified は必ず false
        Assert.IsFalse(AIQuestionGenerator.ParseResponse(TestQuestions.ValidJson, Request()).verified);
    }

    // ---- 生成〜検証〜修正の流れ ----

    [Test]
    public void 一発で合格すれば1回しか問い合わせない()
    {
        var client = new FakeAIClient(TestQuestions.ValidJson);
        var generator = new AIQuestionGenerator(client, false);

        var question = generator.GenerateAsync(Request()).GetAwaiter().GetResult();

        Assert.IsNotNull(question);
        Assert.AreEqual(1, client.CallCount, "合格したのに修正を依頼しています。");
    }

    [Test]
    public void 検証に落ちたら1回だけ修正を依頼する()
    {
        // 1回目は5択（不合格）、2回目で正しい6択が返る
        string broken = TestQuestions.ValidJson.Replace(",\"発汗停止\"", "");
        var client = new FakeAIClient(broken, TestQuestions.ValidJson);
        var generator = new AIQuestionGenerator(client, false);

        var question = generator.GenerateAsync(Request()).GetAwaiter().GetResult();

        Assert.IsNotNull(question, "修正後は合格するはずです。");
        Assert.AreEqual(2, client.CallCount);
    }

    [Test]
    public void 修正後も落ちたら破棄して2回でやめる()
    {
        // 企画書どおり、その場で無限に作り直さない
        string broken = TestQuestions.ValidJson.Replace(",\"発汗停止\"", "");
        var client = new FakeAIClient(broken, broken);
        var generator = new AIQuestionGenerator(client, false);

        var question = generator.GenerateAsync(Request()).GetAwaiter().GetResult();

        Assert.IsNull(question, "2回落ちたら破棄するはずです。");
        Assert.AreEqual(2, client.CallCount, "3回以上問い合わせています。無限リトライの危険があります。");
    }

    [Test]
    public void 応答が無ければ修正も依頼しない()
    {
        var client = new FakeAIClient((string)null);
        var generator = new AIQuestionGenerator(client, false);

        Assert.IsNull(generator.GenerateAsync(Request()).GetAwaiter().GetResult());
        Assert.AreEqual(1, client.CallCount);
    }

    [Test]
    public void APIが使えなければ問い合わせない()
    {
        var client = new FakeAIClient(TestQuestions.ValidJson) { IsAvailable = false };
        var generator = new AIQuestionGenerator(client, false);

        Assert.IsNull(generator.GenerateAsync(Request()).GetAwaiter().GetResult());
        Assert.AreEqual(0, client.CallCount);
    }

    [Test]
    public void 既存と重複する問題は弾かれる()
    {
        var client = new FakeAIClient(TestQuestions.ValidJson, TestQuestions.ValidJson);
        var generator = new AIQuestionGenerator(client, false);
        var existing = new[] { TestQuestions.Valid().IdentityKey };

        Assert.IsNull(generator.GenerateAsync(Request(), existing).GetAwaiter().GetResult());
    }

    [Test]
    public void 分野が指定されていない注文は送らない()
    {
        var client = new FakeAIClient(TestQuestions.ValidJson);
        var generator = new AIQuestionGenerator(client, false);

        Assert.IsNull(generator.GenerateAsync(new QuestionRequest("", 1)).GetAwaiter().GetResult());
        Assert.AreEqual(0, client.CallCount);
    }

    [Test]
    public void 指示書の形の入口でも生成できる()
    {
        var client = new FakeAIClient(TestQuestions.ValidJson);
        var generator = new AIQuestionGenerator(client, false);

        var question = generator.GenerateQuestion("薬理学", "アドレナリン", 2).GetAwaiter().GetResult();
        Assert.IsNotNull(question);
    }
}
