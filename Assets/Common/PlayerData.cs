using UnityEngine;

public class PlayerData : MonoBehaviour
{
    static public PlayerData instance;

    public int sumPoint;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        sumPoint = PlayerPrefs.GetInt("Point");
    }
    public void SavePoint()
    {
        PlayerPrefs.SetInt("Point", sumPoint);
    }
}
