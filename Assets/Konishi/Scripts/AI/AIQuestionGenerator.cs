using UnityEngine;

/// <summary>
/// 次の問題をバックグラウンドで1問だけ先読み生成しておく。
///
/// Tomo注: 企画書の「生成はバックグラウンドで行い、間に合わなければ既存問題へフォールバック」に対応します。
///         プレイヤーが今の問題を解いている間に次を作り、間に合っていれば使う、という作りです。
///         間に合わなくてもゲームは止まりません（QuestionManager が固定問題を出します）。
/// </summary>
public class AIQuestionGenerator : MonoBehaviour
{
    [SerializeField] private AIAPIClient apiClient;

    [Tooltip("生成に失敗した問題をログに出す。デモ前の確認用。")]
    [SerializeField] private bool verboseLog = true;

    /// 生成が完了して、まだ使われていない問題。無ければ null。
    private QuestionData readyQuestion;

    /// 多重リクエスト防止。1度に1問しか作らない。
    private bool isGenerating;

    /// APIキーが設定されていて生成を試せる状態か。
    public bool IsAvailable => apiClient != null && apiClient.IsAvailable;

    private void Awake()
    {
        if (apiClient == null) apiClient = GetComponent<AIAPIClient>();
    }

    /// <summary>
    /// 次の問題の生成を始める。すでに生成中、または生成済みの在庫がある場合は何もしない。
    /// 応答を待たないので、呼び出し側はそのままゲームを進められる。
    /// </summary>
    public void Prefetch(string subject, int chain, string relatedTerm)
    {
        if (!IsAvailable || isGenerating || readyQuestion != null) return;

        isGenerating = true;
        string prompt = PromptTemplates.BuildQuestionPrompt(subject, chain, relatedTerm);

        StartCoroutine(apiClient.RequestChatCompletion(
            PromptTemplates.SystemPrompt,
            prompt,
            OnResponse,
            OnError));
    }

    /// <summary>
    /// 生成済みの問題があれば取り出す。取り出した問題は在庫から消える。
    /// 間に合っていなければ false を返すので、呼び出し側は固定問題にフォールバックする。
    /// </summary>
    public bool TryTakeGenerated(out QuestionData question)
    {
        question = readyQuestion;
        readyQuestion = null;
        return question != null;
    }

    /// 生成結果を受け取って検証し、通れば在庫に入れる。
    private void OnResponse(string content)
    {
        isGenerating = false;

        var dto = AIQuestionDto.Parse(content);
        if (dto == null)
        {
            if (verboseLog) Debug.LogWarning("[AI] 応答をJSONとして読めませんでした。固定問題を使います。");
            return;
        }

        if (!dto.Validate(PromptTemplates.ChoiceCount, out string reason))
        {
            // 企画書「生成失敗時は同じ生成を即時再試行せず、次の機会に別の生成を行う」に従い、ここでは作り直さない。
            if (verboseLog) Debug.LogWarning("[AI] 生成された問題が検証に通りませんでした: " + reason);
            return;
        }

        readyQuestion = dto.ToQuestionData();
        if (verboseLog) Debug.Log("[AI] 次の問題を生成しました: " + readyQuestion.questionText);
    }

    /// 通信失敗時。ゲームは止めず、固定問題で続行する。
    private void OnError(string error)
    {
        isGenerating = false;
        if (verboseLog) Debug.LogWarning("[AI] 生成に失敗しました。固定問題を使います: " + error);
    }
}
