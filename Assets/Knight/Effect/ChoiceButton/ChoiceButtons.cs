using System.Collections;
using UnityEngine;

public class ChoiceButtons : MonoBehaviour
{
    static public ChoiceButtons instance;

    [SerializeField] private ChoiceButton[] choiceButtons;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ActiveButtons()
    {
        StartCoroutine(ActiveButtonsCoroutine());
    }
    public IEnumerator ActiveButtonsCoroutine()
    {
        foreach (ChoiceButton button in choiceButtons)
        {
            button.NonActive();
        }

        foreach (ChoiceButton button in choiceButtons)
        {
            button.Active();
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void SelectButton(ChoiceButton cb)
    {
        foreach (ChoiceButton button in choiceButtons)
        {
            if (button == cb)
            {
                button.OnClick();
            }
            else
            {
                button.OffClick();
            }
        }
    }
}
