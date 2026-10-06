using NUnit.Framework;
using UnityEngine;

/// <summary>
/// スコア計算の検査。
///
/// Tomo注: 企画書の「わかりませんを選んだ時点のChainを二乗する」が守られているかを見ます。
///         報酬設計の芯なので、ここが壊れるとゲームの意味が変わります。
/// </summary>
public class ScoreCalculatorTests
{
    private GameObject host;
    private ScoreCalculator calculator;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject("ScoreCalculatorHost");
        calculator = host.AddComponent<ScoreCalculator>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(host);
    }

    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(2, 4)]
    [TestCase(3, 9)]
    [TestCase(10, 100)]
    public void Chainの二乗がスコアになる(int chain, int expected)
    {
        Assert.AreEqual(expected, calculator.CalculateScore(chain));
    }

    [Test]
    public void Chainが増えるほどスコアの伸びが大きくなる()
    {
        // 二次関数であることの確認。線形になっていたらここで落ちる。
        int growthAtLow = calculator.CalculateScore(3) - calculator.CalculateScore(2);
        int growthAtHigh = calculator.CalculateScore(10) - calculator.CalculateScore(9);
        Assert.Greater(growthAtHigh, growthAtLow);
    }
}
