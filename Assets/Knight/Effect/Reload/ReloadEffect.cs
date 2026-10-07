using UnityEngine;
using UnityEngine.UI;

public class ReloadEffect : MonoBehaviour
{
    [SerializeField] private AudioData ad;
    [SerializeField] private Image horizon_Right;
    [SerializeField] private Image horizon_Left;

    private float progress = 0f;
    private void Start()
    {
        horizon_Right.fillAmount = 0;
        horizon_Left.fillAmount = 0;

        AudioManager.instance.PlaySE(ad);

        Destroy (gameObject, 5f);
    }
    private void Update()
    {
        progress += Time.deltaTime;

        //horizon_Right.fillAmount += horizon_Right.fillAmount / 10 + Time.deltaTime;
        //horizon_Left.fillAmount += horizon_Left.fillAmount / 10 + Time.deltaTime;

        horizon_Right.fillAmount = Mathf.SmoothStep(0, 1, progress);
        horizon_Left.fillAmount = Mathf.SmoothStep(0, 1, progress);
    }
}
