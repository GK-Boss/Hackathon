using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public static QuizManager instance;

    [SerializeField] private QuestionUI questionUI;
    public static bool success = false;

    private QuestionData currentQuestion;

    private void Awake()
    {
        instance = this;
    }
    public void ChangeQuestion()
    {
        currentQuestion = QuestionManager.instance.GetNextQuestion();
        questionUI.SetData(currentQuestion);
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
