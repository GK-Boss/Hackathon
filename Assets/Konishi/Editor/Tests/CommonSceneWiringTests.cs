using System.Collections.Generic;
using System.IO;
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
    private static readonly HashSet<string> AllowedEmpty = new HashSet<string>();

    [OneTimeSetUp]
    public void OpenScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [Test]
    public void ゲーム進行の参照がすべて埋まっている()
    {
        AssertAllReferencesAssigned<GameFlowController>();
    }

    [Test]
    public void UIの参照がすべて埋まっている()
    {
        AssertAllReferencesAssigned<QuestionUI>();
        AssertAllReferencesAssigned<FeedbackUI>();
        AssertAllReferencesAssigned<ResultUI>();
    }

    [Test]
    public void 選択肢のUIが想定の数だけ並んでいる()
    {
        // Tomo注: 選択肢ボタンの管理は ChoiceButtons に移りました（UIの作り替えに追従）。
        //         ChoiceButtons は選択肢を6個前提でシャッフルするので、
        //         ボタンの数がずれていると実行時に範囲外アクセスで落ちます。
        var choiceButtons = FindInScene<ChoiceButtons>();
        Assert.IsNotEmpty(choiceButtons, "SceneにChoiceButtonsがありません。");

        foreach (var buttons in choiceButtons)
        {
            var property = new SerializedObject(buttons).FindProperty("choiceButtons");
            Assert.IsNotNull(property, "ChoiceButtons.choiceButtons が見つかりません。UIの作りが変わった可能性があります。");
            Assert.AreEqual(QuestionRequest.DefaultChoiceCount, property.arraySize,
                "選択肢ボタンの数が " + QuestionRequest.DefaultChoiceCount + " 個ではありません。実行時に落ちます。");
        }
    }

    [Test]
    public void 分野ボタンの分野に問題が存在する()
    {
        // Tomo注: 分野ボタンの一覧は Subjects.subjectNames（Sceneの手入力）から作られ、
        //         問題は questions_seed.json にあります。両者は別々に編集されるため、
        //         名前が1文字でも違うと「選んでも他の分野の問題が出る」状態になります。
        //         実際に「生物学」と「生物」で食い違ったことがあるので、ここで止めます。
        var subjects = FindInScene<Subjects>();
        Assert.IsNotEmpty(subjects, "SceneにSubjectsがありません。分野選択画面が作れません。");

        // 端末に残った生成問題に左右されないよう、種データだけを読む
        var repository = new LocalJsonQuestionRepository(
            Path.Combine(Path.GetTempPath(), "subject_check_" + System.Guid.NewGuid().ToString("N") + ".json"));
        repository.LoadSeed();

        foreach (var component in subjects)
        {
            var property = new SerializedObject(component).FindProperty("subjectNames");
            Assert.IsNotNull(property, "Subjects.subjectNames が見つかりません。");
            Assert.Greater(property.arraySize, 0, "分野が1つも登録されていません。");

            for (int i = 0; i < property.arraySize; i++)
            {
                string name = property.GetArrayElementAtIndex(i).stringValue;
                int count = repository.GetQuestionsBySubjectAsync(name).GetAwaiter().GetResult().Count;

                Assert.Greater(count, 0,
                    "分野ボタン「" + name + "」に対応する問題が questions_seed.json にありません。"
                    + "このまま選ぶと、他の分野の問題が出ます。"
                    + "ボタン名と問題の subject を一致させてください。");
            }
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
