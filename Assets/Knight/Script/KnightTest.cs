using UnityEngine;

public class KnightTest : MonoBehaviour
{
    [SerializeField] private QuizManager quizManager;
    
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;

    [SerializeField] private BulletCircle bc;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Instantiate(prefab, transform.position, transform.rotation, parent);
        //}

        if (Input.GetKeyDown(KeyCode.B))
        {
            quizManager.ChangeQuestion();
        }
    }
}
