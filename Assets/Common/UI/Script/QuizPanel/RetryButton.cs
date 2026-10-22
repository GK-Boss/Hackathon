using UnityEngine;

public class RetryButton : MonoBehaviour
{
    static public RetryButton instance;

    private void Awake()
    {
        instance = this;
    }
}
