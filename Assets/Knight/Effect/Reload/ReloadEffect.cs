using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReloadEffect : EffectBase
{
    [Header("Audio")]
    [SerializeField] private AudioData ad;

    [Header("Horizontal")]
    [SerializeField] private Image horizonRight;
    [SerializeField] private Transform horizonRight_TargetPosition;
    [SerializeField] private Image horizonLeft;
    [SerializeField] private Transform horizonLeft_TargetPosition;

    [SerializeField] private float fillSecconds = 1.0f;
    [SerializeField] private AnimationCurve horizonFillCurve;
    [SerializeField] private float moveSecconds = 1.0f;
    [SerializeField] private AnimationCurve horizonMoveCurve;

    [Header("Vertical")]
    [SerializeField] private Image verticalRight;
    [SerializeField] private Transform verticalRight_TargetPosition;
    [SerializeField] private Image verticalLeft;
    [SerializeField] private Transform verticalLeft_TargetPosition;

    [SerializeField] private float verticalMoveSecconds = 1.0f;
    [SerializeField] private AnimationCurve verticalMoveCurve;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float textDelaySecconds = 1.0f;
    [SerializeField] private float textSecconds = 1.0f;
    [SerializeField] private AnimationCurve textCurve;

    private float progress_horizontal = 0f;
    private float progress_vertical = 0f;
    private float progress_text = 0f;
    private void Start()
    {

        AudioManager.instance.PlaySE(ad);

        Destroy (gameObject, lifeTime);

        StartCoroutine(Horizontal());
        StartCoroutine(Vertical());
        StartCoroutine(Text());
    }
    private void Update()
    {
        progress_horizontal += Time.deltaTime;
        progress_vertical += Time.deltaTime;
        progress_text += Time.deltaTime;
    }

    private IEnumerator Horizontal()
    {
        horizonRight.fillAmount = 0;
        horizonLeft.fillAmount = 0;

        while (progress_horizontal <= fillSecconds)
        {
            horizonRight.fillAmount = horizonFillCurve.Evaluate(progress_horizontal / fillSecconds);
            horizonLeft.fillAmount = horizonFillCurve.Evaluate(progress_horizontal / fillSecconds);
            yield return null;
        }

        horizonRight.fillAmount = 1;
        horizonLeft.fillAmount = 1;

        progress_horizontal = 0;

        Vector3 rSPos = horizonRight.transform.position;
        Vector3 lSPos = horizonLeft.transform.position;

        while (progress_horizontal <= moveSecconds)
        {
            horizonRight.transform.position = Vector3.Lerp(rSPos, horizonRight_TargetPosition.position, horizonMoveCurve.Evaluate(progress_horizontal / moveSecconds));
            horizonLeft.transform.position = Vector3.Lerp(lSPos, horizonLeft_TargetPosition.position, horizonMoveCurve.Evaluate(progress_horizontal / moveSecconds));
            yield return null;
        }

        horizonRight.transform.position = horizonRight_TargetPosition.position;
        horizonLeft.transform.position = horizonLeft_TargetPosition.position;
    }

    private IEnumerator Vertical()
    {
        Vector3 rSPos = verticalRight.transform.position;
        Vector3 lSPos = verticalLeft.transform.position;
        while (progress_vertical <= verticalMoveSecconds)
        {
            verticalRight.transform.position = Vector3.Lerp(rSPos, verticalRight_TargetPosition.position, verticalMoveCurve.Evaluate(progress_vertical / verticalMoveSecconds));
            verticalLeft.transform.position = Vector3.Lerp(lSPos, verticalLeft_TargetPosition.position, verticalMoveCurve.Evaluate(progress_vertical / verticalMoveSecconds));
            yield return null;
        }

        verticalRight.transform.position = verticalRight_TargetPosition.position;
        verticalLeft.transform.position = verticalLeft_TargetPosition.position;
    }

    private IEnumerator Text()
    {
        text.alpha = 0;

        yield return new WaitForSeconds(textDelaySecconds);
        progress_text = 0;

        while (progress_text <= textSecconds)
        {
            text.alpha = textCurve.Evaluate(progress_text / textSecconds);
            yield return null;
        }
    }
}
