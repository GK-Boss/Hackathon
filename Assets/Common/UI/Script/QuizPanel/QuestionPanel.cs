using UnityEngine;
using UnityEngine.UI;

public class QuestionPanel : MonoBehaviour
{
    static public QuestionPanel instance;
    [SerializeField] private Transform parent;

    private Image image;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        image = GetComponent<Image>();
    }
    public void Active()
    {
        gameObject.SetActive(true);
        //image.enabled = true;
    }

    public void NonActive()
    {
        gameObject.SetActive(false);
        //image.enabled = false;
    }

    public void ResetUI()
    {
        Active();

        foreach (Transform t in parent)
        {
            Destroy(t.gameObject);
        }
    }
}
