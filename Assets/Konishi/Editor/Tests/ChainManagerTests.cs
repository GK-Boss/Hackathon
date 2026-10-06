using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Chainの増減の検査。
///
/// Tomo注: 「正解で+1」「ミスで0」というゲームループの土台です。
/// </summary>
public class ChainManagerTests
{
    private GameObject host;
    private ChainManager chain;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject("ChainManagerHost");
        chain = host.AddComponent<ChainManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(host);
    }

    [Test]
    public void 初期値は0()
    {
        Assert.AreEqual(0, chain.CurrentChain);
    }

    [Test]
    public void 正解するたびに1ずつ増える()
    {
        Assert.AreEqual(1, chain.AddCorrectAnswer());
        Assert.AreEqual(2, chain.AddCorrectAnswer());
        Assert.AreEqual(3, chain.AddCorrectAnswer());
        Assert.AreEqual(3, chain.CurrentChain);
    }

    [Test]
    public void リセットすると0に戻る()
    {
        chain.AddCorrectAnswer();
        chain.AddCorrectAnswer();
        chain.ResetChain();
        Assert.AreEqual(0, chain.CurrentChain);
    }
}
