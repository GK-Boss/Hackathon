using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 注文(QuestionRequest)を受けて、AIに問題を1問作らせる。
///
/// Tomo注: ゲーム側からこのクラスを直接呼ばないでください。必ず QuestionManager 経由です。
///
///         生成の流れ（企画書・指示書どおり）:
///           生成 → 検証 → 失敗 → 【1回だけ】修正を依頼 → 再検証 → 失敗 → 破棄
///
///         同じ生成をその場で何度も繰り返しません。失敗したら次の機会に別の問題を作ります。
/// </summary>
public class AIQuestionGenerator
{
    private readonly IQuestionAIClient client;

    /// 生成の成否をConsoleに出すか。デモ前の確認用。
    private readonly bool verboseLog;

    public AIQuestionGenerator(IQuestionAIClient client, bool verboseLog = true)
    {
        this.client = client;
        this.verboseLog = verboseLog;
    }

    /// AIに問い合わせられる状態か。
    public bool IsAvailable => client != null && client.IsAvailable;

    /// <summary>
    /// 指示書 §8 のかたちの入口。分野・関連語・難易度を指定して1問作る。
    /// </summary>
    public Task<QuestionData> GenerateQuestion(string subject, string relatedTerm, int difficulty, CancellationToken cancellationToken = default)
    {
        return GenerateAsync(new QuestionRequest(subject, difficulty, relatedTerm), null, cancellationToken);
    }

    /// <summary>
    /// 注文どおりに問題を作る。検証を通らなければ null を返す（呼び出し側は既存問題を使う）。
    /// </summary>
    /// <param name="request">どんな問題を作るかの注文。</param>
    /// <param name="existingKeys">既存問題の IdentityKey。重複を弾くために渡す。</param>
    public async Task<QuestionData> GenerateAsync(
        QuestionRequest request,
        ICollection<string> existingKeys = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable) return null;

        if (request == null)
        {
            if (verboseLog) Debug.LogWarning("[Question] 注文(QuestionRequest)がnullです。");
            return null;
        }

        if (!request.IsValid(out string requestReason))
        {
            if (verboseLog) Debug.LogWarning("[Question] 注文が不正です: " + requestReason);
            return null;
        }

        // ---- 1回目 ----
        string content = await client.CompleteAsync(
            PromptTemplates.SystemPrompt,
            PromptTemplates.BuildQuestionPrompt(request),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            if (verboseLog) Debug.LogWarning("[Question] AIから応答がありませんでした。既存問題を使います。");
            return null;
        }

        var question = ParseResponse(content, request);
        string firstReason;

        if (question == null)
        {
            firstReason = "返ってきた文字列をJSONとして読めませんでした。";
        }
        else if (AIQuestionValidator.Validate(question, request.choiceCount, request.subject, existingKeys, out firstReason))
        {
            if (verboseLog) Debug.Log("[Question] 生成しました: " + question);
            return question;
        }

        // ---- 修正は1回だけ ----
        if (verboseLog) Debug.Log("[Question] 検証に落ちたので1回だけ修正を依頼します: " + firstReason);

        string repaired = await client.CompleteAsync(
            PromptTemplates.SystemPrompt,
            PromptTemplates.BuildRepairPrompt(request, content, firstReason),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(repaired))
        {
            if (verboseLog) Debug.LogWarning("[Question] 修正の応答がありませんでした。この問題は破棄します。");
            return null;
        }

        var repairedQuestion = ParseResponse(repaired, request);
        string secondReason = "JSONとして読めませんでした。";

        if (repairedQuestion != null &&
            AIQuestionValidator.Validate(repairedQuestion, request.choiceCount, request.subject, existingKeys, out secondReason))
        {
            if (verboseLog) Debug.Log("[Question] 修正後に合格しました: " + repairedQuestion);
            return repairedQuestion;
        }

        // 2回目も落ちたら破棄する。その場で作り直さず、次の機会に別の問題を作る。
        if (verboseLog) Debug.LogWarning("[Question] 修正後も検証に落ちたので破棄します: " + secondReason);
        return null;
    }

    /// <summary>
    /// AIの応答文字列を QuestionData に変換する。失敗したら null。
    /// コードブロック(```json ... ```)で包まれて返ってくることがあるため、JSON部分だけ切り出す。
    /// 分野と難易度はAIの自己申告を使わず、注文の値で上書きする。
    /// </summary>
    public static QuestionData ParseResponse(string rawContent, QuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(rawContent)) return null;

        int start = rawContent.IndexOf('{');
        int end = rawContent.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        QuestionData question;
        try
        {
            question = JsonUtility.FromJson<QuestionData>(rawContent.Substring(start, end - start + 1));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Question] JSONの解析に失敗しました: " + e.Message);
            return null;
        }

        if (question == null) return null;

        if (request != null)
        {
            question.subject = request.subject;
            question.difficulty = request.difficulty;
        }

        // 人が目視していないので、必ず未検証として扱う
        question.verified = false;
        if (question.relatedTerms == null) question.relatedTerms = new List<RelatedTerm>();

        return question;
    }
}
