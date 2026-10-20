using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BulletHole : MonoBehaviour
{
    private GameObject questionPanel;
    [SerializeField] private RewardIcon rewardIcon1;
    [SerializeField] private RewardIcon rewardIcon2;
    [SerializeField] private GameObject chain;
    private RectTransform rt;

    private void Awake()
    {
        rt = rewardIcon1.GetComponent<RectTransform>(); 
    }
    private void Start()
    {
        questionPanel = GameObject.Find("QuestionPanel");
        rewardIcon2 = GameObject.Find("RewardIcon2").GetComponent<RewardIcon>();
        rewardIcon2.NonActive();
    }
    public void Init(bool success)
    {
        rt.anchoredPosition -= GetComponent<RectTransform>().anchoredPosition;
            StartCoroutine(Succsess());

        //if (success)
        //    StartCoroutine(Succsess());
        //else
        //    StartCoroutine(Fail());
    }

    public IEnumerator Succsess()
    {
        yield return new WaitForSeconds(2);
        questionPanel.SetActive(false);
        rewardIcon2.Active();
        Instantiate(chain, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 180)), GameObject.Find("Chains").transform);
    }

    public IEnumerator Fail()
    {
        yield return null;
    }
}
