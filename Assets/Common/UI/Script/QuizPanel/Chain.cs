using UnityEngine;
using UnityEngine.UI;

public class Chain : MonoBehaviour
{
    private Image chainImage;

    private Color tragetColor;
    private float progress;

    private void Awake()
    {
        chainImage = GetComponent<Image>();
    }
    public void SetChainType(RewardType rewardType)
    {
        switch (rewardType)
        {
            case RewardType.Chain_Normal:
                tragetColor = Color.yellow;
                break;
            case RewardType.Chain_Rare:
                tragetColor = Color.red;
                break;
            case RewardType.Chain_Boss:
                tragetColor = new Color(1, 0, 1);
                break;
            default:
                tragetColor = Color.white;
                break;
        }
    }

    private void Update()
    {
        progress += Time.deltaTime;
        if (progress < 1)
            chainImage.color = Color.Lerp(Color.white, tragetColor, progress);
        else
            chainImage.color = tragetColor;
    }
}
