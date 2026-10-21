using UnityEngine;

public class RewardManager : MonoBehaviour
{
    static public RewardManager instance;

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

    private void Awake()
    {
        instance = this;
    }
    public RewardType SelectRewardType()
    {
        float randomNum = Random.value;
        if (challengeRatio > randomNum)
            return RewardType.ReloadChallenge;
        else if (challengeRatio + rareRatio > randomNum)
            return RewardType.Chain_Rare;
        else
            return RewardType.Chain_Normal;
    }

    public RewardType SelectReward_Challenge()
    {
        return Random.value < 0.5f ? RewardType.Reload : RewardType.ReloadRush;
    }

    public bool Reload()
    {
        return Random.value <= reloadRatio;
    }
}
