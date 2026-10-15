using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 自前サーバー経由でAIを呼ぶ。これが本番の構成。
///
/// Tomo注: 指示書の構成はこうです。
///
///           Unity  →(HTTPS)→  自前サーバー  →  OpenAI API
///
///         OpenAIのAPIキーはサーバーにだけ置き、Unityのビルドには含めません。
///
///         サーバー側に用意してもらう仕様（AIClientSettings.serverUrl に設定するエンドポイント）:
///           POST  Content-Type: application/json
///           リクエスト  { "system": "...", "user": "...", "model": "...", "temperature": 0.8 }
///           レスポンス  { "content": "AIが返した本文" }
///
///         サーバーがまだ無い間は DirectOpenAIClient（開発用）で動かせます。
/// </summary>
public class GameServerAIClient : IQuestionAIClient
{
    private readonly AIClientSettings settings;

    public GameServerAIClient(AIClientSettings settings)
    {
        this.settings = settings;
    }

    public bool IsAvailable => settings != null && !string.IsNullOrWhiteSpace(settings.serverUrl);

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            Debug.Log("[AI] サーバーURLが未設定です。既存問題だけで動作します。");
            return null;
        }

        var payload = new ServerRequest
        {
            system = systemPrompt,
            user = userPrompt,
            model = settings.model,
            temperature = settings.temperature
        };

        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));

        using (var request = new UnityWebRequest(settings.serverUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrWhiteSpace(settings.serverToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + settings.serverToken);
            }
            request.timeout = settings.timeoutSeconds;

            await request.SendAsync(cancellationToken);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[AI] サーバーへの問い合わせに失敗しました: " + request.error);
                return null;
            }

            try
            {
                var response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                return response?.content;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AI] サーバーの応答を読めませんでした: " + e.Message);
                return null;
            }
        }
    }

    [Serializable]
    private class ServerRequest
    {
        public string system;
        public string user;
        public string model;
        public float temperature;
    }

    [Serializable]
    private class ServerResponse
    {
        public string content;
    }
}
