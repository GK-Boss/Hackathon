using UnityEngine;

/// <summary>
/// AIへの接続設定。
///
/// Tomo注: このアセットは .gitignore で除外しています。
///         Tools > Learning Game > Setup Question System で作られます。
///
///         企画書・指示書の方針により、公開するアプリに OpenAI の APIキーを含めません。
///         本番は GameServer モードで、自前サーバー経由にしてください。
///         DirectOpenAI_DevelopmentOnly は、サーバーができるまでの開発用です。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/AIClientSettings", fileName = "AIClientSettings")]
public class AIClientSettings : ScriptableObject
{
    /// Resources.Load で読み込むパス。実体は Assets/Konishi/Resources/AIClientSettings.asset。
    public const string ResourcePath = "AIClientSettings";

    public enum Mode
    {
        /// 本番。自前サーバー経由でAIを呼ぶ。APIキーはサーバー側だけに置く。
        GameServer = 0,

        /// 開発用。Unityから直接OpenAIを呼ぶ。ビルドに含めないこと。
        DirectOpenAI_DevelopmentOnly = 1
    }

    [Header("どこ経由でAIを呼ぶか")]
    [Tooltip("本番は GameServer。DirectOpenAI は開発中だけ。")]
    public Mode mode = Mode.GameServer;

    [Header("本番：自前サーバー")]
    [Tooltip("問題生成を中継するサーバーのURL。例: https://example.com/api/question/generate")]
    public string serverUrl = "";

    [Tooltip("サーバー側で発行するクライアント用トークン。OpenAIのキーではない。")]
    public string serverToken = "";

    [Header("開発用：OpenAI直叩き（ビルドに含めないこと）")]
    [Tooltip("開発中だけ使う。ここにキーを入れたままビルドしないこと。")]
    public string openAIApiKey = "";

    public string openAIEndpoint = "https://api.openai.com/v1/chat/completions";

    [Header("共通")]
    [Tooltip("使用するモデル名。サーバー側で固定する場合は無視される。")]
    public string model = "gpt-4o-mini";

    [Range(0f, 2f)]
    [Tooltip("0に近いほど安定、大きいほど出題のバリエーションが増える。")]
    public float temperature = 0.8f;

    [Tooltip("この秒数を超えたら生成をあきらめ、既存問題にフォールバックする。")]
    public int timeoutSeconds = 20;

    /// 選んだモードで実際に通信できる状態か。
    public bool IsConfigured =>
        mode == Mode.GameServer
            ? !string.IsNullOrWhiteSpace(serverUrl)
            : !string.IsNullOrWhiteSpace(openAIApiKey);

    /// Resources から設定を読み込む。無ければ null（呼び出し側は既存問題だけで動く）。
    public static AIClientSettings LoadOrNull() => Resources.Load<AIClientSettings>(ResourcePath);

    /// <summary>設定に合った通信クラスを作る。設定が無ければ null。</summary>
    public IQuestionAIClient CreateClient()
    {
        if (!IsConfigured) return null;

        return mode == Mode.GameServer
            ? (IQuestionAIClient)new GameServerAIClient(this)
            : new DirectOpenAIClient(this);
    }
}
