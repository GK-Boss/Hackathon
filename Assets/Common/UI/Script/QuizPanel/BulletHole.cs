using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BulletHole : MonoBehaviour
{
    [SerializeField] private RewardIcon rewardIcon1;
    private RewardIcon rewardIcon2;
    [SerializeField] private GameObject chain;

    [SerializeField] private Image image;
    private RectTransform rt;

    private void Awake()
    {
        rt = rewardIcon1.GetComponent<RectTransform>(); 
    }
    private void Start()
    {
        rewardIcon2 = GameObject.Find("RewardIcon").GetComponent<RewardIcon>();
    }
    public void Init(bool success)
    {
        rewardIcon1.Active();
        rt.anchoredPosition -= GetComponent<RectTransform>().anchoredPosition;

        if (QuizManager.giveUp)
        {
            Debug.Log("G");
            QuizManager.giveUp = false;
            StartCoroutine(GiveUp());
            return;
        }

        //StartCoroutine(Success()); // Debug
        //return;

        if (success)
            StartCoroutine(Success());
        else
            StartCoroutine(Fail());
    }

    public IEnumerator Success()
    {
        yield return new WaitForSeconds(2);
        QuestionPanel.instance.NonActive();
        rewardIcon2.Active();
        image.enabled = false;

        switch (QuizManager.reward)
        {
            case RewardType.Chain_Normal:
                Chains.instance.GenerateChain_Normal(transform.position);
                break;
            case RewardType.Chain_Rare:
                yield return StartCoroutine(Chains.instance.GenerateChain_Rare(transform.position));
                break;
            case RewardType.Chain_Boss:
                Chains.instance.GenerateChain_Normal(transform.position);
                break;
            case RewardType.ReloadChallenge:
                //Chains.instance.GenerateChain_Normal(transform.position);
                break;
            case RewardType.Reload:
                QuizManager.reloadNum++;
                //Chains.instance.GenerateChain_Normal(transform.position);
                break;
            case RewardType.ReloadRush:
                QuizManager.reloadNum++;
                QuizManager.isRush = true;
                break;
        }
        NextButton.instance.gameObject.SetActive(true);
    }

    public IEnumerator Fail()
    {
        yield return null;

        if (QuizManager.reloadNum > 0)
        {
            QuizManager.reloadNum--;
            QuizManager.instance.Reload();
        }
        else
        {
            yield return new WaitForSeconds(2f);
            QuestionPanel.instance.NonActive();
            rewardIcon2.Active();
            image.enabled = false;
            LoadButton.instance.gameObject.SetActive(false);
            BackButton.instance.gameObject.SetActive(true);
            RetryButton.instance.gameObject.SetActive(true);
        }
    }

    private IEnumerator GiveUp()
    {
        rewardIcon1.Active();
        yield return StartCoroutine(rewardIcon1.AddPoints());
        QuestionPanel.instance.NonActive();
        rewardIcon2.Active();
        rewardIcon2.ApplyPoint();
    }
}
