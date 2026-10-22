using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChoiceButtons : MonoBehaviour
{
    static public ChoiceButtons instance;

    [SerializeField] private ChoiceButton[] choiceButtons;

    [HideInInspector] public ChoiceButton currentChoiceButton;

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

    public void ActiveButtons(QuestionData questionData)
    {
        StartCoroutine(ActiveButtonsCoroutine(questionData));
    }
    public IEnumerator ActiveButtonsCoroutine(QuestionData questionData)
    {
        currentChoiceButton = null;

        foreach (ChoiceButton button in choiceButtons)
        {
            button.NonActive();
        }

        List<int> randomIndexs = new() { 0, 1, 2, 3, 4, 5 };

        for (int i = 0; i < randomIndexs.Count; i++)
        {
            int randomIndex = Random.Range(i, randomIndexs.Count);

            (randomIndexs[randomIndex], randomIndexs[i]) = (randomIndexs[i], randomIndexs[randomIndex]);
        }

        int index = 0;
        foreach (ChoiceButton button in choiceButtons)
        {
            button.Setting(questionData.choices[randomIndexs[index]], questionData.choices[randomIndexs[index]] == questionData.CorrectChoice);
            button.Active();
            index++;
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void SelectButton(ChoiceButton cb)
    {
        if (cb == currentChoiceButton)
        {
            currentChoiceButton = null;

            foreach (ChoiceButton button in choiceButtons)
                button.OffClick();
        }
        else
        {
            currentChoiceButton = cb;

            foreach (ChoiceButton button in choiceButtons)
                if (button != currentChoiceButton)
                    button.OffClick();
        }
    }

    public IEnumerator Load()
    {
        if (!currentChoiceButton) yield break;

        yield return StartCoroutine(currentChoiceButton.FourthMove());
    }
}
