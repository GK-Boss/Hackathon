using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardIcon : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void Active()
    {
        image.enabled = true;

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
        }
    }
    public void NonActive()
    {
        image.enabled = false;
    }
}
