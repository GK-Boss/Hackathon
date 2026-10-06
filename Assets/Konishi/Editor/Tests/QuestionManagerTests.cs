using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 出題まわりの検査。
///
/// Tomo注: ここが見ているのは「デモ保険が効いているか」です。
///         AIを一切設定していない状態（aiGenerator = null）で、固定問題だけでゲームが回ることを確認します。
///         デモ当日にAPIが落ちても遊べる、という前提がここで守られています。
/// </summary>
public class QuestionManagerTests
{
    private GameObject host;
    private QuestionManager manager;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject("QuestionManagerHost");
        // aiGenerator は未設定のまま。これがAPIなしの状態を再現している。
        manager = host.AddComponent<QuestionManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(host);
    }

    [TestCase("Unity")]
    [TestCase("一般教養")]
    public void AI未設定でも問題が出る(string subject)
    {
        manager.SelectSubject(subject);
        Assert.IsNotNull(manager.Current);
        Assert.IsNotEmpty(manager.Current.questionText);
    }

    [TestCase("Unity")]
    [TestCase("一般教養")]
    public void 分野を選ぶと最初の問題に戻る(string subject)
    {
        manager.SelectSubject(subject);
        manager.MoveNext(1);
        manager.MoveNext(2);
        Assert.AreNotEqual(0, manager.CurrentIndex);

        manager.SelectSubject(subject);
        Assert.AreEqual(0, manager.CurrentIndex);
    }

    [TestCase("Unity")]
    [TestCase("一般教養")]
    public void 進み続けても問題が尽きない(string subject)
    {
        // 固定問題は数が少ないので循環する。デモ中に出題が止まらないことの確認。
        manager.SelectSubject(subject);
        for (int i = 0; i < 20; i++)
        {
            Assert.IsNotNull(manager.Current, i + "問目で問題が無くなりました。");
            manager.MoveNext(i);
        }
    }

    [TestCase("Unity")]
    [TestCase("一般教養")]
    public void 固定問題がすべて出題できる形になっている(string subject)
    {
        // 固定問題はデモの本線なので、中身の形式をここで検査しておく。
        manager.SelectSubject(subject);
        var seen = new HashSet<string>();

        for (int i = 0; i < 20; i++)
        {
            var question = manager.Current;
            seen.Add(question.questionText);

            Assert.AreEqual(PromptTemplates.ChoiceCount, question.choices.Length,
                "選択肢の数が違います: " + question.questionText);
            Assert.GreaterOrEqual(question.correctIndex, 0, question.questionText);
            Assert.Less(question.correctIndex, question.choices.Length,
                "正解indexが範囲外です: " + question.questionText);
            Assert.IsNotEmpty(question.explanation, "解説が空です: " + question.questionText);

            manager.MoveNext(i);
        }

        Assert.GreaterOrEqual(seen.Count, 2, "固定問題が1問しかありません。");
    }

    [Test]
    public void 分野ごとに別の問題が出る()
    {
        manager.SelectSubject("Unity");
        string unityFirst = manager.Current.questionText;

        manager.SelectSubject("一般教養");
        string generalFirst = manager.Current.questionText;

        Assert.AreNotEqual(unityFirst, generalFirst);
    }
}
