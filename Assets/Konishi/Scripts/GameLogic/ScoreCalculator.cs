using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    // 「わかりません」を選んだ時点のChainを二乗してスコア化します。
    public int CalculateScore(int chain) => chain * chain;
}
