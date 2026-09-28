using UnityEngine;
using UnityEngine.UI;

public class QuestionUI : MonoBehaviour
{
    [SerializeField] private Text questionText;
    [SerializeField] private Text subjectText;
    [SerializeField] private Text chainText;
    [SerializeField] private Text[] answerTexts;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private Button dontKnowButton;

    public void Show(string subject, QuestionData question, int chain, UnityEngine.Events.UnityAction<int> answerAction, UnityEngine.Events.UnityAction dontKnowAction)
    {
        subjectText.text = subject;
        questionText.text = question.questionText;
        chainText.text = "CHAIN  " + chain;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerTexts[i].text = (i + 1) + "  " + question.choices[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => answerAction(index));
        }
        dontKnowButton.onClick.RemoveAllListeners();
        dontKnowButton.onClick.AddListener(dontKnowAction);
        gameObject.SetActive(true);
    }
}
