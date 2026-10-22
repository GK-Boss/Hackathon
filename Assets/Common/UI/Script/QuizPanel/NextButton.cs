using UnityEngine;

public class NextButton : MonoBehaviour
{
    static public NextButton instance;

    private void Awake()
    {
        instance = this;
    }
}
