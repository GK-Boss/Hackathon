using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("デフォルト")]
    public float rareRatio;
    public float challengeRatio;
    public float reloadRatio;

    [Header("アイテム")]
    public int rareUpValue;
    public int rareUpNum;
    public int challengeUpValue;
    public int challengeUpNum;
    public int reloadUpValue;
    public int reloadUpNum;

    public RewardType SelectRewardType()
    {
        float randomNum = Random.value;
        return RewardType.Chain_Normal;
    }

    public bool Reload()
    {
        return Random.value <= reloadRatio ? true : false;
    }
}
