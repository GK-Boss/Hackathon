using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 出題する問題を決める。
///
/// Tomo注: AI生成が間に合っていればAIの問題を、間に合わなければ下の固定問題を出します。
///         固定問題はデモ中にAPIが落ちても遊べるようにするための保険なので消さないでください。
/// </summary>
public class QuestionManager : MonoBehaviour
{
    [Tooltip("未設定なら固定問題だけで動く。")]
    [SerializeField] private AIQuestionGenerator aiGenerator;

    private readonly List<QuestionData> unityQuestions = new List<QuestionData>
    {
        new QuestionData("Unityで2D画像を表示するコンポーネントは？", new[] { "SpriteRenderer", "AudioSource", "Light", "Animator", "Camera" }, 0, "SpriteRendererはSpriteアセットを画面に描画します。", "SpriteRenderer"),
        new QuestionData("ゲーム開始時に一度だけ呼ばれるUnityメソッドは？", new[] { "Start", "Update", "LateUpdate", "FixedUpdate", "OnGUI" }, 0, "Startは有効なコンポーネントの初期化時に一度呼ばれます。", "ライフサイクル"),
        new QuestionData("UIボタンのクリック処理を登録するイベントは？", new[] { "onClick", "onDraw", "onMove", "onLoad", "onTick" }, 0, "Button.onClickにリスナーを登録するとクリック時に処理を呼べます。", "UIイベント"),
        new QuestionData("2D物理演算で使うコンポーネントは？", new[] { "Rigidbody2D", "Rigidbody3D", "PhysicsCamera", "ColliderUI", "ForceManager" }, 0, "Rigidbody2Dは2D物理演算の質量や速度を管理します。", "2D Physics")
    };

    private readonly List<QuestionData> generalQuestions = new List<QuestionData>
    {
        new QuestionData("水が海面で蒸発して雲になる過程は？", new[] { "蒸発", "凝固", "融解", "沈殿", "燃焼" }, 0, "蒸発は液体が気体へ変化する現象です。", "水循環"),
        new QuestionData("日本の国鳥は？", new[] { "キジ", "ツル", "ハト", "カラス", "ワシ" }, 0, "キジは日本の国鳥です。", "日本文化"),
        new QuestionData("光合成で植物が空気中から取り込む気体は？", new[] { "二酸化炭素", "酸素", "窒素", "水素", "ヘリウム" }, 0, "植物は二酸化炭素と水から養分を作ります。", "生物")
    };

    private List<QuestionData> activeQuestions;
    private string activeSubject;

    /// AI生成で差し替えている問題。null のときは固定問題を使う。
    private QuestionData generatedQuestion;

    public int CurrentIndex { get; private set; }

    /// <summary>今出すべき問題。AI生成が間に合っていればそちらを優先する。</summary>
    public QuestionData Current => generatedQuestion ?? activeQuestions[CurrentIndex % activeQuestions.Count];

    /// <summary>分野を決めて1問目を用意する。</summary>
    public void SelectSubject(string subject)
    {
        activeSubject = subject;
        activeQuestions = subject == "Unity" ? unityQuestions : generalQuestions;
        CurrentIndex = 0;
        generatedQuestion = null;

        // 1問目は固定問題で即座に出し、その裏で2問目の生成を始める（待たせないため）
        RequestNextGeneration(0);
    }

    /// <summary>
    /// 次の問題へ進む。
    /// AI生成が間に合っていればそれを使い、間に合っていなければ固定問題を順番に出す。
    /// </summary>
    /// <param name="chain">現在のChain数。難易度の指定に使う。</param>
    public void MoveNext(int chain)
    {
        string previousTerm = Current.relatedTerm;

        if (aiGenerator != null && aiGenerator.TryTakeGenerated(out var generated))
        {
            generatedQuestion = generated;
        }
        else
        {
            generatedQuestion = null;
            CurrentIndex = (CurrentIndex + 1) % activeQuestions.Count;
        }

        // さらに次の問題を先読みしておく
        RequestNextGeneration(chain, previousTerm);
    }

    /// 次の問題の生成をバックグラウンドで依頼する。AI未設定なら何も起きない。
    private void RequestNextGeneration(int chain, string relatedTerm = null)
    {
        if (aiGenerator == null) return;
        aiGenerator.Prefetch(activeSubject, chain, relatedTerm ?? Current.relatedTerm);
    }
}
