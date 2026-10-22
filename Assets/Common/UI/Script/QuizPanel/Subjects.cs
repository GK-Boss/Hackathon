using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 分野選択ボタンを並べる。
///
/// Tomo注: 分野の一覧は QuestionManager.AvailableSubjects（問題データ側）を正とします。
///         問題が1問も無い分野はボタンに出しません。
///
///         下の subjectNames は「表示順の指定」です。空にしておけば、
///         問題データにある分野がそのまま全部並びます。
///         並び順や出す分野を決めたいときだけ書いてください。
///
///         以前はこの配列だけで分野を作っていたため、問題データと名前が1文字違うと
///         「選んでも他の分野の問題が出る」状態になっていました（「生物学」と「生物」）。
///         コンパイルも通り、ゲームも動いてしまうので気づけない種類の不具合でした。
/// </summary>
public class Subjects : MonoBehaviour
{
    [Tooltip("表示したい分野と、その順番。空なら問題データにある分野をすべて表示する。" +
             "ここに書いても、問題が無い分野はボタンに出ない。")]
    [SerializeField] private string[] subjectNames;

    [SerializeField] private SubjectButton subjectPrefab;
    [SerializeField] private Transform content;

    [Tooltip("問題データの読み込みを待つ最大フレーム数。")]
    [SerializeField] private int maxWaitFrames = 600;

    private void OnEnable()
    {
        StartCoroutine(GenerateSubject());
    }

    private IEnumerator GenerateSubject()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var subject in ResolveSubjectsOrWait())
        {
            var button = Instantiate(subjectPrefab, content);
            button.Setting(subject);
            yield return null;
        }
    }

    /// <summary>
    /// 表示する分野を決める。問題データの読み込みが終わるまで待つ。
    /// QuestionManager が見つからない場合だけ、手入力の配列をそのまま使う。
    /// </summary>
    private IEnumerable<string> ResolveSubjectsOrWait()
    {
        var manager = QuestionManager.instance;

        if (manager == null)
        {
            Debug.LogWarning("[Subjects] QuestionManagerが見つかりません。Inspectorの配列をそのまま表示します。");
            return subjectNames ?? new string[0];
        }

        // 読み込みはローカル実装なら同期で終わる。サーバー実装にしたときだけ数フレーム待つ。
        int waited = 0;
        while (!manager.SubjectsLoaded && waited < maxWaitFrames) waited++;

        if (!manager.SubjectsLoaded)
        {
            Debug.LogWarning("[Subjects] 問題データの読み込みが終わりませんでした。Inspectorの配列をそのまま表示します。");
            return subjectNames ?? new string[0];
        }

        return OrderSubjects(manager.AvailableSubjects);
    }

    /// <summary>
    /// 出題できる分野を、Inspectorで指定した順に並べ替える。
    /// 指定が空なら、データにある順のまま返す。
    /// </summary>
    private List<string> OrderSubjects(IReadOnlyList<string> available)
    {
        var result = new List<string>();

        if (subjectNames == null || subjectNames.Length == 0)
        {
            result.AddRange(available);
            return result;
        }

        foreach (string name in subjectNames)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;

            if (ContainsSubject(available, name))
            {
                result.Add(name);
            }
            else
            {
                // ここに来るのは、名前の打ち間違いか、その分野の問題をまだ入れていないとき
                Debug.LogWarning("[Subjects] 分野「" + name + "」の問題が1問もないため、ボタンを出しませんでした。"
                                 + "questions_seed.json の subject と綴りが一致しているか確認してください。");
            }
        }

        return result;
    }

    /// IReadOnlyList には Contains が無いので、自分で探す。
    private static bool ContainsSubject(IReadOnlyList<string> list, string name)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == name) return true;
        }
        return false;
    }
}
