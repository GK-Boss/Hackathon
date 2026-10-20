using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Vector2 textOffset;

    [SerializeField] private Vector2 firstOffset;
    [SerializeField] private AnimationCurve firstMoveCurve;
    [SerializeField] private float firstMoveSeconds = 0.5f;

    [SerializeField] private Vector2 secondOffset;
    [SerializeField] private AnimationCurve secondMoveCurve;
    [SerializeField] private float secondMoveSeconds = 0.2f;

    [SerializeField] private Vector2 thirdOffset;
    [SerializeField] private AnimationCurve thirdMoveCurve;
    [SerializeField] private float thirdMoveSeconds = 0.2f;

    [SerializeField] private Vector2 fourthOffset;
    [SerializeField] private AnimationCurve fourthMoveCurve;
    [SerializeField] private float fourthMoveSeconds = 0.5f;

    [HideInInspector] public bool trueChoice;

    private Vector2 pos;
    private RectTransform imageTransform;
    private RectTransform textTransform;
    private bool canSelect;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        textTransform = text.GetComponent<RectTransform>();
        imageTransform = image.GetComponent<RectTransform>();
    }

    public void Setting(string _text, bool isTrue)
    {
        text.text = _text;
        trueChoice = isTrue;

        imageTransform.anchoredPosition = pos;
        textTransform.anchoredPosition = pos + textOffset;
    }
    public void Active()
    {
        canSelect = false;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        pos = imageTransform.anchoredPosition;

        imageTransform.anchoredPosition = pos + firstOffset;
        textTransform.anchoredPosition = pos + firstOffset + textOffset;

        image.color = new Color(1f, 1f, 1f, 0f);
        text.color = new Color(1f, 1f, 1f, 0f);

        moveCoroutine = StartCoroutine(FirstMove());
    }

    public void NonActive()
    {
        canSelect = false;
        image.color = new Color(1f, 1f, 1f, 0f);
        text.color = new Color(1f, 1f, 1f, 0f);
    }

    private IEnumerator FirstMove()
    {
        float progress = 0f;

        while (progress < firstMoveSeconds)
        {
            progress += Time.deltaTime;

            float t = Mathf.Clamp01(progress / firstMoveSeconds);
            float curveValue = firstMoveCurve.Evaluate(t);

            imageTransform.anchoredPosition = Vector2.Lerp(pos + firstOffset, pos, curveValue);
            textTransform.anchoredPosition = Vector2.Lerp(pos + firstOffset + textOffset, pos + textOffset, curveValue);

            image.color = new Color(1f, 1f, 1f, t);
            text.color = new Color(1f, 1f, 1f, t);

            yield return null;
        }

        imageTransform.anchoredPosition = pos;
        textTransform.anchoredPosition = pos + textOffset;
        image.color = Color.white;
        text.color = Color.white;

        canSelect = true;
        moveCoroutine = null;
    }

    /// <summary>
    /// 選択したときのアニメーション
    /// </summary>
    /// <returns></returns>
    private IEnumerator SecondMove()
    {
        float progress = 0f;
        while (progress < secondMoveSeconds)
        {
            progress += Time.deltaTime;
            float t = Mathf.Clamp01(progress / secondMoveSeconds);
            float curveValue = secondMoveCurve.Evaluate(t);
            imageTransform.anchoredPosition = Vector2.Lerp(pos, pos + secondOffset, curveValue);
            textTransform.anchoredPosition = Vector2.Lerp(pos + textOffset, pos + secondOffset + textOffset, curveValue);
            yield return null;
        }
        imageTransform.anchoredPosition = pos + secondOffset;
        textTransform.anchoredPosition = pos + secondOffset + textOffset;
        moveCoroutine = null;
    }

    private IEnumerator ThirdMove()
    {
        float progress = 0f;
        Vector2 firstPos_Image = imageTransform.anchoredPosition;
        Vector2 firstPos_Text = textTransform.anchoredPosition - textOffset;
        while (progress < thirdMoveSeconds)
        {
            progress += Time.deltaTime;
            float t = Mathf.Clamp01(progress / thirdMoveSeconds);
            float curveValue = thirdMoveCurve.Evaluate(t);
            imageTransform.anchoredPosition = Vector2.Lerp(firstPos_Image, pos + thirdOffset, curveValue);
            textTransform.anchoredPosition = Vector2.Lerp(firstPos_Text + textOffset, pos + thirdOffset + textOffset, curveValue);
            yield return null;
        }
        imageTransform.anchoredPosition = pos + thirdOffset;
        textTransform.anchoredPosition = pos + thirdOffset + textOffset;
        moveCoroutine = null;
    }

    public IEnumerator FourthMove()
    {
        float progress = 0f;

        Vector2 firstPos_Image = imageTransform.anchoredPosition;
        Vector2 firstPos_Text = textTransform.anchoredPosition - textOffset;
        while (progress < fourthMoveSeconds)
        {
            progress += Time.deltaTime;
            float t = Mathf.Clamp01(progress / fourthMoveSeconds);
            float curveValue = fourthMoveCurve.Evaluate(t);
            imageTransform.anchoredPosition = Vector2.Lerp(firstPos_Image, pos + fourthOffset, curveValue);
            textTransform.anchoredPosition = Vector2.Lerp(firstPos_Text + textOffset, pos + fourthOffset + textOffset, curveValue);
            image.color = new Color(1f, 1f, 1f, 1 - t);
            text.color = new Color(1f, 1f, 1f, 1 - t);
            yield return null;
        }
        imageTransform.anchoredPosition = pos + fourthOffset;
        textTransform.anchoredPosition = pos + fourthOffset + textOffset;
        image.color = new Color(1f, 1f, 1f, 0f);
        text.color = new Color(1f, 1f, 1f, 0f);
        moveCoroutine = null;
    }

    public void ResetPos()
    {

    }

    public void OnClick()
    {
        if (!canSelect) return;
        StartCoroutine(SecondMove());
        ChoiceButtons.instance.SelectButton(this);
    }

    public void OffClick()
    {
        StartCoroutine(ThirdMove());
    }
}