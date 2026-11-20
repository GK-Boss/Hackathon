
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 分野選択ボタンを並べる。
/// QuestionManagerから分野一覧を取得する。
/// </summary>
public class Subjects : MonoBehaviour
{
    static public Subjects instance;

    [Tooltip("表示したい分野と、その順番。空なら全分野を表示する。")]
    [SerializeField] private string[] subjectNames;

    [SerializeField] private SubjectButton subjectPrefab;
    [SerializeField] private Transform content;

    [Tooltip("問題データの読み込みを待つ最大フレーム数。")]
    [SerializeField] private int maxWaitFrames = 600;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        StartCoroutine(GenerateSubject());
    }

    public void SetSubjectNames(string[] subjectNames)
    {
        this.subjectNames = subjectNames;
    }

    private IEnumerator GenerateSubject()
    {
        // 既存ボタンを削除
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        var manager = QuestionManager.instance;

        // QuestionManagerが見つからない場合
        if (manager == null)
        {
            Debug.LogWarning(
                "[Subjects] QuestionManagerが見つかりません。Inspectorの配列を使用します。");

            foreach (var subject in subjectNames ?? new string[0])
            {
                var button = Instantiate(subjectPrefab, content);
                button.Setting(subject);
                yield return null;
            }

            yield break;
        }

        // 問題データの読み込みを待つ
        int waited = 0;

        while (!manager.SubjectsLoaded && waited < maxWaitFrames)
        {
            // ここが重要！
            // 1フレーム待ってから、もう一度確認する
            yield return null;

            waited++;
        }

        // タイムアウト
        if (!manager.SubjectsLoaded)
        {
            Debug.LogWarning(
                "[Subjects] 問題データの読み込みが終わりませんでした。Inspectorの配列を使用します。");

            foreach (var subject in subjectNames ?? new string[0])
            {
                var button = Instantiate(subjectPrefab, content);
                button.Setting(subject);
                yield return null;
            }

            yield break;
        }

        // 読み込み成功
        var subjects = OrderSubjects(manager.AvailableSubjects);

        Debug.Log(
            "[Subjects] 分野一覧を読み込みました: " +
            subjects.Count + "分野");

        foreach (var subject in subjects)
        {
            var button = Instantiate(subjectPrefab, content);
            button.Setting(subject);

            yield return null;
        }
    }

    /// <summary>
    /// Inspectorで指定された順番に並べ替える。
    /// </summary>
    private List<string> OrderSubjects(
        IReadOnlyList<string> available)
    {
        var result = new List<string>();

        if (subjectNames == null || subjectNames.Length == 0)
        {
            result.AddRange(available);
            return result;
        }

        foreach (string name in subjectNames)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (ContainsSubject(available, name))
            {
                result.Add(name);
            }
            else
            {
                Debug.LogWarning(
                    "[Subjects] 分野「" + name +
                    "」の問題が1問もないため、ボタンを出しませんでした。");
            }
        }

        return result;
    }

    private static bool ContainsSubject(
        IReadOnlyList<string> list,
        string name)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == name)
                return true;
        }

        return false;
    }
}
