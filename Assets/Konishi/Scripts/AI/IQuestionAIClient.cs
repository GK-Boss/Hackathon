using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// AIに文章を書かせるための窓口。
///
/// Tomo注: AIQuestionGenerator はこのインターフェースしか見ません。
///         「自前サーバー経由か、直接OpenAIか」を知らずに動きます。
///
///         本番は GameServerAIClient（自前サーバー経由）を使ってください。
///         DirectOpenAIClient は開発中だけの手段です。
/// </summary>
public interface IQuestionAIClient
{
    /// 設定が揃っていて、リクエストを送れる状態か。
    bool IsAvailable { get; }

    /// <summary>
    /// AIに問い合わせて、返ってきた本文を返す。
    /// 失敗時は例外を投げず null を返す（ゲームを止めず既存問題へフォールバックするため）。
    /// </summary>
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
