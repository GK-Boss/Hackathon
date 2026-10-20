using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BulletHole : MonoBehaviour
{
    [SerializeField] private RewardIcon rewardIcon1;
    [SerializeField] private RewardIcon rewardIcon2;
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
            StartCoroutine(Success());

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
        Instantiate(chain, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)), GameObject.Find("Chains").transform);
        image.enabled = false;
        yield return new WaitForSeconds(2);
        QuizManager.instance.ChangeQuestion();
    }

    public IEnumerator Fail()
    {
        yield return new WaitForSeconds(2);
        QuestionPanel.instance.NonActive();
    }
}
