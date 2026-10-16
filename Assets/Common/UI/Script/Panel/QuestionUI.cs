using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subjectText;
    [SerializeField] private TextMeshProUGUI questionText;

    private QuestionData questionData;

    public void SetData(QuestionData _questionData)
    {
        questionData = _questionData;
        AppryUI();
    }

    private void AppryUI()
    {
        subjectText.text = questionData.subject;
        questionText.text = questionData.questionText;
        ChoiceButtons.instance.ActiveButtons(questionData);
    }
}
