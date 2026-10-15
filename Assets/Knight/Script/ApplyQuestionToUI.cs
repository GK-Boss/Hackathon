using UnityEngine;

public class ApplyQuestionToUI : MonoBehaviour
{
    public void ApplyQuestion(QuestionData questionData)
    {
        // UI‚É–â‘è•¶‚ğ•\¦‚·‚éˆ—‚ğ‚±‚±‚É’Ç‰Á
        Debug.Log($"–â‘è: {questionData.questionText}");
        for (int i = 0; i < questionData.choices.Length; i++)
        {
            Debug.Log($"‘I‘ğˆ {i + 1}: {questionData.choices[i]}");
        }
    }
}
