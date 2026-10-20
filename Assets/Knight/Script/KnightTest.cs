using UnityEngine;

public class KnightTest : MonoBehaviour
{
    [SerializeField] private QuizManager quizManager;
    
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;

    [SerializeField] private BulletCircle bc;
    [SerializeField] private AudioData bgm1;
    [SerializeField] private AudioData bgm2;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Instantiate(prefab, transform.position, transform.rotation, parent);
        //}

        if (Input.GetKeyDown(KeyCode.B))
        {
            quizManager.ChangeQuestion();
            //ChoiceButtons.instance.ActiveButtons();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            AudioManager.instance.ChangeBGM(bgm1);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            AudioManager.instance.ChangeBGM(bgm2);
        }

    }
}
