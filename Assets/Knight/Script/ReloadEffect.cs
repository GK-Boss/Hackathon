using UnityEngine;
using UnityEngine.UI;

public class ReloadEffect : MonoBehaviour
{
    [SerializeField] private AudioData ad;
    [SerializeField] private Image horizon_Right;
    [SerializeField] private Image horizon_Left;

    private void Start()
    {
        horizon_Right.fillAmount = 0;
        horizon_Left.fillAmount = 0;

        AudioManager.instance.PlaySE(ad);
    }
    private void Update()
    {
        horizon_Right.fillAmount += horizon_Right.fillAmount / 10 + Time.deltaTime;
        horizon_Left.fillAmount += horizon_Left.fillAmount / 10 + Time.deltaTime;
    }
}
