using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// テスト用の共通部品。
///
/// Tomo注: 各テストが同じ問題を作り直さずに済むようにまとめています。
///         ここを直すと全テストに影響するので注意してください。
/// </summary>
public static class TestQuestions
{
    public const int ChoiceCount = QuestionRequest.DefaultChoiceCount;

    /// 検証に通る正常な問題。各テストはここから1か所だけ壊して使う。
    public static QuestionData Valid(string text = "アドレナリンの作用として正しいものはどれか", string subject = "薬理学")
    {
        return new QuestionData
        {
            subject = subject,
            difficulty = 1,
            questionText = text,
            choices = new[] { "心拍数増加", "心拍数低下", "瞳孔縮小", "気管支収縮", "血糖低下", "発汗停止" },
            correctIndex = 0,
            explanation = "アドレナリンはβ1受容体を介して心拍数を上げます。",
            relatedTerms = new List<RelatedTerm>
            {
                new RelatedTerm("アドレナリン", 5),
                new RelatedTerm("β1受容体", 4),
                new RelatedTerm("カテコールアミン", 2)
            }
        };
    }

    /// 正常な問題をJSON文字列にする。AIの応答を模すために使う。
    public static string ValidJson =>
        "{\"questionText\":\"アドレナリンの作用として正しいものはどれか\","
        + "\"choices\":[\"心拍数増加\",\"心拍数低下\",\"瞳孔縮小\",\"気管支収縮\",\"血糖低下\",\"発汗停止\"],"
        + "\"correctIndex\":0,\"explanation\":\"β1受容体を介して心拍数を上げます。\","
        + "\"relatedTerms\":[{\"term\":\"アドレナリン\",\"relevance\":5},{\"term\":\"β1受容体\",\"relevance\":4}]}";
}

/// <summary>
/// AIの代わりに、決めた文字列を返すだけのクライアント。
/// 通信もAPIキーも要らないので、テストが安定して速くなる。
/// </summary>
public class FakeAIClient : IQuestionAIClient
{
    private readonly Queue<string> responses = new Queue<string>();

    /// CompleteAsync が呼ばれた回数。リトライ回数の検査に使う。
    public int CallCount { get; private set; }

    public bool IsAvailable { get; set; } = true;

    public FakeAIClient(params string[] orderedResponses)
    {
        foreach (string response in orderedResponses) responses.Enqueue(response);
    }

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(responses.Count > 0 ? responses.Dequeue() : null);
    }
}
