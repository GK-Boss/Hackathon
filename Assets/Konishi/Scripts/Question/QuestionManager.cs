using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 問題システムの窓口（Facade）。
///
/// Tomo注: ゲーム側が触るのはこのクラスだけです。
///         AI・DB・関連語の選び方・検証・保存の詳細は、ここから先に隠れています。
///
///         ゲーム側の使い方はこれだけ:
///             QuestionData q = questionManager.GetNextQuestion();
///             questionManager.RegisterQuestionAnswered(q, isCorrect);
///             questionManager.RegisterQuestionReloaded(q);
///             QuestionData boss = await questionManager.GenerateBossQuestion();
///
///         内側の分担:
///             IQuestionRepository   問題の保存・取得
///             QuestionSelector      次にどれを出すか（関連語＋1/Nルール）
///             AIQuestionGenerator   AIでの新規生成
///             AIQuestionValidator   生成結果の検証（Generatorの中で呼ばれる）
///             KnowledgeTracker      そのプレイ中に触れた知識の記録
/// </summary>
public class QuestionManager : MonoBehaviour
{
    static public QuestionManager instance;

    [Header("出題範囲")]
    [Tooltip("人が目視検証した問題だけを出題する。デモ本番ではONにする。")]
    [SerializeField] private bool verifiedOnly = false;

    [Tooltip("AIが作った問題を出題に使う。OFFにすると既存問題だけで回る。")]
    [SerializeField] private bool useGeneratedQuestions = true;

    [Tooltip("生成の成否をConsoleに出す。デモ前の確認用。")]
    [SerializeField] private bool verboseLog = true;

    [Tooltip("1回の攻略でAIに作らせる問題数の上限。APIの使いすぎを防ぐ。0で無制限。")]
    [SerializeField] private int maxGenerationsPerRun = 20;

    // ---- 内部の部品。ゲーム側には見せない ----
    private IQuestionRepository repository;
    private QuestionSelector selector;
    private AIQuestionGenerator generator;
    private KnowledgeTracker knowledgeTracker;

    /// この攻略で出題済みの問題id。
    private readonly HashSet<string> usedIds = new HashSet<string>();

    /// 裏で生成が終わって、まだ使っていない問題。
    private QuestionData pregenerated;

    /// 多重リクエスト防止。1度に1問しか作らない。
    private bool isGenerating;

    private string activeSubject;
    private bool initialized;

    /// 攻略を抜けたら生成を打ち切るためのトークン。再生停止時にも使う。
    private CancellationTokenSource runCancellation;

    /// この攻略でAIに作らせた数。上限の判定に使う。
    private int generatedThisRun;

    /// 出題候補の読み込みが終わったか。
    private bool candidatesLoaded;

    /// 出題候補の手元のコピー。GetNextQuestion は通信を待てないので、ここから選ぶ。
    private List<QuestionData> candidates = new List<QuestionData>();

    /// 直前に返した問題。
    public QuestionData Current { get; private set; }

    /// この攻略で出題した問題数。
    public int AskedCount => usedIds.Count;

    /// <summary>
    /// 出題候補の読み込みが終わり、GetNextQuestion が問題を返せる状態か。
    ///
    /// Tomo注: ローカル実装では StartRun から戻った時点で必ず true です。
    ///         サーバー実装に差し替えたときだけ、一瞬 false になります。
    ///         そのときは「読み込み中」を出すなどして、GetNextQuestion を待ってください。
    /// </summary>
    public bool IsReady => candidatesLoaded;

    /// そのプレイ中に触れた知識（語 → 回数）。結果画面などで見せたいとき用。
    public Dictionary<string, int> KnowledgeCounts => knowledgeTracker.GetKnowledgeCounts();

    private void Awake() => Initialize();

    /// <summary>
    /// 内部の部品を組み立てる。2回目以降は何もしない。
    ///
    /// Tomo注: Awakeは編集モードで呼ばれないので、各入口の先頭からも呼んでいます。
    ///         テストから部品を差し替えたいときは Configure を使ってください。
    /// </summary>
    public void Initialize()
    {
        if (initialized) return;

        instance = this;

        var settings = AIClientSettings.LoadOrNull();
        var client = settings != null ? settings.CreateClient() : null;

        Configure(
            new LocalJsonQuestionRepository(),
            new QuestionSelector(),
            new AIQuestionGenerator(client, verboseLog),
            new KnowledgeTracker());
    }

    /// <summary>
    /// 部品を差し替える。テストや、将来サーバー版リポジトリへ差し替えるときに使う。
    /// </summary>
    public void Configure(
        IQuestionRepository repository,
        QuestionSelector selector,
        AIQuestionGenerator generator,
        KnowledgeTracker knowledgeTracker)
    {
        this.repository = repository;
        this.selector = selector;
        this.generator = generator;
        this.knowledgeTracker = knowledgeTracker;

        initialized = true;

        RunInBackground(this.repository.InitializeAsync(), "問題データの初期化");
    }

    // ==================================================================
    // ゲーム側から使う入口
    // ==================================================================

    /// <summary>分野を決めて攻略を始める。知識と出題済みをリセットする。</summary>
    public void StartRun(string subject)
    {
        Initialize();

        // 前の攻略の生成が走っていれば打ち切る（結果が混ざらないように）
        CancelRunGeneration();
        runCancellation = new CancellationTokenSource();

        activeSubject = subject;
        usedIds.Clear();
        knowledgeTracker.Clear();
        pregenerated = null;
        Current = null;
        generatedThisRun = 0;
        candidatesLoaded = false;
        candidates = new List<QuestionData>();

        // Tomo注: ここで待たないこと。待つとサーバー実装にしたときに画面が固まります。
        //         ローカル実装は同期で終わるので、この行を抜けた時点で候補は揃っています。
        RunInBackground(LoadCandidatesAsync(), "出題候補の読み込み");
    }

    private void OnDisable() => CancelRunGeneration();

    /// 走っている生成を打ち切る。再生停止や分野の切り替えで呼ぶ。
    private void CancelRunGeneration()
    {
        if (runCancellation == null) return;

        runCancellation.Cancel();
        runCancellation.Dispose();
        runCancellation = null;
    }

    /// <summary>
    /// 待たない処理を投げる。例外を握りつぶさず、必ずConsoleに出す。
    ///
    /// Tomo注: async の結果を捨てると、中で落ちても何も表示されません。
    ///         原因不明の「生成されない」を防ぐため、ここで必ず拾います。
    /// </summary>
    private static async void RunInBackground(Task task, string label)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // 攻略を抜けたときの打ち切り。異常ではない。
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Question] " + label + "で例外が出ました: " + e.Message);
        }
    }

    /// <summary>
    /// 次の通常問題を返す。ゲーム側はこれだけ呼べばよい。
    ///
    /// 内部の流れ（企画書どおり）:
    ///   現在の問題の関連語を関連度で選ぶ
    ///     → その関連語に紐づく問題数 N を数える
    ///     → 1/N の確率で新規生成（裏で作っておいたものがあれば使う）
    ///     → 間に合っていなければ既存問題を使う
    /// </summary>
    public QuestionData GetNextQuestion()
    {
        Initialize();

        var next = PickNextQuestion();
        if (next == null) return Current;   // 1問も無いときは直前の問題を返す

        Current = next;
        usedIds.Add(next.id);

        // プレイヤーが答えている間に、次の1問を裏で作っておく
        RequestBackgroundGeneration();

        return Current;
    }

    /// <summary>
    /// 回答を記録する。正解・不正解にかかわらず「その問題に触れた」ことを知識に加える。
    /// </summary>
    public void RegisterQuestionAnswered(QuestionData question, bool isCorrect)
    {
        Initialize();

        // 企画書どおり、正解したかではなく「触れたか」で記録する
        knowledgeTracker.RegisterQuestion(question);
    }

    /// <summary>
    /// Reloadで同じ問題に再挑戦したときに呼ぶ。
    /// 企画書どおり、知識の重複としては数えない。
    /// </summary>
    public void RegisterQuestionReloaded(QuestionData question)
    {
        Initialize();

        knowledgeTracker.RegisterReload(question);
    }

    /// <summary>
    /// そのプレイ中に得た知識から、ボス問題を新しく生成する。
    /// 通常問題をそのまま流用しない。
    /// 生成できなければ null を返すので、呼び出し側でゲームを止めない扱いにすること。
    /// </summary>
    public async Task<QuestionData> GenerateBossQuestion()
    {
        Initialize();

        var counts = knowledgeTracker.GetKnowledgeCounts();
        if (counts.Count == 0)
        {
            if (verboseLog) Debug.LogWarning("[Question] 知識が1つも記録されていないので、ボス問題を作れません。");
            return null;
        }

        var request = QuestionRequest.ForBoss(activeSubject, counts);
        request.avoidQuestions = AskedQuestionTexts();

        var token = runCancellation?.Token ?? CancellationToken.None;
        var boss = await generator.GenerateAsync(request, await CollectExistingKeys(), token);
        if (boss == null) return null;

        // 企画書どおり、使う・使わないにかかわらず保存する
        await repository.SaveQuestionAsync(boss);
        return boss;
    }

    // ==================================================================
    // 内部処理
    // ==================================================================

    /// 次に出す問題を決める。すべて手元のキャッシュだけで判断する（通信を待たない）。
    private QuestionData PickNextQuestion()
    {
        // 1. 裏で生成が終わっていればそれを使う
        if (useGeneratedQuestions && pregenerated != null)
        {
            var generated = pregenerated;
            pregenerated = null;
            return generated;
        }

        // 2. 現在の問題の関連語に紐づく既存問題から選ぶ
        string term = ChooseTermFromCurrent();
        var byTerm = QuestionsWithTerm(term);
        var chosen = selector.ChooseExisting(byTerm, usedIds);
        if (chosen != null) return chosen;

        // 3. 関連語で絞ると尽きていたので、分野全体から選び直す
        chosen = selector.ChooseExisting(candidates, usedIds);
        if (chosen != null) return chosen;

        // 4. 全部出し終えた。出題済みを解除して最初から回す（デモ中に出題が尽きないように）
        usedIds.Clear();
        return selector.ChooseExisting(candidates, usedIds);
    }

    /// キャッシュの中から、指定した関連語を持つ問題を集める。
    private List<QuestionData> QuestionsWithTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return candidates;

        var matched = candidates.FindAll(q => q.HasTerm(term));
        return matched.Count > 0 ? matched : candidates;
    }

    /// <summary>
    /// 出題候補をリポジトリから読み込んでキャッシュする。
    ///
    /// Tomo注: GetNextQuestion は通信を待てないので、候補はここで先に手元へ持ってきます。
    ///         将来リポジトリをサーバー版に差し替えても、待つのはここだけで済みます。
    /// </summary>
    private async Task LoadCandidatesAsync()
    {
        await repository.InitializeAsync();

        var list = await repository.GetQuestionsBySubjectAsync(activeSubject, verifiedOnly);

        // 指定分野に問題が無いときは、分野を問わず出す（デモ中に何も出ないのを避けるため）
        if (list.Count == 0)
        {
            if (verboseLog) Debug.LogWarning("[Question] 分野「" + activeSubject + "」の問題がありません。全分野から出題します。");
            list = await repository.GetQuestionsBySubjectAsync(null, verifiedOnly);
        }

        candidates = list;
        candidatesLoaded = true;
    }

    /// 現在の問題から、関連度で重み付けして関連語を1つ選ぶ。
    private string ChooseTermFromCurrent()
    {
        if (Current == null || Current.relatedTerms == null) return null;

        var term = selector.ChooseRelatedTerm(Current.relatedTerms);
        return term?.term;
    }

    /// <summary>
    /// 裏で次の問題を作る。待たないので、ゲームはそのまま進む。
    /// 1/Nルールで「新規生成しない」と決まった場合は何もしない。
    /// </summary>
    private void RequestBackgroundGeneration()
    {
        if (!useGeneratedQuestions || isGenerating || pregenerated != null) return;
        if (generator == null || !generator.IsAvailable) return;
        if (runCancellation == null) return;

        // APIの使いすぎを防ぐ上限。Chainが伸び続けても青天井にしない。
        if (maxGenerationsPerRun > 0 && generatedThisRun >= maxGenerationsPerRun)
        {
            if (verboseLog && generatedThisRun == maxGenerationsPerRun)
            {
                Debug.Log("[Question] この攻略の生成上限(" + maxGenerationsPerRun + ")に達したので、以降は既存問題だけで進めます。");
                generatedThisRun++;   // このログを1回だけ出すため
            }
            return;
        }

        isGenerating = true;
        RunInBackground(GenerateInBackground(ChooseTermFromCurrent(), runCancellation.Token), "問題の生成");
    }

    private async Task GenerateInBackground(string relatedTerm, CancellationToken cancellationToken)
    {
        try
        {
            // 1/Nルール。その関連語に紐づく問題が少ないほど、新しく作る確率が上がる。
            int existingCount = string.IsNullOrWhiteSpace(relatedTerm)
                ? 0
                : await repository.CountQuestionsByRelatedTermAsync(relatedTerm, activeSubject);

            if (!selector.ShouldGenerateNew(existingCount)) return;

            int difficulty = QuestionRequest.DifficultyForChain(usedIds.Count);

            var request = new QuestionRequest(activeSubject, difficulty, relatedTerm)
            {
                currentQuestionText = Current?.questionText,
                avoidQuestions = AskedQuestionTexts()
            };

            generatedThisRun++;

            var generated = await generator.GenerateAsync(request, await CollectExistingKeys(), cancellationToken);
            if (generated == null) return;   // 失敗。次の機会に別の生成を行う

            // 攻略を抜けたあとに届いた結果は使わない
            if (cancellationToken.IsCancellationRequested) return;

            // 企画書どおり、今回使わなくてもDBに保存する
            await repository.SaveQuestionAsync(generated);
            candidates.Add(generated);
            pregenerated = generated;
        }
        finally
        {
            isGenerating = false;
        }
    }

    /// 既存問題の重複判定キー。AIが同じ問題を作らないようにするため。
    private async Task<List<string>> CollectExistingKeys()
    {
        var all = await repository.GetQuestionsBySubjectAsync(null);
        var keys = new List<string>();
        foreach (var question in all) keys.Add(question.IdentityKey);
        return keys;
    }

    /// この攻略ですでに出した問題文。プロンプトに渡して重複を避ける。
    private List<string> AskedQuestionTexts()
    {
        var texts = new List<string>();
        foreach (var question in candidates)
        {
            if (usedIds.Contains(question.id)) texts.Add(question.questionText);
        }
        return texts;
    }
}
