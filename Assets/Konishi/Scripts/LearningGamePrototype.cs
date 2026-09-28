using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// ハッカソン用の最小プレイアブルプロトタイプ。
/// 外部APIを使わず固定問題で、分野選択から結果表示までのコアループを確認できます。
/// </summary>
public class LearningGamePrototype : MonoBehaviour
{
    [Serializable]
    private class Question
    {
        public string prompt;
        public string[] choices;
        public int answerIndex;
        public string explanation;
        public string relatedTerm;

        public Question(string prompt, string[] choices, int answerIndex, string explanation, string relatedTerm)
        {
            this.prompt = prompt;
            this.choices = choices;
            this.answerIndex = answerIndex;
            this.explanation = explanation;
            this.relatedTerm = relatedTerm;
        }
    }

    private readonly Color background = new Color(0.035f, 0.055f, 0.12f);
    private readonly Color panel = new Color(0.075f, 0.105f, 0.20f);
    private readonly Color accent = new Color(0.30f, 0.78f, 1f);
    private readonly Color gold = new Color(1f, 0.73f, 0.25f);
    private readonly Color danger = new Color(1f, 0.32f, 0.40f);

    private Canvas canvas;
    private Font font;
    private Text titleText;
    private Text statusText;
    private Text questionText;
    private Text explanationText;
    private Text characterText;
    private GameObject content;
    private int chain;
    private int questionIndex;
    private string selectedSubject;
    private List<Question> questions;

    private void Start()
    {
        // Unity 6ではArial.ttfが組み込みフォントとして廃止されています。
        // 実行時に利用できるLegacyRuntime.ttfを使用します。
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem();
        BuildCanvas();
        ShowSubjectSelection();
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        // Project SettingsがInput System Package (New)のため、
        // 旧StandaloneInputModuleではなく新Input System対応モジュールを使用します。
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("LearningGameCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var backdrop = CreatePanel(canvas.transform, "Backdrop", background, Vector2.zero, Vector2.one);
        backdrop.transform.SetAsFirstSibling();
        content = new GameObject("ScreenContent");
        content.transform.SetParent(canvas.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.08f, 0.04f);
        contentRect.anchorMax = new Vector2(0.92f, 0.96f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
    }

    private void ClearContent()
    {
        for (int i = content.transform.childCount - 1; i >= 0; i--)
            Destroy(content.transform.GetChild(i).gameObject);
    }

    private void ShowSubjectSelection()
    {
        ClearContent();
        titleText = CreateText(content.transform, "Title", "わからないは、強さだ。", 64, gold, TextAnchor.MiddleCenter);
        SetRect(titleText.rectTransform, new Vector2(0, 0.86f), new Vector2(1, 0.98f));
        var subtitle = CreateText(content.transform, "Subtitle", "知識の旅を始めよう", 28, Color.white, TextAnchor.MiddleCenter);
        SetRect(subtitle.rectTransform, new Vector2(0, 0.79f), new Vector2(1, 0.87f));

        characterText = CreateText(content.transform, "Character", "◆\n  ◇  \n小さな知識の精霊", 42, accent, TextAnchor.MiddleCenter);
        SetRect(characterText.rectTransform, new Vector2(0.15f, 0.47f), new Vector2(0.85f, 0.77f));

        var prompt = CreateText(content.transform, "Prompt", "挑戦する分野を選択", 32, Color.white, TextAnchor.MiddleCenter);
        SetRect(prompt.rectTransform, new Vector2(0, 0.38f), new Vector2(1, 0.47f));
        CreateSubjectButton("Unity / ゲーム開発", "Unity", 0.28f);
        CreateSubjectButton("一般教養", "一般教養", 0.17f);
        CreateSubjectButton("プログラミング", "プログラミング", 0.06f);
        statusText = CreateText(content.transform, "Hint", "正解を重ねるほど、Chainが伸びる。\n「わかりません」でChainをスコアに変換できます。", 22, new Color(0.70f, 0.76f, 0.90f), TextAnchor.MiddleCenter);
        SetRect(statusText.rectTransform, new Vector2(0, 0.0f), new Vector2(1, 0.06f));
    }

    private void CreateSubjectButton(string label, string subject, float y)
    {
        var button = CreateButton(content.transform, label, accent, 28);
        SetRect(button.GetComponent<RectTransform>(), new Vector2(0.08f, y), new Vector2(0.92f, y + 0.085f));
        button.onClick.AddListener(() => StartGame(subject));
    }

    private void StartGame(string subject)
    {
        selectedSubject = subject;
        chain = 0;
        questionIndex = 0;
        questions = BuildQuestions(subject);
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        ClearContent();
        var header = CreateText(content.transform, "Header", selectedSubject, 26, accent, TextAnchor.MiddleLeft);
        SetRect(header.rectTransform, new Vector2(0.02f, 0.92f), new Vector2(0.55f, 0.99f));
        statusText = CreateText(content.transform, "Chain", "CHAIN  " + chain, 34, gold, TextAnchor.MiddleRight);
        SetRect(statusText.rectTransform, new Vector2(0.50f, 0.92f), new Vector2(0.98f, 0.99f));

        var q = questions[questionIndex % questions.Count];
        var difficulty = Math.Min(5, chain + 1);
        var difficultyText = CreateText(content.transform, "Difficulty", "DIFFICULTY  " + new string('★', difficulty), 20, new Color(1f, 0.85f, 0.35f), TextAnchor.MiddleCenter);
        SetRect(difficultyText.rectTransform, new Vector2(0, 0.83f), new Vector2(1, 0.90f));
        questionText = CreateText(content.transform, "Question", q.prompt, 38, Color.white, TextAnchor.MiddleCenter);
        questionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        SetRect(questionText.rectTransform, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.82f));

        for (int i = 0; i < q.choices.Length; i++)
        {
            int choice = i;
            var button = CreateButton(content.transform, (i + 1) + "  " + q.choices[i], panel, 25);
            float y = 0.50f - i * 0.085f;
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.04f, y), new Vector2(0.96f, y + 0.068f));
            button.onClick.AddListener(() => Answer(choice));
        }

        var dontKnow = CreateButton(content.transform, "？  わかりません（Chainをスコア化）", new Color(0.23f, 0.16f, 0.36f), 23);
        SetRect(dontKnow.GetComponent<RectTransform>(), new Vector2(0.04f, 0.035f), new Vector2(0.96f, 0.105f));
        dontKnow.onClick.AddListener(() => AdmitUnknown());
    }

    private void Answer(int choice)
    {
        var q = questions[questionIndex % questions.Count];
        if (choice != q.answerIndex)
        {
            ShowResult(false, "不正解…", "Chainはすべて失われました。\n正解: " + q.choices[q.answerIndex]);
            return;
        }

        chain++;
        questionIndex++;
        if (questionIndex >= questions.Count) questionIndex = 0;
        ShowFeedback(true, "正解！  Chain + 1", "関連ワード：" + q.relatedTerm);
    }

    private void AdmitUnknown()
    {
        var q = questions[questionIndex % questions.Count];
        int score = chain * chain;
        ShowResult(true, "わからないを認めた！", "Chain " + chain + " → スコア " + score + "\n\n" + q.explanation);
    }

    private void ShowFeedback(bool correct, string message, string detail)
    {
        ClearContent();
        var result = CreateText(content.transform, "Feedback", message, 52, correct ? accent : danger, TextAnchor.MiddleCenter);
        SetRect(result.rectTransform, new Vector2(0, 0.56f), new Vector2(1, 0.76f));
        var detailText = CreateText(content.transform, "Detail", detail, 28, Color.white, TextAnchor.MiddleCenter);
        SetRect(detailText.rectTransform, new Vector2(0, 0.43f), new Vector2(1, 0.56f));
        characterText = CreateText(content.transform, "Character", chain >= 3 ? "◆  ◇  ◆\n精霊がはっきり見えてきた！" : "◆\n精霊の輪郭が少し見えた！", 38, gold, TextAnchor.MiddleCenter);
        SetRect(characterText.rectTransform, new Vector2(0, 0.70f), new Vector2(1, 0.90f));
        var next = CreateButton(content.transform, "次の問題へ", accent, 30);
        SetRect(next.GetComponent<RectTransform>(), new Vector2(0.12f, 0.16f), new Vector2(0.88f, 0.25f));
        next.onClick.AddListener(ShowQuestion);
    }

    private void ShowResult(bool honest, string headline, string detail)
    {
        ClearContent();
        var result = CreateText(content.transform, "Result", headline, 48, honest ? gold : danger, TextAnchor.MiddleCenter);
        SetRect(result.rectTransform, new Vector2(0, 0.65f), new Vector2(1, 0.82f));
        var detailText = CreateText(content.transform, "ResultDetail", detail, 28, Color.white, TextAnchor.MiddleCenter);
        detailText.horizontalOverflow = HorizontalWrapMode.Wrap;
        SetRect(detailText.rectTransform, new Vector2(0.05f, 0.43f), new Vector2(0.95f, 0.65f));
        var score = CreateText(content.transform, "Score", "FINAL SCORE  " + (honest ? chain * chain : 0), 38, gold, TextAnchor.MiddleCenter);
        SetRect(score.rectTransform, new Vector2(0, 0.30f), new Vector2(1, 0.40f));
        var again = CreateButton(content.transform, "もう一度挑戦", accent, 30);
        SetRect(again.GetComponent<RectTransform>(), new Vector2(0.12f, 0.13f), new Vector2(0.88f, 0.23f));
        again.onClick.AddListener(ShowSubjectSelection);
    }

    private List<Question> BuildQuestions(string subject)
    {
        if (subject == "Unity")
        {
            return new List<Question>
            {
                new Question("Unityで2D画像を表示するコンポーネントは？", new[] { "SpriteRenderer", "AudioSource", "Light", "Animator", "Camera" }, 0, "SpriteRendererはSpriteアセットを画面に描画するためのコンポーネントです。", "SpriteRenderer"),
                new Question("ゲーム開始時に一度だけ呼ばれるUnityメソッドは？", new[] { "Start", "Update", "LateUpdate", "FixedUpdate", "OnGUI" }, 0, "Startは有効なコンポーネントの初期化時に一度呼ばれます。", "ライフサイクル"),
                new Question("UIボタンのクリック処理を登録するイベントは？", new[] { "onClick", "onDraw", "onMove", "onLoad", "onTick" }, 0, "Button.onClickにリスナーを登録すると、クリック時に処理を呼べます。", "UIイベント"),
                new Question("2D物理演算で使うコンポーネントは？", new[] { "Rigidbody2D", "Rigidbody3D", "PhysicsCamera", "ColliderUI", "ForceManager" }, 0, "Rigidbody2Dは2D物理演算の質量や速度を管理します。", "2D Physics"),
            };
        }

        return new List<Question>
        {
            new Question("水が海面で蒸発して雲になる過程は？", new[] { "蒸発", "凝固", "融解", "沈殿", "燃焼" }, 0, "蒸発は液体が気体へ変化する現象です。", "水循環"),
            new Question("日本の国鳥は？", new[] { "キジ", "ツル", "ハト", "カラス", "ワシ" }, 0, "キジは1947年に日本の国鳥として選ばれました。", "日本文化"),
            new Question("光合成で植物が空気中から取り込む気体は？", new[] { "二酸化炭素", "酸素", "窒素", "水素", "ヘリウム" }, 0, "植物は二酸化炭素と水から、光のエネルギーで養分を作ります。", "生物"),
        };
    }

    private Text CreateText(Transform parent, string name, string value, int size, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = anchor;
        text.raycastTarget = false;
        // Best Fitは実行時にフォントを縮小再生成するため、文字がぼやけることがあります。
        // 固定サイズで描画し、CanvasScaler側で画面サイズに合わせます。
        text.resizeTextForBestFit = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private Button CreateButton(Transform parent, string label, Color color, int size)
    {
        var go = new GameObject("Button_" + label);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        var labelText = CreateText(go.transform, "Label", label, size, Color.white, TextAnchor.MiddleCenter);
        var rect = labelText.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(12, 4);
        rect.offsetMax = new Vector2(-12, -4);
        return button;
    }

    private Image CreatePanel(Transform parent, string name, Color color, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        SetRect(image.rectTransform, min, max);
        return image;
    }

    private void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
