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
