using UnityEngine;

/// <summary>
/// ChatGPT(OpenAI API)への接続設定をまとめたアセット。
///
/// Tomo注: このアセットはAPIキーを持つため、.gitignore でリポジトリから除外しています。
///         各自がローカルで Tools > Learning Game > Create OpenAI Settings を実行して作成し、
///         Inspector からキーを貼り付けてください。未作成でもゲームは固定問題で動きます。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/OpenAISettings", fileName = "OpenAISettings")]
public class OpenAISettings : ScriptableObject
{
    /// Resources.Load で読み込むときのパス。実体は Assets/Konishi/Resources/OpenAISettings.asset。
    public const string ResourcePath = "OpenAISettings";

    [Header("認証")]
    [Tooltip("OpenAIのAPIキー。絶対にコミットしないこと。")]
    public string apiKey = "";

    [Header("モデル")]
    [Tooltip("使用するモデル名。コスト・速度・品質を見て変更する。")]
    public string model = "gpt-4o-mini";

    [Tooltip("0に近いほど出題が安定し、大きいほどバリエーションが増える。")]
    [Range(0f, 2f)]
    public float temperature = 0.8f;

    [Header("通信")]
    [Tooltip("この秒数を超えたら生成をあきらめ、固定問題にフォールバックする。")]
    public int timeoutSeconds = 20;

    [Tooltip("Chat Completions のエンドポイント。通常は変更しない。")]
    public string endpoint = "https://api.openai.com/v1/chat/completions";

    /// APIキーが設定されているか。false なら AI 連携は行わない。
    public bool IsConfigured => !string.IsNullOrWhiteSpace(apiKey);

    /// Resources から設定を読み込む。存在しなければ null を返す（呼び出し側が固定問題へフォールバックする）。
    public static OpenAISettings LoadOrNull() => Resources.Load<OpenAISettings>(ResourcePath);
}
