#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// AI連携に必要なSceneオブジェクトと参照を自動で用意する。
///
/// Tomo注: Hierarchyに AIClient を手作業で置く代わりに、このメニューを1回実行すれば済むようにしています。
///         何度実行しても同じ結果になります（すでにある場合は作り直さない）。
///         APIキーだけは自動で入れられないので、各自がInspectorで設定してください。
/// </summary>
public static class AIClientSetup
{
    private const string ScenePath = "Assets/Common/Scene/CommonScene.unity";
    private const string SystemsName = "Systems";
    private const string AIClientName = "AIClient";

    [MenuItem("Tools/Learning Game/Setup AI Client")]
    public static void SetupAIClient()
    {
        // 設定アセットが無ければ先に作る（APIキーは空のまま）
        OpenAISettingsCreator.CreateSettings();

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        var systems = GameObject.Find(SystemsName);
        if (systems == null)
        {
            Debug.LogError("[AI] Hierarchyに " + SystemsName + " が見つかりません。先に Create Scene UI を実行してください。");
            return;
        }

        // Systems/AIClient を用意する
        var clientTransform = systems.transform.Find(AIClientName);
        GameObject client;
        if (clientTransform == null)
        {
            client = new GameObject(AIClientName);
            client.transform.SetParent(systems.transform, false);
            Undo.RegisterCreatedObjectUndo(client, "Create AI Client");
        }
        else
        {
            client = clientTransform.gameObject;
        }

        var apiClient = client.GetComponent<AIAPIClient>() ?? client.AddComponent<AIAPIClient>();
        var generator = client.GetComponent<AIQuestionGenerator>() ?? client.AddComponent<AIQuestionGenerator>();

        // AIQuestionGenerator から AIAPIClient への参照を埋める（private [SerializeField] のため SerializedObject を使う）
        AssignField(generator, "apiClient", apiClient);

        // QuestionManager から AIQuestionGenerator への参照を埋める
        var questionManager = Object.FindFirstObjectByType<QuestionManager>();
        if (questionManager == null)
        {
            Debug.LogError("[AI] SceneにQuestionManagerが見つかりません。参照の割り当てをスキップしました。");
        }
        else
        {
            AssignField(questionManager, "aiGenerator", generator);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[AI] AIClientのセットアップが完了しました。残りの作業はOpenAISettingsにAPIキーを入れるだけです。");
    }

    /// private [SerializeField] なフィールドに参照を設定して保存する。
    private static void AssignField(Object target, string fieldName, Object value)
    {
        var serialized = new SerializedObject(target);
        var property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogError("[AI] " + target.GetType().Name + " に " + fieldName + " が見つかりません。");
            return;
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
