using UnityEngine;
using TMPro;

public class SumPointText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    private void Update()
    {
        text.text = PlayerData.instance.sumPoint + "pt";
    }
}
