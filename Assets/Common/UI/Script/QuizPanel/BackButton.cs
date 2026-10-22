using UnityEngine;

public class BackButton : MonoBehaviour
{
    static public BackButton instance;

    private void Awake()
    {
        instance = this;
    }
}
