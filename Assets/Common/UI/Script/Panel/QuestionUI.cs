using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionUI : MonoBehaviour
{
    [SerializeField] private Chains chains;
    [SerializeField] private TextMeshProUGUI subjectText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private QuestionPanel questionPanel;
    [SerializeField] private RewardIcon rewardIcon;
    [SerializeField] private BulletCircle bulletCircle;
    [SerializeField] private ChoiceButtons choiceButtons;
    [SerializeField] private LoadButton loadButton;

    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject retryButton;


    private QuestionData questionData;

    public void SetData(QuestionData _questionData)
    {
        questionData = _questionData;
        ResetUI();
        AppryUI();
    }

    public void ResetUI()
    {
        //chains.ResetUI();
        questionPanel.ResetUI();
        rewardIcon.NonActive();
        bulletCircle.NonActive();
        loadButton.ResetUI();

        nextButton.SetActive(false);
        backButton.SetActive(false);
        retryButton.SetActive(false);
    }


    private void AppryUI()
    {
        subjectText.text = questionData.subject;
        questionText.text = questionData.questionText;
        choiceButtons.ActiveButtons(questionData);
    }
}
 