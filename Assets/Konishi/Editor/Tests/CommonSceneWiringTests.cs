using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CommonSceneのInspector参照が外れていないかの検査。
///
/// Tomo注: Unityで一番多い事故は「Inspectorの参照が空のまま再生して、実行した瞬間に落ちる」です。
///         コンパイルは通ってしまうので、コードを読んでも気づけません。
///         ここが、このプロジェクトで唯一それを捕まえられる場所です。
///
///         5択から6択へ移すときも、まずこのテストが落ちて教えてくれます。
/// </summary>
public class CommonSceneWiringTests
{
    private const string ScenePath = "Assets/Common/Scene/CommonScene.unity";

    /// <summary>
    /// 空のままで正しい参照。ここに書いたものは検査から外す。
    /// 「TypeName.fieldName」の形式で、必ず理由をコメントに書くこと。
    /// </summary>
    private static readonly HashSet<string> AllowedEmpty = new HashSet<string>
    {
        // AIAPIClient.settings は空が正常。Awakeで Resources から読み込むため。
        "AIAPIClient.settings"
    };

    [OneTimeSetUp]
    public void OpenScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [Test]
    public void ゲーム進行の参照がすべて埋まっている()
    {
        AssertAllReferencesAssigned<GameFlowController>();
        AssertAllReferencesAssigned<QuestionManager>();
    }

    [Test]
    public void UIの参照がすべて埋まっている()
    {
        AssertAllReferencesAssigned<QuestionUI>();
        AssertAllReferencesAssigned<FeedbackUI>();
        AssertAllReferencesAssigned<ResultUI>();
    }

    [Test]
    public void AI連携の参照がすべて埋まっている()
    {
        AssertAllReferencesAssigned<AIAPIClient>();
        AssertAllReferencesAssigned<AIQuestionGenerator>();
    }

    [Test]
    public void 選択肢のUIが想定の数だけ並んでいる()
    {
        var questionUI = FindInScene<QuestionUI>();
        Assert.IsNotEmpty(questionUI, "SceneにQuestionUIがありません。");

        foreach (var ui in questionUI)
        {
            var serialized = new SerializedObject(ui);
            Assert.AreEqual(PromptTemplates.ChoiceCount, serialized.FindProperty("answerTexts").arraySize,
                "answerTexts の数が PromptTemplates.ChoiceCount と違います。");
            Assert.AreEqual(PromptTemplates.ChoiceCount, serialized.FindProperty("answerButtons").arraySize,
                "answerButtons の数が PromptTemplates.ChoiceCount と違います。");
        }
    }

    [Test]
    public void 必要なコンポーネントがSceneに存在する()
    {
        Assert.IsNotEmpty(FindInScene<GameFlowController>(), "GameFlowControllerがありません。");
        Assert.IsNotEmpty(FindInScene<ChainManager>(), "ChainManagerがありません。");
        Assert.IsNotEmpty(FindInScene<ScoreCalculator>(), "ScoreCalculatorがありません。");
        Assert.IsNotEmpty(FindInScene<QuestionManager>(), "QuestionManagerがありません。");
    }

    /// 非アクティブなPanelの中身も対象にするため Include を指定している。
    private static T[] FindInScene<T>() where T : Object
    {
        return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    /// <summary>
    /// 指定した型のコンポーネントを走査し、参照型のフィールドに空が無いことを確認する。
    /// 配列の各要素も対象になる。
    /// </summary>
    private static void AssertAllReferencesAssigned<T>() where T : Component
    {
        var components = FindInScene<T>();
        Assert.IsNotEmpty(components, typeof(T).Name + " がSceneにありません。");

        foreach (var component in components)
        {
            var serialized = new SerializedObject(component);
            var property = serialized.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference) continue;

                // m_Script はUnityが自動で入れるスクリプト自身への参照なので対象外
                if (property.name == "m_Script") continue;
                if (AllowedEmpty.Contains(typeof(T).Name + "." + property.name)) continue;

                Assert.IsNotNull(property.objectReferenceValue,
                    GetPath(component) + " の " + typeof(T).Name + "." + property.propertyPath + " が空です。" +
                    "InspectorでD&Dして割り当ててください。");
            }
        }
    }

    /// エラーメッセージ用に、Hierarchy上の場所を「親/子」の形で作る。
    private static string GetPath(Component component)
    {
        string path = component.gameObject.name;
        var parent = component.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
