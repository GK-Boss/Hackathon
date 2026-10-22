using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Sceneに並ぶ分野ボタンと、問題データの分野名が一致しているかの検査。
///
/// Tomo注: 分野ボタンの一覧は Subjects.subjectNames（Sceneに手入力）から作られ、
///         問題は questions_seed.json の subject で引かれます。
///         この2つは別々に管理されているので、片方だけ直すと簡単にずれます。
///
///         ずれても例外は出ません。QuestionManager が「その分野の問題がありません」と
///         判断して全分野から出題するため、一見動いているように見えます。
///         実機で遊んで初めて「生物を選んだのにUnityの問題が出る」と気づくことになります。
///
///         実例（2026-10-22）: シーンのボタンは「生物」、データは「生物学」でずれていた。
///         「英語」のボタンはあるのに問題が1問も無かった。
/// </summary>
public class SubjectCoverageTests
{
    private const string ScenePath = "Assets/Common/Scene/CommonScene.unity";

    [OneTimeSetUp]
    public void OpenScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    /// Sceneの Subjects コンポーネントに入力された分野名を読む。
    private static List<string> SubjectNamesInScene()
    {
        var names = new List<string>();

        foreach (var subjects in Object.FindObjectsByType<Subjects>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var property = new SerializedObject(subjects).FindProperty("subjectNames");
            if (property == null) continue;

            for (int i = 0; i < property.arraySize; i++)
            {
                string value = property.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrWhiteSpace(value)) names.Add(value.Trim());
            }
        }
        return names;
    }

    private static LocalJsonQuestionRepository LoadSeed()
    {
        var repository = new LocalJsonQuestionRepository();
        repository.LoadSeed();
        return repository;
    }

    [Test]
    public void Sceneに分野ボタンが並んでいる()
    {
        Assert.IsNotEmpty(SubjectNamesInScene(),
            "Subjects.subjectNames が空です。分野選択画面にボタンが1つも出ません。");
    }

    [Test]
    public void Sceneに並ぶ分野すべてに問題がある()
    {
        var repository = LoadSeed();

        foreach (string subject in SubjectNamesInScene())
        {
            var questions = repository.GetQuestionsBySubjectAsync(subject).GetAwaiter().GetResult();

            Assert.IsNotEmpty(questions,
                "分野「" + subject + "」のボタンはありますが、問題が1問もありません。"
                + "questions_seed.json の subject と綴りが一致しているか確認してください。"
                + "ずれていると、全分野から混ざった問題が出ます。");
        }
    }

    [Test]
    public void 問題だけあってボタンが無い分野を見つける()
    {
        // こちらは落とさない。意図して隠している分野もあるため、気づけるようにログだけ残す。
        var repository = LoadSeed();
        var inScene = SubjectNamesInScene();

        foreach (string subject in repository.GetSubjectsAsync().GetAwaiter().GetResult())
        {
            if (!inScene.Contains(subject))
            {
                Debug.Log("[Question] 分野「" + subject + "」には問題がありますが、Sceneにボタンがありません。"
                          + "出題されるのはフォールバック時だけです。");
            }
        }
        Assert.Pass();
    }
}
