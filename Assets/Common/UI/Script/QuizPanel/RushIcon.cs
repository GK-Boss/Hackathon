using UnityEngine;
using UnityEngine.UI;

public class RushIcon : MonoBehaviour
{
    private Image image;
    private void Start()
    {
        image = GetComponent<Image>();
    }
    private void Update()
    {
        image.enabled = QuizManager.isRush;
    }
}
