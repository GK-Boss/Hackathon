using UnityEngine;

public class KnightTest : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(prefab, transform.position, transform.rotation, parent);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ChoiceButtons.instance.ActiveButtons();
        }
    }
}
