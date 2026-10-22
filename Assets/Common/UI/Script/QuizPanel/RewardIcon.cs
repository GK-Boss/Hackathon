using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RewardIcon : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private TextMeshProUGUI pointText;

    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject retryButton;

    private Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void Active()
    {
        image.enabled = true;
        pointText.text = "";
        if (explanationText)
            explanationText.text = QuizManager.instance.CurrentQuestion.explanation;

        switch (QuizManager.reward)
        {
            case RewardType.Chain_Normal:
                image.sprite = sprites[0];
                break;
            case RewardType.Chain_Rare:
                image.sprite = sprites[1];
                break;
            case RewardType.Chain_Boss:
                image.sprite = sprites[2];
                break;
            case RewardType.ReloadChallenge:
                image.sprite = sprites[3];
                break;
            case RewardType.Reload:
                image.sprite = sprites[4];
                break;
            case RewardType.ReloadRush:
                image.sprite = sprites[5];
                break;
            //case RewardType.Fail:
            //    image.enabled = false;
            //    text.text = "0";
            //    StartCoroutine(AddPoints());
            //    break;
            case RewardType.Fail:
                image.enabled = false;
                pointText.text = "Miss";
                break;
        }
    }
    public void NonActive()
    {
        image.enabled = false;
        pointText.text = "";
        explanationText.text = "";
    }

    public IEnumerator AddPoints()
    {
        image.enabled = false;
        pointText.text = "0";

        yield return new WaitForSeconds(1f);

        int points = 0;
        int index = 1;

        while (Chains.instance.chainCount > 0)
        {
            Chains.instance.RemoveChain();

            points += index;

            pointText.text = points.ToString();

            index++;
            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log(points);

        QuizManager.point = points;
    }

    public void ApplyPoint()
    {
        image.enabled = false;
        pointText.text = QuizManager.point.ToString();
        LoadButton.instance.gameObject.SetActive(false);
        backButton.SetActive(true);
        retryButton.SetActive(true);
    }
}
