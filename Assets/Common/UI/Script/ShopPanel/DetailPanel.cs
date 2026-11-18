using UnityEngine;
using TMPro;

public class DetailPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private TextMeshProUGUI chaingingText;

    public void ShowDetail(int index)
    {
        switch (index)
        {
            case 0:
                Show("", "");
                break;
            case 1:
                Show("", "");
                break;
            case 2:
                Show("", "");
                break;
            default:
                break;
        }
    }

    private void Show(string _explanation, string _changing)
    {

    }
}
