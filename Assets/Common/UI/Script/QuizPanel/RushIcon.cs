using UnityEngine;
using UnityEngine.UI;

public class RushIcon : MonoBehaviour
{
    private Image image;
    private void Start()
    {
        image = GetComponent<Image>();
        image.enabled = false;
    }
    private void Update()
    {
        image.enabled = QuizManager.isRush;
    }
}
