using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadText;
    [SerializeField] private Image bulletImage;

    [SerializeField] private Vector2 firstOffset;
    [SerializeField] private AnimationCurve firstMoveCurve;
    [SerializeField] private float firstMoveSeconds = 0.5f;

    [SerializeField] private TextMeshProUGUI shotText;

    [SerializeField] private BulletCircle bulletCircle;

    private bool canClick = true;
    private bool lTsF = true;

    private Vector2 defaultPos;

    private void Awake()
    {
        defaultPos = bulletImage.GetComponent<RectTransform>().anchoredPosition;
    }

    public void ResetUI()
    {
        canClick = true;
        lTsF = true;
        StopAllCoroutines();
        bulletImage.GetComponent<RectTransform>().anchoredPosition = defaultPos;
        bulletImage.enabled = false;
        loadText.color = new(loadText.color.r, loadText.color.g, loadText.color.b, 1);
        shotText.color = new(shotText.color.r, shotText.color.g, shotText.color.b, 0);
    }

    public void LoadOrShot()
    {
        if (!canClick) return;

        canClick = false;

        if (lTsF)
            StartCoroutine(LoadCurrentBullet());
        else
            StartCoroutine(ShotBullet());
    }

    public IEnumerator LoadCurrentBullet()
    {
        QuizManager.success = ChoiceButtons.instance.currentChoiceButton.trueChoice;

        bulletCircle.Active();

        yield return StartCoroutine(ChoiceButtons.instance.Load());

        bulletImage.enabled = true;

        float progress = 0;
        RectTransform bulletImageTransForm = bulletImage.GetComponent<RectTransform>();

        while (progress < firstMoveSeconds)
        {
            progress += Time.deltaTime;
            loadText.color = new(loadText.color.r, loadText.color.g, loadText.color.b, 1 - progress / firstMoveSeconds);
            shotText.color = new(shotText.color.r, shotText.color.g, shotText.color.b, progress / firstMoveSeconds);

            float t = Mathf.Clamp01(progress / firstMoveSeconds);
            float curveValue = firstMoveCurve.Evaluate(t);
            bulletImageTransForm.anchoredPosition = Vector2.Lerp(defaultPos - firstOffset, defaultPos, curveValue);

            yield return null;
        }

        canClick = true;
        lTsF = false;
    }

    public IEnumerator ShotBullet()
    {
        bulletCircle.Shot();
        bulletCircle.NonActive();
        bulletImage.fillAmount = 0.777f;

        yield break;
    }
}
