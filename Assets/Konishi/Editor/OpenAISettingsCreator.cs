#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// OpenAISettings アセットをローカルに作るためのエディタメニュー。
///
/// Tomo注: このアセットはAPIキーを含むため .gitignore で除外しています。
///         そのためリポジトリをクローンした人は最初にこのメニューを1回実行してください。
/// </summary>
public static class OpenAISettingsCreator
{
    private const string FolderPath = "Assets/Konishi/Resources";
    private const string AssetPath = FolderPath + "/OpenAISettings.asset";

    [MenuItem("Tools/Learning Game/Create OpenAI Settings")]
    public static void CreateSettings()
    {
        var existing = AssetDatabase.LoadAssetAtPath<OpenAISettings>(AssetPath);
        if (existing != null)
        {
            // すでにある場合は作り直さず選択だけする（入力済みのAPIキーを消さないため）
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            Debug.Log("[AI] OpenAISettingsはすでに存在します: " + AssetPath);
            return;
        }

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
            AssetDatabase.Refresh();
        }

        var settings = ScriptableObject.CreateInstance<OpenAISettings>();
        AssetDatabase.CreateAsset(settings, AssetPath);
        AssetDatabase.SaveAssets();

        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
        Debug.Log("[AI] OpenAISettingsを作成しました。InspectorでAPIキーを設定してください: " + AssetPath);
    }
}
#endif
