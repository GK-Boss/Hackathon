using UnityEngine;
using UnityEngine.UI;

public class FeedbackUI : MonoBehaviour
{
    [SerializeField] private Text feedbackText;
    [SerializeField] private Text detailText;
    [SerializeField] private Button nextButton;

    public void Show(string title, string detail, UnityEngine.Events.UnityAction nextAction)
    {
        feedbackText.text = title;
        detailText.text = detail;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(nextAction);
        gameObject.SetActive(true);
    }
}
