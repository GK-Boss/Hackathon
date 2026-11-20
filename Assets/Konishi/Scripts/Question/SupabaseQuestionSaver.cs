
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class SupabaseQuestionSaver
{
    [Serializable]
    private class SaveRequest
    {
        public QuestionData question;
    }

    [Serializable]
    private class SaveResponse
    {
        public bool success;
        public bool duplicated;
        public string id;
        public string error;
    }

    public static async Task<bool> SaveAsync(
        QuestionData question,
        AIClientSettings settings)
    {
        if (question == null)
        {
            Debug.LogWarning("[Save] 問題データがnullです。");
            return false;
        }

        if (settings == null ||
            string.IsNullOrWhiteSpace(settings.saveQuestionUrl))
        {
            Debug.LogWarning("[Save] 保存APIのURLが設定されていません。");
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.serverToken))
        {
            Debug.LogWarning("[Save] サーバートークンが設定されていません。");
            return false;
        }

        try
        {
            var payload = new SaveRequest
            {
                question = question
            };

            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);

            using (var request = new UnityWebRequest(
                settings.saveQuestionUrl,
                UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Content-Type", "application/json");

                request.SetRequestHeader(
                    "Authorization",
                    "Bearer " + settings.serverToken);

                request.timeout = settings.timeoutSeconds;

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(
                        "[Save] Supabase保存失敗: " +
                        request.responseCode + " / " +
                        request.error + " / " +
                        request.downloadHandler.text);

                    return false;
                }

                var result = JsonUtility.FromJson<SaveResponse>(
                    request.downloadHandler.text);

                if (result == null || !result.success)
                {
                    Debug.LogWarning(
                        "[Save] 保存に失敗しました: " +
                        result?.error);

                    return false;
                }

                if (!string.IsNullOrEmpty(result.id))
                {
                    question.id = result.id;
                }

                Debug.Log(
                    result.duplicated
                        ? "[Save] 既存問題のため保存をスキップしました。"
                        : "[Save] Supabaseに保存しました。ID: " + result.id);

                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "[Save] 保存処理で例外が発生しました: " + e.Message);

            return false;
        }
    }
}
