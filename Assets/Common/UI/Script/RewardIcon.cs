using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardIcon : MonoBehaviour
{
    private Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void Active()
    {
        image.enabled = true;
    }
    public void NonActive()
    {
        image.enabled = false;
    }
}
