using UnityEngine;

public class PlayerData : MonoBehaviour
{
    static public PlayerData instance;

    public int playerGrade = 1;
    public int sumPoint;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        playerGrade = PlayerPrefs.GetInt("Grade");
        sumPoint = PlayerPrefs.GetInt("Point");
    }
    public void ChangeGrade(int newGrade)
    {
        playerGrade = newGrade;
        PlayerPrefs.SetInt("Grade", newGrade);
    }
    public void SavePoint()
    {
        PlayerPrefs.SetInt("Point", sumPoint);
    }
}
