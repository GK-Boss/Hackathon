using System;
using UnityEngine;

[Serializable]
public class QuestionData
{
    public string questionText;
    public string[] choices;
    public int correctIndex;
    public string explanation;
    public string relatedTerm;

    public QuestionData(string text, string[] choices, int correctIndex, string explanation, string relatedTerm)
    {
        questionText = text;
        this.choices = choices;
        this.correctIndex = correctIndex;
        this.explanation = explanation;
        this.relatedTerm = relatedTerm;
    }
}
