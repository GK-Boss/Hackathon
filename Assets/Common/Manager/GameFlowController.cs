using System;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    static public GameFlowController instance;

    [Header("Game Logic")]
    [SerializeField] private ChainManager chainManager;
    [SerializeField] private ScoreCalculator scoreCalculator;
    [SerializeField] private QuestionManager questionManager;

    [Header("Scene UI")]
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject gradeSelectPanel;
    [SerializeField] private GameObject subjectSelectPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private QuestionUI questionUI;
    [SerializeField] private FeedbackUI feedbackUI;
    [SerializeField] private ResultUI resultUI;

    [Header("BGM")]
    [SerializeField] private AudioData selectBGM;
    [SerializeField] private AudioData quizBGM;

    private string selectedSubject;
    private int selectedGrade;

    private void Start() => ShowModeSelectPanel();

    //public void SelectUnity() => StartGame("Unity");
    //public void SelectGeneral() => StartGame("一般教養");
    //public void SelectProgramming() => StartGame("プログラミング");

    private void Awake()
    {
        instance = this;
    }
    public void StartGame(string subject)
    {
        selectedSubject = subject;
        //chainManager.ResetChain();
        questionManager.StartRun(subject);
        ShowQuizPanel();
    }

    #region ShowPanel
    private void ShowPanel(PanelState state)
    {
        modeSelectPanel.SetActive(state == PanelState.ModeSelect);
        shopPanel.SetActive(state == PanelState.Shop);
        settingsPanel.SetActive(state == PanelState.Settings);
        gradeSelectPanel.SetActive(state == PanelState.GradeSelect);
        subjectSelectPanel.SetActive(state == PanelState.SubjectSelect);
        quizPanel.SetActive(state == PanelState.Quiz);
        feedbackPanel.SetActive(state == PanelState.Feedback);
        resultPanel.SetActive(state == PanelState.Result);

        AudioManager.instance.ChangeBGM(state == PanelState.Quiz ? quizBGM : selectBGM);
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

    public void ShowGradeSelectPanel()
    {
        ShowPanel(PanelState.GradeSelect);
    }
    public void ShowSubjectSelectPanel()
    {
        ShowPanel(PanelState.SubjectSelect);
    }

    public void ShowQuizPanel()
    {
        ShowPanel(PanelState.Quiz);
        QuizManager.instance.StartRun();
    }

    public void ShowFeedbackPanel()
    {
        ShowPanel(PanelState.Feedback);
    }

    public void ShowResultPanel()
    {
        ShowPanel(PanelState.Result);
    }

    #endregion
}

[Serializable]
public enum PanelState
{
    ModeSelect,
    Shop,
    Settings,
    GradeSelect,
    SubjectSelect,
    Quiz,
    Feedback,
    Result
}
