using UnityEngine;

public class GradeButton : MonoBehaviour
{
    [SerializeField] private int grade;
    [SerializeField] private string[] subjectNames;

    private void OnEnable()
    {
        if (grade < PlayerData.instance.playerGrade)
            Destroy(gameObject);
    }
    public void OnClick()
    {
        Subjects.instance.SetSubjectNames(subjectNames);
        GameFlowController.instance.ShowSubjectSelectPanel();
    }
}
