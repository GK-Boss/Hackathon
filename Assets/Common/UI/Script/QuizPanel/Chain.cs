using UnityEngine;
using UnityEngine.UI;

public class Chain : MonoBehaviour
{
    [SerializeField] private Image chainFillter;

    private Color tragetColor;
    private float progress;

    public void SetChainType(RewardType rewardType)
    {
        tragetColor = new Color(1, 1, 1, 0);
        //switch (rewardType)
        //{
        //    case RewardType.Chain_Normal:
        //        tragetColor = new Color(1, 0.92f, 0.016f, 0);
        //        break;
        //    case RewardType.Chain_Rare:
        //        tragetColor = new Color(1, 0, 0, 0);
        //        break;
        //    case RewardType.Chain_Boss:
        //        tragetColor = new Color(1, 0, 1, 0);
        //        break;
        //    default:
        //        tragetColor = Color.white;
        //        break;
        //}
    }

    private void Update()
    {
        progress += Time.deltaTime;
        if (progress < 1)
            chainFillter.color = Color.Lerp(Color.white, tragetColor, progress);
        else
            chainFillter.color = tragetColor;
    }
}
