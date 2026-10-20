using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BulletCircle : MonoBehaviour
{
    [SerializeField] private Vector2 firstPos;
    [SerializeField] private float firstScale;

    [SerializeField] private Vector2[] movePoints;

    [SerializeField] private Transform bulletHole_Parent;
    [SerializeField] private BulletHole bulletHole;

    private Image image;
    private RectTransform circleRectTransform;

    private bool move;
    private Vector2 pastPoint;
    private Vector2 nextPoint;
    private float moveProgress;

    private void Awake()
    {
        image = GetComponent<Image>();
        circleRectTransform = GetComponent<RectTransform>();
    }

    public void Active()
    {
        image.enabled = true;
        transform.localScale = new(firstScale, firstScale);
        move = false;
        StartCoroutine(ActiveCoroutin());
    }

    public void NonActive()
    {
        image.enabled = false;
        move = false;
    }

    private IEnumerator ActiveCoroutin()
    {
        float progress = 0;

        while (progress < 0.5f)
        {
            transform.localScale = new(Mathf.Lerp(firstScale, 1, progress / 0.5f), Mathf.Lerp(firstScale, 1, progress / 0.5f), 1);
            yield return null;
            progress += Time.deltaTime;
        }

        nextPoint = movePoints[Random.Range(0, movePoints.Length)];
        move = true;
        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (!move) return;

        if (moveProgress < 0.1f)
        {
            circleRectTransform.anchoredPosition = Vector3.Slerp(pastPoint, nextPoint, moveProgress / 0.3f);
            moveProgress += Time.deltaTime;
        }
        else
        {
            pastPoint = circleRectTransform.anchoredPosition;
            nextPoint = movePoints[Random.Range(0, movePoints.Length)];

            moveProgress = 0;
        }
    }

    public void Shot()
    {
        Instantiate(bulletHole, transform.position, Quaternion.identity, bulletHole_Parent).Init(QuizManager.success);
    }
}
