using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Unityから直接OpenAIを呼ぶ。【開発用。本番では使わない】
///
/// Tomo注: この構成だとAPIキーがビルドに含まれ、アプリから抜き出せてしまいます。
///         指示書でも「公開するUnityアプリから直接OpenAI APIを呼び出す構成にはしない」とあります。
///
///         サーバーができるまでの開発・デモ確認用としてだけ使ってください。
///         ストアに出すビルドでは、必ず AIClientSettings.mode を GameServer にしてください。
/// </summary>
public class DirectOpenAIClient : IQuestionAIClient
{
    private readonly AIClientSettings settings;

    public DirectOpenAIClient(AIClientSettings settings)
    {
        this.settings = settings;
    }

    public bool IsAvailable => settings != null && !string.IsNullOrWhiteSpace(settings.openAIApiKey);

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            Debug.Log("[AI] OpenAIのAPIキーが未設定です。既存問題だけで動作します。");
            return null;
        }

#if !UNITY_EDITOR
        Debug.LogWarning("[AI] ビルドしたアプリでOpenAIを直接呼んでいます。公開前に GameServer モードへ切り替えてください。");
#endif

        var payload = new ChatRequest
        {
            model = settings.model,
            temperature = settings.temperature,
            response_format = new ResponseFormat { type = "json_object" },
            messages = new[]
            {
                new ChatMessage { role = "system", content = systemPrompt },
                new ChatMessage { role = "user", content = userPrompt }
            }
        };

        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));

        using (var request = new UnityWebRequest(settings.openAIEndpoint, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + settings.openAIApiKey);
            request.timeout = settings.timeoutSeconds;

            await request.SendAsync(cancellationToken);

            if (request.result != UnityWebRequest.Result.Success)
            {
                // レスポンス本文にキーは含まれないのでログに出して問題ない
                Debug.LogWarning("[AI] OpenAIへの問い合わせに失敗しました: " + request.error);
                return null;
            }

            try
            {
                var response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                if (response?.choices == null || response.choices.Length == 0) return null;

                return response.choices[0].message?.content;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AI] OpenAIの応答を読めませんでした: " + e.Message);
                return null;
            }
        }
    }

    // ---- OpenAI APIのJSONに合わせた入れ物。フィールド名は仕様に合わせているため変更しないこと ----

    [Serializable] private class ChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public float temperature;
        public ResponseFormat response_format;
    }

    [Serializable] private class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable] private class ResponseFormat
    {
        public string type;
    }

    [Serializable] private class ChatResponse
    {
        public ResponseChoice[] choices;
    }

    [Serializable] private class ResponseChoice
    {
        public ResponseMessage message;
    }

    [Serializable] private class ResponseMessage
    {
        public string content;
    }
}
