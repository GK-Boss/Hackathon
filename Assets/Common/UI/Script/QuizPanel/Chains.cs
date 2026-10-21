using System.Collections;
using UnityEngine;

public class Chains : MonoBehaviour
{
    [SerializeField] private Chain chainPrefab_Normal;
    [SerializeField] private Chain chainPrefab_Rare;

    public void GenerateChain_Normal(Vector3 pos)
    {
        Instantiate(chainPrefab_Normal, pos, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
    }

    public IEnumerator GenerateChain_Rare(Vector3 pos)
    {
        Instantiate(chainPrefab_Rare, pos, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
        yield return new WaitForSeconds(0.3f);

        bool success = true;
        while (success)
        {
            Instantiate(chainPrefab_Rare, pos, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
            yield return new WaitForSeconds(0.3f);
            success = Random.value < 0.5f;
        }
    }

    public void ResetUI()
    {
        foreach (Transform t in transform)
            Destroy(t.gameObject);
    }
}
