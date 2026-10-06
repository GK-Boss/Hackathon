using NUnit.Framework;

/// <summary>
/// AIの応答を受け取る部分の検査。
///
/// Tomo注: AIは必ず壊れた出力を返してきます。ここが素通しになると、
///         選択肢が4個しかない問題や正解のない問題がそのまま画面に出ます。
///         「AIに判断させず、コードで検査する」ための砦なので、必ず緑に保ってください。
/// </summary>
public class AIQuestionDtoTests
{
    private const int ChoiceCount = 5;

    /// 検証に通る正常なDTOを作る。各テストはここから1か所だけ壊して使う。
    private static AIQuestionDto CreateValid()
    {
        return new AIQuestionDto
        {
            questionText = "Unityで2D画像を表示するコンポーネントは？",
            choices = new[] { "SpriteRenderer", "AudioSource", "Light", "Animator", "Camera" },
            correctIndex = 0,
            explanation = "SpriteRendererはSpriteアセットを画面に描画します。",
            relatedTerm = "SpriteRenderer"
        };
    }

    [Test]
    public void 正常な問題は検証に通る()
    {
        Assert.IsTrue(CreateValid().Validate(ChoiceCount, out string reason), reason);
    }

    [Test]
    public void 問題文が空なら弾く()
    {
        var dto = CreateValid();
        dto.questionText = "   ";
        Assert.IsFalse(dto.Validate(ChoiceCount, out _));
    }

    [Test]
    public void 選択肢の数が違えば弾く()
    {
        var dto = CreateValid();
        dto.choices = new[] { "A", "B", "C", "D" };
        Assert.IsFalse(dto.Validate(ChoiceCount, out _));
    }

    [Test]
    public void 選択肢が空文字なら弾く()
    {
        var dto = CreateValid();
        dto.choices[2] = "";
        Assert.IsFalse(dto.Validate(ChoiceCount, out _));
    }

    [Test]
    public void 選択肢が重複していたら弾く()
    {
        // 正解が一意に決まらなくなるため。AIがよくやるミス。
        var dto = CreateValid();
        dto.choices[3] = dto.choices[0];
        Assert.IsFalse(dto.Validate(ChoiceCount, out _));
    }

    [Test]
    public void 正解indexが範囲外なら弾く()
    {
        var dto = CreateValid();
        dto.correctIndex = 5;
        Assert.IsFalse(dto.Validate(ChoiceCount, out _));
    }

    [Test]
    public void 解説が空なら弾く()
    {
        var dto = CreateValid();
        dto.explanation = null;
        Assert.IsFalse(dto.Validate(ChoiceCount, out _));
    }

    [Test]
    public void 検証に通ればQuestionDataへ変換できる()
    {
        var question = CreateValid().ToQuestionData();
        Assert.AreEqual("SpriteRenderer", question.choices[question.correctIndex]);
        Assert.AreEqual(ChoiceCount, question.choices.Length);
    }

    [Test]
    public void 素のJSONを読める()
    {
        string json = "{\"questionText\":\"問\",\"choices\":[\"a\",\"b\",\"c\",\"d\",\"e\"],\"correctIndex\":1,\"explanation\":\"解説\",\"relatedTerm\":\"語\"}";
        var dto = AIQuestionDto.Parse(json);
        Assert.IsNotNull(dto);
        Assert.AreEqual(1, dto.correctIndex);
    }

    [Test]
    public void コードブロックで包まれていても読める()
    {
        // ChatGPTは指示しても ```json を付けてくることがあるため。
        string wrapped = "```json\n{\"questionText\":\"問\",\"choices\":[\"a\",\"b\",\"c\",\"d\",\"e\"],\"correctIndex\":2,\"explanation\":\"解説\",\"relatedTerm\":\"語\"}\n```";
        var dto = AIQuestionDto.Parse(wrapped);
        Assert.IsNotNull(dto);
        Assert.AreEqual(2, dto.correctIndex);
    }

    [Test]
    public void JSONでない応答はnullを返す()
    {
        Assert.IsNull(AIQuestionDto.Parse("すみません、問題を作れませんでした。"));
        Assert.IsNull(AIQuestionDto.Parse(""));
        Assert.IsNull(AIQuestionDto.Parse(null));
    }
}
