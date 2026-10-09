using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour
{
    [SerializeField] private RectTransform visual;
    [SerializeField] private RectTransform textTransform;
    [SerializeField] private Vector2 textOffset;
    [SerializeField] private Vector2 firstOffset;
    [SerializeField] private AnimationCurve firstMoveCurve;
    [SerializeField] private float firstMoveSeconds = 0.5f;

    private Vector2 pos;
    private Image image;
    private bool canSelect;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        image = visual.GetComponent<Image>();
    }

    public void Active()
    {
        canSelect = false;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        pos = visual.anchoredPosition;

        visual.anchoredPosition = pos + firstOffset;
        textTransform.anchoredPosition = pos + firstOffset + textOffset;

        image.color = new Color(1f, 1f, 1f, 0f);

        moveCoroutine = StartCoroutine(FirstMove());
    }

    public void NonActive()
    {
        canSelect = false;
        image.color = new Color(1f, 1f, 1f, 0f);
    }

    private IEnumerator FirstMove()
    {
        float progress = 0f;

        while (progress < firstMoveSeconds)
        {
            progress += Time.deltaTime;

            float t = Mathf.Clamp01(progress / firstMoveSeconds);
            float curveValue = firstMoveCurve.Evaluate(t);

            visual.anchoredPosition = Vector2.Lerp(pos + firstOffset, pos, curveValue);
            textTransform.anchoredPosition = Vector2.Lerp(pos + firstOffset + textOffset, pos + textOffset, curveValue);

            image.color = new Color(1f, 1f, 1f, t);

            yield return null;
        }

        visual.anchoredPosition = pos;
        textTransform.anchoredPosition = pos + textOffset;
        image.color = Color.white;

        canSelect = true;
        moveCoroutine = null;
    }

    public void OnClick()
    {
        if (!canSelect) return;
        ChoiceButtons.instance.SelectButton(this);
    }

    public void OffClick()
    {

    }
}