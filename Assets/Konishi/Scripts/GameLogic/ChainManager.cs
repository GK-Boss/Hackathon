using UnityEngine;

public class ChainManager : MonoBehaviour
{
    public int CurrentChain { get; private set; }

    public void ResetChain() => CurrentChain = 0;
    public int AddCorrectAnswer()
    {
        CurrentChain++;
        return CurrentChain;
    }
}
