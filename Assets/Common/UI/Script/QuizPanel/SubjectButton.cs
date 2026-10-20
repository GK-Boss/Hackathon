using UnityEngine;
using TMPro;

public class SubjectButton : MonoBehaviour
{
    public string subjectName;
    [SerializeField] private TextMeshProUGUI nameText;

    public void Setting(string name)
    {
        subjectName = name;
        if (nameText != null)
        {
            nameText.text = name;
        }
    }

    public void OnClick()
    {
        if (GameFlowController.instance != null)
        {
            GameFlowController.instance.StartGame(subjectName);
        }
        else
        {
            Debug.LogError("GameFlowController not found in the scene.");
        }
    }
}
