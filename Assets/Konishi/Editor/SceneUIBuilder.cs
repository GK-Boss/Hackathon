#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class SceneUIBuilder
{
    [MenuItem("Tools/Learning Game/Create Scene UI")]
    public static void CreateSceneUI()
    {
        var oldCanvas = GameObject.Find("LearningGameCanvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        var canvasObject = new GameObject("LearningGameCanvas");
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Learning Game Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var backdrop = CreatePanel(canvasObject.transform, "Backdrop", new Color(0.035f, 0.055f, 0.12f));
        Full(backdrop);
        var subject = CreatePanel(canvasObject.transform, "SubjectSelectPanel", Color.clear);
        Full(subject);
        Text(subject.transform, "TitleText", "わからないは、強さだ。", 64, new Color(1f, .73f, .25f), .80f, .96f);
        Text(subject.transform, "SubtitleText", "知識の旅を始めよう", 28, Color.white, .74f, .81f);
        Text(subject.transform, "PromptText", "挑戦する分野を選択", 32, Color.white, .36f, .44f);
        AddButton(subject.transform, "SubjectButton_Unity", "Unity / ゲーム開発", .25f, .34f);
        AddButton(subject.transform, "SubjectButton_General", "一般教養", .15f, .24f);
        AddButton(subject.transform, "SubjectButton_Programming", "プログラミング", .05f, .14f);

        var quiz = CreatePanel(canvasObject.transform, "QuizPanel", Color.clear);
        Full(quiz);
        Text(quiz.transform, "SubjectText", "Unity", 28, Color.cyan, .05f, .96f, TextAnchor.MiddleLeft);
        Text(quiz.transform, "ChainText", "CHAIN  0", 34, new Color(1f, .73f, .25f), .55f, .96f, TextAnchor.MiddleRight);
        var questionText = Text(quiz.transform, "QuestionText", "問題文", 38, Color.white, .58f, .79f);
        var answerTexts = new Text[5];
        var answerButtons = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            float top = .50f - i * .085f;
            answerButtons[i] = AddButton(quiz.transform, "AnswerButton_" + (i + 1), "選択肢 " + (i + 1), top - .065f, top);
            answerTexts[i] = answerButtons[i].GetComponentInChildren<Text>();
        }
        var dontKnowButton = AddButton(quiz.transform, "DontKnowButton", "？  わかりません（Chainをスコア化）", .03f, .105f);
        quiz.SetActive(false);
        var questionUI = quiz.AddComponent<QuestionUI>();
        Assign(questionUI, "questionText", questionText);
        Assign(questionUI, "subjectText", quiz.transform.Find("SubjectText").GetComponent<Text>());
        Assign(questionUI, "chainText", quiz.transform.Find("ChainText").GetComponent<Text>());
        Assign(questionUI, "answerTexts", answerTexts);
        Assign(questionUI, "answerButtons", answerButtons);
        Assign(questionUI, "dontKnowButton", dontKnowButton);

        var feedback = CreatePanel(canvasObject.transform, "FeedbackPanel", Color.clear);
        Full(feedback);
        Text(feedback.transform, "FeedbackText", "正解！", 52, Color.cyan, .56f, .76f);
        Text(feedback.transform, "DetailText", "関連ワード", 28, Color.white, .42f, .56f);
        var nextButton = AddButton(feedback.transform, "NextButton", "次の問題へ", .16f, .25f);
        var feedbackUI = feedback.AddComponent<FeedbackUI>();
        Assign(feedbackUI, "feedbackText", feedback.transform.Find("FeedbackText").GetComponent<Text>());
        Assign(feedbackUI, "detailText", feedback.transform.Find("DetailText").GetComponent<Text>());
        Assign(feedbackUI, "nextButton", nextButton);
        feedback.SetActive(false);

        var result = CreatePanel(canvasObject.transform, "ResultPanel", Color.clear);
        Full(result);
        Text(result.transform, "ResultTitleText", "結果", 48, new Color(1f, .73f, .25f), .65f, .82f);
        Text(result.transform, "ResultDetailText", "詳細", 28, Color.white, .42f, .65f);
        Text(result.transform, "ScoreText", "FINAL SCORE  0", 38, new Color(1f, .73f, .25f), .30f, .40f);
        var retryButton = AddButton(result.transform, "RetryButton", "もう一度挑戦", .13f, .23f);
        var resultUI = result.AddComponent<ResultUI>();
        Assign(resultUI, "titleText", result.transform.Find("ResultTitleText").GetComponent<Text>());
        Assign(resultUI, "detailText", result.transform.Find("ResultDetailText").GetComponent<Text>());
        Assign(resultUI, "scoreText", result.transform.Find("ScoreText").GetComponent<Text>());
        Assign(resultUI, "retryButton", retryButton);
        result.SetActive(false);

        var systems = GameObject.Find("Systems") ?? new GameObject("Systems");
        var chain = AddComponent<ChainManager>(systems, "ChainManager");
        var score = AddComponent<ScoreCalculator>(systems, "ScoreCalculator");
        var questions = AddComponent<QuestionManager>(systems, "QuestionManager");
        var flowObject = new GameObject("GameFlowController");
        flowObject.transform.SetParent(systems.transform, false);
        var flow = flowObject.AddComponent<GameFlowController>();
        Assign(flow, "chainManager", chain);
        Assign(flow, "scoreCalculator", score);
        Assign(flow, "questionManager", questions);
        Assign(flow, "subjectSelectPanel", subject);
        Assign(flow, "quizPanel", quiz);
        Assign(flow, "feedbackPanel", feedback);
        Assign(flow, "resultPanel", result);
        Assign(flow, "questionUI", questionUI);
        Assign(flow, "feedbackUI", feedbackUI);
        Assign(flow, "resultUI", resultUI);

        BindSubjectButtons(subject, flow);
        EnsureEventSystem();
        Selection.activeGameObject = canvasObject;
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Learning Game UIをScene上に作成しました。Inspectorから自由に調整できます。");
    }

    private static void BindSubjectButtons(GameObject subject, GameFlowController flow)
    {
        Bind(subject, "SubjectButton_Unity", flow.SelectUnity);
        Bind(subject, "SubjectButton_General", flow.SelectGeneral);
        Bind(subject, "SubjectButton_Programming", flow.SelectProgramming);
    }

    private static void Bind(GameObject root, string name, UnityEngine.Events.UnityAction action)
    {
        root.transform.Find(name).GetComponent<Button>().onClick.AddListener(action);
    }

    private static void EnsureEventSystem()
    {
        var eventSystem = Object.FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
        else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            var old = eventSystem.GetComponent<StandaloneInputModule>();
            if (old != null) Object.DestroyImmediate(old);
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go;
    }

    private static Text Text(Transform parent, string name, string value, int size, Color color, float minY, float maxY, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        Rect(go).anchorMin = new Vector2(.03f, minY);
        Rect(go).anchorMax = new Vector2(.97f, maxY);
        Rect(go).offsetMin = Vector2.zero;
        Rect(go).offsetMax = Vector2.zero;
        return text;
    }

    private static Button AddButton(Transform parent, string name, string label, float minY, float maxY)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = new Color(.12f, .22f, .38f);
        var button = go.AddComponent<Button>();
        FullY(go, minY, maxY);
        Text(go.transform, "Label", label, 26, Color.white, 0f, 1f);
        return button;
    }

    private static void Full(GameObject go) { Rect(go).anchorMin = Vector2.zero; Rect(go).anchorMax = Vector2.one; Rect(go).offsetMin = Vector2.zero; Rect(go).offsetMax = Vector2.zero; }
    private static void FullY(GameObject go, float minY, float maxY) { Rect(go).anchorMin = new Vector2(.04f, minY); Rect(go).anchorMax = new Vector2(.96f, maxY); Rect(go).offsetMin = Vector2.zero; Rect(go).offsetMax = Vector2.zero; }
    private static RectTransform Rect(GameObject go) => go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
    private static T AddComponent<T>(GameObject parent, string name) where T : Component { var go = new GameObject(name); go.transform.SetParent(parent.transform, false); return go.AddComponent<T>(); }
    private static void Assign(Object target, string property, Object value) { var so = new SerializedObject(target); so.FindProperty(property).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    private static void Assign(Object target, string property, Object[] values) { var so = new SerializedObject(target); var p = so.FindProperty(property); p.arraySize = values.Length; for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; so.ApplyModifiedPropertiesWithoutUndo(); }
}
#endif
