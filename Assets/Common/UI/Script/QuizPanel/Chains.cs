using System.Collections;
using UnityEngine;

public class Chains : MonoBehaviour
{
    static public Chains instance;

    [SerializeField] private Chain chainPrefab_Normal;
    [SerializeField] private Chain chainPrefab_Rare;

    [HideInInspector] public int chainCount = 0;

    private void Awake()
    {
        instance = this;
    }
    public void GenerateChain_Normal(Vector3 pos)
    {
        Instantiate(chainPrefab_Normal, pos, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
        chainCount++;
    }

    public IEnumerator GenerateChain_Rare(Vector3 pos)
    {
        Instantiate(chainPrefab_Rare, pos, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
        chainCount++;
        yield return new WaitForSeconds(0.3f);

        bool success = true;
        while (success)
        {
            Instantiate(chainPrefab_Rare, pos, Quaternion.Euler(0, 0, Random.Range(0, 360)), transform);
            chainCount++;
            yield return new WaitForSeconds(0.3f);
            success = Random.value < 0.5f;

            if (!success && QuizManager.reloadNum > 0)
            {
                success = true;
                QuizManager.reloadNum--;
            }
        }

        if (QuizManager.isRush)
            QuizManager.reloadNum++;
    }

    public void RemoveChain()
    {
        foreach (Transform t in transform)
        {
            chainCount--;
            Destroy(t.gameObject);
            return;
        }
    }

    public void ResetUI()
    {
        foreach (Transform t in transform)
            Destroy(t.gameObject);
        chainCount = 0;
    }
}
