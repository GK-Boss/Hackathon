using UnityEngine;

public class Chains : MonoBehaviour
{
    [SerializeField] private Chain chainPrefab;

    public void GenerateChain_Normal()
    {

    }
    public void ResetUI()
    {
        foreach (Transform t in transform)
            Destroy(t.gameObject);
    }
}
