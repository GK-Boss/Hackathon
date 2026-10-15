#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene上の選択肢ボタンの数を QuestionRequest.DefaultChoiceCount に合わせる。
///
/// Tomo注: Create Scene UI はUIを丸ごと作り直すので、手で整えた配置が消えます。
///         こちらは足りないボタンを複製して足すだけなので、既存の見た目を保てます。
///
///         追加したボタンは、直前のボタンと同じ間隔で下にずらして置きます。
///         細かい位置はInspectorで調整してください。
///         何度実行しても、必要な数に達していれば何もしません。
/// </summary>
public static class ChoiceCountMigration
{
    private const string ScenePath = "Assets/Common/Scene/CommonScene.unity";

    [MenuItem("Tools/Learning Game/Match Choice Buttons To Spec")]
    public static void MatchChoiceButtons()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var questionUI = Object.FindFirstObjectByType<QuestionUI>(FindObjectsInactive.Include);
        if (questionUI == null)
        {
            Debug.LogError("[Question] SceneにQuestionUIが見つかりません。先に Create Scene UI を実行してください。");
            return;
        }

        var serialized = new SerializedObject(questionUI);
        var buttonsProperty = serialized.FindProperty("answerButtons");
        var textsProperty = serialized.FindProperty("answerTexts");

        int target = QuestionRequest.DefaultChoiceCount;
        int current = buttonsProperty.arraySize;

        if (current == target)
        {
            Debug.Log("[Question] 選択肢ボタンはすでに" + target + "個です。何もしませんでした。");
            return;
        }

        if (current > target)
        {
            Debug.LogWarning("[Question] ボタンが" + current + "個あり、仕様の" + target
                             + "個より多いです。余分なボタンは手で削除してください（消す対象の判断は人がしてください）。");
            return;
        }

        if (current < 2)
        {
            Debug.LogError("[Question] 複製元にするボタンが足りません。Create Scene UI を実行してください。");
            return;
        }

        // 直前2つの間隔を読み、同じ間隔で下に並べていく
        var last = (Button)buttonsProperty.GetArrayElementAtIndex(current - 1).objectReferenceValue;
        var secondLast = (Button)buttonsProperty.GetArrayElementAtIndex(current - 2).objectReferenceValue;
        if (last == null || secondLast == null)
        {
            Debug.LogError("[Question] answerButtons に未割り当ての要素があります。先にInspectorで埋めてください。");
            return;
        }

        var lastRect = last.GetComponent<RectTransform>();
        var secondLastRect = secondLast.GetComponent<RectTransform>();
        Vector2 minStep = lastRect.anchorMin - secondLastRect.anchorMin;
        Vector2 maxStep = lastRect.anchorMax - secondLastRect.anchorMax;
        Vector2 posStep = lastRect.anchoredPosition - secondLastRect.anchoredPosition;

        var previousRect = lastRect;

        for (int i = current; i < target; i++)
        {
            var copy = Object.Instantiate(last.gameObject, last.transform.parent);
            copy.name = "AnswerButton_" + (i + 1);
            Undo.RegisterCreatedObjectUndo(copy, "Add Choice Button");

            var rect = copy.GetComponent<RectTransform>();
            rect.anchorMin = previousRect.anchorMin + minStep;
            rect.anchorMax = previousRect.anchorMax + maxStep;
            rect.anchoredPosition = previousRect.anchoredPosition + posStep;
            rect.sizeDelta = previousRect.sizeDelta;
            previousRect = rect;

            var button = copy.GetComponent<Button>();
            var text = copy.GetComponentInChildren<Text>(true);
            if (text != null) text.text = "選択肢 " + (i + 1);

            buttonsProperty.InsertArrayElementAtIndex(buttonsProperty.arraySize);
            buttonsProperty.GetArrayElementAtIndex(buttonsProperty.arraySize - 1).objectReferenceValue = button;

            textsProperty.InsertArrayElementAtIndex(textsProperty.arraySize);
            textsProperty.GetArrayElementAtIndex(textsProperty.arraySize - 1).objectReferenceValue = text;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[Question] 選択肢ボタンを" + current + "個から" + target
                  + "個に増やしました。位置はInspectorで微調整してください。");
    }
}
#endif
