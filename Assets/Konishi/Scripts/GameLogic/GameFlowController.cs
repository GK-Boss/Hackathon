using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    [Header("Game Logic")]
    [SerializeField] private ChainManager chainManager;
    [SerializeField] private ScoreCalculator scoreCalculator;
    [SerializeField] private QuestionManager questionManager;

    [Header("Scene UI")]
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject subjectSelectPanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private QuestionUI questionUI;
    [SerializeField] private FeedbackUI feedbackUI;
    [SerializeField] private ResultUI resultUI;

    private string selectedSubject;

    private void Start() => ShowModeSelectPanel();

    public void SelectUnity() => StartGame("Unity");
    public void SelectGeneral() => StartGame("一般教養");
    public void SelectProgramming() => StartGame("プログラミング");

    private void StartGame(string subject)
    {
        selectedSubject = subject;
        chainManager.ResetChain();
        questionManager.SelectSubject(subject);
        ShowQuestion();
    }

    #region ShowPanel
    public void ShowModeSelectPanel()
    {
        modeSelectPanel.SetActive(true);
        subjectSelectPanel.SetActive(false);
        quizPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        resultPanel.SetActive(false);
    }
    public void ShowSubjectSelect()
    {
        subjectSelectPanel.SetActive(true);
        quizPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    public void ShowQuestion()
    {
        subjectSelectPanel.SetActive(false);
        quizPanel.SetActive(true);
        feedbackPanel.SetActive(false);
        resultPanel.SetActive(false);
        questionUI.Show(selectedSubject, questionManager.Current, chainManager.CurrentChain, Answer, AdmitUnknown);
    }

    #endregion
    private void Answer(int choice)
    {
        var question = questionManager.Current;
        if (choice != question.correctIndex)
        {
            ShowResult("不正解…", "Chainはすべて失われました。\n正解: " + question.choices[question.correctIndex], 0);
            return;
        }

        chainManager.AddCorrectAnswer();
        // Chainを渡すのは、AI生成の難易度指定に使うため
        questionManager.MoveNext(chainManager.CurrentChain);
        quizPanel.SetActive(false);
        feedbackUI.Show("正解！  Chain + 1", "関連ワード：" + question.relatedTerm, ShowQuestion);
        feedbackPanel.SetActive(true);
    }

    private void AdmitUnknown()
    {
        var question = questionManager.Current;
        int score = scoreCalculator.CalculateScore(chainManager.CurrentChain);
        ShowResult("わからないを認めた！", "Chain " + chainManager.CurrentChain + " → スコア " + score + "\n\n" + question.explanation, score);
    }

    private void ShowResult(string title, string detail, int score)
    {
        quizPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        resultUI.Show(title, detail, score, ShowSubjectSelect);
        resultPanel.SetActive(true);
    }
}
