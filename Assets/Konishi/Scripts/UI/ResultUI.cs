using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text detailText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Button retryButton;

    public void Show(string title, string detail, int score, UnityEngine.Events.UnityAction retryAction)
    {
        titleText.text = title;
        detailText.text = detail;
        scoreText.text = "FINAL SCORE  " + score;
        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(retryAction);
        gameObject.SetActive(true);
    }
}
