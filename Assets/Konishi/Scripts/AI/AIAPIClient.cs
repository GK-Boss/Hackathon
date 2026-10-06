using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// OpenAI(ChatGPT)のChat Completions APIを叩くだけのクラス。
///
/// Tomo注: ここは「送って受け取る」ことだけを担当します。
///         プロンプトの中身は PromptTemplates、生成結果の判断は AIQuestionGenerator が持ちます。
/// </summary>
public class AIAPIClient : MonoBehaviour
{
    [Tooltip("未設定ならResourcesから自動で読み込む。")]
    [SerializeField] private OpenAISettings settings;

    /// APIキーが設定済みで、リクエストを送れる状態か。
    public bool IsAvailable => settings != null && settings.IsConfigured;

    private void Awake()
    {
        if (settings == null) settings = OpenAISettings.LoadOrNull();

        if (settings == null)
            Debug.Log("[AI] OpenAISettingsが見つかりません。固定問題のみで動作します。");
        else if (!settings.IsConfigured)
            Debug.Log("[AI] OpenAISettingsにAPIキーが未設定です。固定問題のみで動作します。");
    }

    /// <summary>
    /// system/userの2メッセージを送り、AIの返答テキストを受け取る。
    /// 失敗しても例外は投げず、onError に理由を渡す（ゲームは固定問題で続行できるようにするため）。
    /// </summary>
    public IEnumerator RequestChatCompletion(string systemPrompt, string userPrompt, Action<string> onSuccess, Action<string> onError)
    {
        if (!IsAvailable)
        {
            onError?.Invoke("APIキーが未設定です。");
            yield break;
        }

        // リクエストJSONの組み立て。文字列のエスケープはJsonUtilityに任せる。
        var request = new ChatRequest
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

        byte[] body = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(request));

        using (var web = new UnityWebRequest(settings.endpoint, UnityWebRequest.kHttpVerbPOST))
        {
            web.uploadHandler = new UploadHandlerRaw(body);
            web.downloadHandler = new DownloadHandlerBuffer();
            web.SetRequestHeader("Content-Type", "application/json");
            web.SetRequestHeader("Authorization", "Bearer " + settings.apiKey);
            web.timeout = settings.timeoutSeconds;

            yield return web.SendWebRequest();

            if (web.result != UnityWebRequest.Result.Success)
            {
                // Tomo注: レスポンス本文にはキーは含まれないのでログに出して問題ありません。
                onError?.Invoke(web.error + " / " + web.downloadHandler.text);
                yield break;
            }

            var response = JsonUtility.FromJson<ChatResponse>(web.downloadHandler.text);
            if (response == null || response.choices == null || response.choices.Length == 0 || response.choices[0].message == null)
            {
                onError?.Invoke("応答の形式が想定外です。");
                yield break;
            }

            onSuccess?.Invoke(response.choices[0].message.content);
        }
    }

    // ---- 以下はAPIのJSONに合わせた入れ物。フィールド名はAPIの仕様に合わせているため変更しないこと。 ----

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public float temperature;
        public ResponseFormat response_format;
    }

    [Serializable]
    private class ChatMessage
    {
        public string role;
        public string content;
    }

    /// JSONオブジェクトでの返答を強制する指定。これがないと前後に文章が付くことがある。
    [Serializable]
    private class ResponseFormat
    {
        public string type;
    }

    [Serializable]
    private class ChatResponse
    {
        public ResponseChoice[] choices;
    }

    [Serializable]
    private class ResponseChoice
    {
        public ResponseMessage message;
    }

    [Serializable]
    private class ResponseMessage
    {
        public string content;
    }
}
