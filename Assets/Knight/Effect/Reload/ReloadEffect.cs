using UnityEngine;
using UnityEngine.UI;

public class ReloadEffect : MonoBehaviour
{
    [SerializeField] private AudioData ad;
    [SerializeField] private Image horizonRight;
    [SerializeField] private Transform horizonRight_TargetPosition;
    [SerializeField] private Image horizonLeft;
    [SerializeField] private Transform horizonLeft_TargetPosition;
    [SerializeField] private Image verticalTop;
    [SerializeField] private Image verticalBottom;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float horizonMoveValue;

    private float progress = 0f;
    private void Start()
    {
        horizonRight.fillAmount = 0;
        horizonLeft.fillAmount = 0;

        AudioManager.instance.PlaySE(ad);

        Destroy (gameObject, 5f);
    }
    private void Update()
    {
        progress += Time.deltaTime;

        if (progress < 0.5f)
        {
            horizonRight.fillAmount = curve.Evaluate(progress * 2);
            horizonLeft.fillAmount = curve.Evaluate(progress * 2);
        }
        else if (progress < 1)
        {
            horizonRight.fillAmount = 1;
            horizonLeft.fillAmount = 1;

            horizonRight.transform.position = Vector3.Lerp(horizonRight.transform.position, horizonRight_TargetPosition.position, curve.Evaluate((progress - 0.5f) * 2));
            horizonLeft.transform.position = Vector3.Lerp(horizonLeft.transform.position, horizonLeft_TargetPosition.position, curve.Evaluate((progress - 0.5f) * 2));
        }
    }
}
