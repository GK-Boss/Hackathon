using System;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    static public GameFlowController instance { get; private set; }

    [Header("Game Logic")]
    [SerializeField] private ChainManager chainManager;
    [SerializeField] private ScoreCalculator scoreCalculator;
    [SerializeField] private QuestionManager questionManager;

    [Header("Scene UI")]
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject subjectSelectPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private QuestionUI questionUI;
    [SerializeField] private FeedbackUI feedbackUI;
    [SerializeField] private ResultUI resultUI;

    private string selectedSubject;

    /// いま表示している問題。QuestionManagerから受け取ったものを保持する。
    private QuestionData currentQuestion;

    private void Start() => ShowModeSelectPanel();

    public void SelectUnity() => StartGame("Unity");
    public void SelectGeneral() => StartGame("一般教養");
    public void SelectProgramming() => StartGame("プログラミング");

    private void Awake()
    {
        instance = this;
    }
    public void StartGame(string subject)
    {
        selectedSubject = subject;
        chainManager.ResetChain();
        questionManager.StartRun(subject);
        ShowQuizPanel();
    }

    #region ShowPanel
    private void ShowPanel(PanelState state)
    {
        modeSelectPanel.SetActive(state == PanelState.ModeSelect);
        shopPanel.SetActive(state == PanelState.Shop);
        settingsPanel.SetActive(state == PanelState.Settings);
        subjectSelectPanel.SetActive(state == PanelState.SubjectSelect);
        quizPanel.SetActive(state == PanelState.Quiz);
        feedbackPanel.SetActive(state == PanelState.Feedback);
        resultPanel.SetActive(state == PanelState.Result);
    }

    public void ShowModeSelectPanel()
    {
        ShowPanel(PanelState.ModeSelect);
    }
    public void ShowShopPanel()
    {
        ShowPanel(PanelState.Shop);
    }

    public void ShowSettingsPanel()
    {
        ShowPanel(PanelState.Settings);
    }
    public void ShowSubjectSelect()
    {
        ShowPanel(PanelState.SubjectSelect);
    }

    public void ShowQuizPanel()
    {
        ShowPanel(PanelState.Quiz);
        // 問題の取り出しはQuestionManagerに任せる。AIやDBの都合はここに書かない。
        currentQuestion = questionManager.GetNextQuestion();
        if (currentQuestion == null)
        {
            ShowResult("問題がありません", "出題できる問題が見つかりませんでした。", 0);
            return;
        }

        questionUI.Show(selectedSubject, currentQuestion, chainManager.CurrentChain, Answer, AdmitUnknown);
    }

    public void ShowFeedbackPanel()
    {
        ShowPanel(PanelState.Feedback);
    }

    public void ShowResultPanel()
    {
        ShowPanel(PanelState.Result);
    }

    //public void ShowQuestion()
    //{
    //    subjectSelectPanel.SetActive(false);
    //    quizPanel.SetActive(true);
    //    feedbackPanel.SetActive(false);
    //    resultPanel.SetActive(false);

    //    // 問題の取り出しはQuestionManagerに任せる。AIやDBの都合はここに書かない。
    //    currentQuestion = questionManager.GetNextQuestion();
    //    if (currentQuestion == null)
    //    {
    //        ShowResult("問題がありません", "出題できる問題が見つかりませんでした。", 0);
    //        return;
    //    }

    //    questionUI.Show(selectedSubject, currentQuestion, chainManager.CurrentChain, Answer, AdmitUnknown);
    //}

    #endregion
    private void Answer(int choice)
    {
        var question = currentQuestion;
        bool isCorrect = choice == question.correctIndex;

        // 正解・不正解にかかわらず「この問題に触れた」ことを記録する（ボス問題の材料になる）
        questionManager.RegisterQuestionAnswered(question, isCorrect);

        if (!isCorrect)
        {
            ShowResult("不正解…", "Chainはすべて失われました。\n正解: " + question.choices[question.correctIndex], 0);
            return;
        }

        chainManager.AddCorrectAnswer();
        quizPanel.SetActive(false);
        feedbackUI.Show("正解！  Chain + 1", "関連ワード：" + DescribeTerms(question), ShowQuizPanel);
        feedbackPanel.SetActive(true);
    }

    /// <summary>
    /// Reloadで同じ問題に再挑戦するときに呼ぶ。
    /// Tomo注: Reload演出とつなぐときはここを使ってください。知識の二重カウントを防ぎます。
    /// </summary>
    public void RetryCurrentQuestion()
    {
        if (currentQuestion == null) return;

        questionManager.RegisterQuestionReloaded(currentQuestion);
        questionUI.Show(selectedSubject, currentQuestion, chainManager.CurrentChain, Answer, AdmitUnknown);
    }

    /// 関連語をフィードバック画面に出す形にする。
    private static string DescribeTerms(QuestionData question)
    {
        var names = question.TermNames();
        return names.Count == 0 ? "—" : string.Join("、", names);
    }

    private void AdmitUnknown()
    {
        var question = currentQuestion;
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

[Serializable]
public enum PanelState
{
    ModeSelect,
    Shop,
    Settings,
    SubjectSelect,
    Quiz,
    Feedback,
    Result
}
