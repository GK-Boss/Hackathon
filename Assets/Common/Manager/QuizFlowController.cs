using System.Collections;
using UnityEngine;

public class QuizFlowController : MonoBehaviour
{
    static public QuizFlowController instance;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator StartRun()
    {
        QuizManager.instance.StartRun();
        yield return null;
    }

    public IEnumerator SelectBullet()
    {
        yield return null;
    }

    public IEnumerator ShotBullet()
    {
        yield return null;
    }

    public IEnumerator ShowResult()
    {
        yield return null;
    }
}
