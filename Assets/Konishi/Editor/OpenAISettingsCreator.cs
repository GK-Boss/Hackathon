#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AIClientSettings アセットをローカルに作るためのエディタメニュー。
///
/// Tomo注: このアセットは接続情報を含むため .gitignore で除外しています。
///         リポジトリをクローンした人は最初に1回実行してください。
///         キーを入れなくても、ゲームは既存問題で動きます。
/// </summary>
public static class OpenAISettingsCreator
{
    private const string FolderPath = "Assets/Konishi/Resources";
    private const string AssetPath = FolderPath + "/AIClientSettings.asset";

    [MenuItem("Tools/Learning Game/Create AI Client Settings")]
    public static void CreateSettings()
    {
        var existing = AssetDatabase.LoadAssetAtPath<AIClientSettings>(AssetPath);
        if (existing != null)
        {
            // すでにある場合は作り直さず選択だけする（入力済みの設定を消さないため）
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            Debug.Log("[AI] AIClientSettingsはすでに存在します: " + AssetPath);
            return;
        }

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
            AssetDatabase.Refresh();
        }

        var settings = ScriptableObject.CreateInstance<AIClientSettings>();
        AssetDatabase.CreateAsset(settings, AssetPath);
        AssetDatabase.SaveAssets();

        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
        Debug.Log("[AI] AIClientSettingsを作成しました。Inspectorで接続先を設定してください: " + AssetPath);
    }
}
#endif
