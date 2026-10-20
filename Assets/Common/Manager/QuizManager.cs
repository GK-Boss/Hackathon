using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public static QuizManager instance;

    public static bool success = false;

    //本当はこれで演出関連をまとめたかった

    public void FinishSelect() // 使ってない
    {

    }

    public void FinishReward()
    {

    }
}

public enum RewardType
{
    Chain_Normal,
    Chain_Rare,
    Chain_Boss,
    ReloadChallenge,
    Reload,
    ReloadRush
}
