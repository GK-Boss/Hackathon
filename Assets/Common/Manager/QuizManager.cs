using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public static QuizManager instance;

    [SerializeField] private QuestionUI questionUI;
    public static bool success = false;
    public static bool giveUp = false;
    public static RewardType reward;

    private QuestionData currentQuestion;

    private void Awake()
    {
        instance = this;
    }
    public void ChangeQuestion()
    {
        currentQuestion = QuestionManager.instance.GetNextQuestion();
        questionUI.SetData(currentQuestion);
        reward = RewardManager.instance.SelectRewardType();
        Debug.Log(reward);
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
