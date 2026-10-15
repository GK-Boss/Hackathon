#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 問題まわりのSceneオブジェクトと参照を自動で用意する。
///
/// Tomo注: ゲーム側から見える窓口は QuestionManager だけなので、
///         Sceneに置くのも QuestionManager 1つだけです。
///         AI通信やリポジトリは QuestionManager の中で組み立てられます。
///
///         何度実行しても同じ結果になります。
///         スクリプト名を変えてSceneの参照が外れたときも、これを実行すれば直ります。
/// </summary>
public static class QuestionSystemSetup
{
    private const string ScenePath = "Assets/Common/Scene/CommonScene.unity";
    private const string SystemsName = "Systems";
    private const string ManagerName = "QuestionManager";
    private const string ObsoleteAIClientName = "AIClient";

    [MenuItem("Tools/Learning Game/Setup Question System")]
    public static void SetupQuestionSystem()
    {
        OpenAISettingsCreator.CreateSettings();

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        var systems = GameObject.Find(SystemsName);
        if (systems == null)
        {
            Debug.LogError("[Question] Hierarchyに " + SystemsName + " が見つかりません。先に Create Scene UI を実行してください。");
            return;
        }

        // 旧構成で置いていた AIClient は不要になったので片付ける
        var obsolete = systems.transform.Find(ObsoleteAIClientName);
        if (obsolete != null)
        {
            Object.DestroyImmediate(obsolete.gameObject);
            Debug.Log("[Question] 不要になった " + ObsoleteAIClientName + " を削除しました。");
        }

        var manager = EnsureComponent<QuestionManager>(systems, ManagerName);

        var flow = Object.FindFirstObjectByType<GameFlowController>();
        if (flow == null)
        {
            Debug.LogError("[Question] SceneにGameFlowControllerが見つかりません。参照の割り当てをスキップしました。");
        }
        else
        {
            AssignField(flow, "questionManager", manager);
            AssignMissingPanels(flow);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[Question] セットアップが完了しました。接続先の設定は AIClientSettings で行ってください。");
    }

    /// <summary>
    /// GameFlowController の空になっているパネル参照を、同じ名前のGameObjectで埋める。
    ///
    /// Tomo注: パネルを足したときにInspectorでの割り当てを忘れると、
    ///         再生した瞬間に NullReferenceException で落ちます。
    ///         フィールド名の先頭を大文字にした名前のGameObjectを探して入れます
    ///         （modeSelectPanel → ModeSelectPanel）。
    ///         すでに埋まっているものには触りません。
    /// </summary>
    private static void AssignMissingPanels(GameFlowController flow)
    {
        string[] panelFields =
        {
            "modeSelectPanel", "subjectSelectPanel", "quizPanel", "feedbackPanel", "resultPanel"
        };

        var serialized = new SerializedObject(flow);
        bool changed = false;

        foreach (string fieldName in panelFields)
        {
            var property = serialized.FindProperty(fieldName);
            if (property == null || property.objectReferenceValue != null) continue;

            string objectName = char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
            var panel = FindInScene(objectName);

            if (panel == null)
            {
                Debug.LogWarning("[Question] " + fieldName + " が空ですが、「" + objectName
                                 + "」という名前のGameObjectが見つかりません。Inspectorで割り当ててください。");
                continue;
            }

            property.objectReferenceValue = panel;
            changed = true;
            Debug.Log("[Question] GameFlowController." + fieldName + " に " + objectName + " を割り当てました。");
        }

        if (changed) serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    /// 非アクティブなパネルも対象にするため、Transformを総なめして探す。
    private static GameObject FindInScene(string objectName)
    {
        foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name == objectName) return transform.gameObject;
        }
        return null;
    }

    /// Systems の下に指定名のGameObjectを用意し、コンポーネントを付けて返す。すでにあれば作り直さない。
    private static T EnsureComponent<T>(GameObject systems, string objectName) where T : Component
    {
        var child = systems.transform.Find(objectName);
        GameObject target;

        if (child == null)
        {
            target = new GameObject(objectName);
            target.transform.SetParent(systems.transform, false);
            Undo.RegisterCreatedObjectUndo(target, "Create " + objectName);
        }
        else
        {
            target = child.gameObject;
        }

        return target.GetComponent<T>() ?? target.AddComponent<T>();
    }

    /// private [SerializeField] なフィールドに参照を設定して保存する。
    private static void AssignField(Object target, string fieldName, Object value)
    {
        var serialized = new SerializedObject(target);
        var property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogError("[Question] " + target.GetType().Name + " に " + fieldName + " が見つかりません。");
            return;
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
