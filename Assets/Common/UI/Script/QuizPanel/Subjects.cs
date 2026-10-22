using System.Collections;
using UnityEngine;

public class Subjects : MonoBehaviour
{
    [SerializeField] private string[] subjectNames;
    [SerializeField] private SubjectButton subjectPrefab;
    [SerializeField] private Transform content;

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

        foreach (var subject in subjectNames)
        {
            var button = Instantiate(subjectPrefab, content);
            button.Setting(subject);
            yield return null;
        }
    }
}
