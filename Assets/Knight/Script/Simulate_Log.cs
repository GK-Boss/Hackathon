using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class Simulate_Log : MonoBehaviour
{
    private int qNum = 0;
    private int chain = 0;
    private int reload = 0;
    private int reload_Hide = 0;
    private bool reloadRush = false;
    private int challengeFail = 0;

    private bool getKey_Up = false;
    private bool getKey_Down = false;
    private bool getKey_Left = false;
    private bool getKey_Right = false;

    private bool success;
    void Start()
    {
        Debug.Log("開始");

        StartCoroutine(SimulateChain());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            getKey_Up = true;
        }
        else
        {
            //getKey_Up = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            getKey_Down = true;
        }
        else
        {
            //getKey_Down = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            getKey_Left = true;
        }
        else
        {
            //getKey_Left = false;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            getKey_Right = true;
        }
        else
        {
            //getKey_Right = false;
        }
    }

    private IEnumerator SimulateChain()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            qNum++;
            Debug.Log("問題" + qNum);
            yield return new WaitForSeconds(1);
            Debug.Log("↑で正解、↓で不正解、→でギブアップ、左で時間切れ");

            ResetKey();
            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
            yield return StartCoroutine(WaitThreeSeconds());

            if (getKey_Up)
            {
                ResetKey();

                RewardType reward = SelectReward_N();
                if (Random.value < 0.01f)
                {
                    reload_Hide++;
                }

                Debug.Log("正解！　報酬：" + reward);
                yield return new WaitForSeconds(1);

                if (reward == RewardType.Chain_Normal)
                {
                    chain++;

                    Debug.Log("チェイン：" + chain);
                }
                else if (reward == RewardType.Chain_Rare)
                {
                    yield return Chain_Rare();
                }
                else if (reward == RewardType.ReloadChallenge)
                {
                    yield return StartCoroutine(ReloadChallenge());
                    if (!success)
                        continue;

                    Debug.Log("気合を込めて、キーを押してください");
                    yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                    ResetKey();
                    yield return StartCoroutine(WaitThreeSeconds());
                    reward = SelectReward_B();

                    if (reward == RewardType.Reload)
                    {
                        reload++;
                        challengeFail++;
                        Debug.Log("Reloadチャレンジ失敗。連続失敗数が" + challengeFail + "になりました。　Reloadが" + reload + "になりました");
                    }
                    else
                    {
                        challengeFail = 0;
                        reload++;

                        if (!reloadRush)
                        {
                            Debug.Log("Reloadチャレンジ成功！！！　ReloadRssh獲得！　Reloadが" + reload + "になりました");
                            reloadRush = true;
                        }
                        else
                        {
                            yield return StartCoroutine(Chain_Boss());
                        }
                    }
                }
            }
            else if (getKey_Down)
            {
                ResetKey();

                if (reload > 0)
                {
                    Debug.Log("不正解！リロードを行います");
                    reload--;
                }
                else if (reload_Hide > 0)
                {
                    Debug.Log("不正解！隠しリロードを行います");
                    reload_Hide--;
                }
                else
                {
                    qNum = 0;
                    chain = 0;
                    reloadRush = false;
                    challengeFail = 0;
                    Debug.Log("不正解！ゲームオーバー！");
                }
            }
            else if (getKey_Left)
            {
                ResetKey();

                if (reload > 0)
                {
                    Debug.Log("時間切れ！リロードを行います");
                    reload--;
                }
                else if (reload_Hide > 0)
                {
                    Debug.Log("時間切れ！隠しリロードを行います");
                    reload_Hide--;
                }
                else
                {
                    qNum= 0;
                    chain = 0;
                    reloadRush = false;
                    challengeFail = 0;
                    Debug.Log("時間切れ！ゲームオーバー！");
                }
            }
            else if (getKey_Right)
            {
                Debug.Log($"ギブアップ！最終報酬チャレンジに移行します");
                Debug.Log("Chain数：" + chain);

                while (true)
                {
                    Debug.Log("↑で正解、↓で不正解、→でギブアップ、左で時間切れ");
                    ResetKey();
                    yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                    yield return StartCoroutine(WaitThreeSeconds());

                    if (getKey_Up)
                    {
                        ResetKey();

                        int coin = chain * (chain + 1) / 2;
                        Debug.Log("正解！最終報酬は" + coin + "コインです");
                        qNum = 0;
                        chain = 0;
                        reloadRush = false;
                        challengeFail = 0;

                        break;
                    }
                    else if (getKey_Down)
                    {
                        if (reload > 0)
                        {
                            Debug.Log("不正解！リロードを行います");
                            reload--;
                        }
                        else if (reload_Hide > 0)
                        {
                            Debug.Log("不正解！隠しリロードを行います");
                            reload_Hide--;
                        }
                        else
                        {
                            chain /= 2;
                            Debug.Log("不正解！　Chain数：" + chain);
                        }
                    }
                    else if (getKey_Left)
                    {
                        if (reload > 0)
                        {
                            Debug.Log("時間切れ！リロードを行います");
                            reload--;
                        }
                        else if (reload_Hide > 0)
                        {
                            Debug.Log("時間切れ！隠しリロードを行います");
                            reload_Hide--;
                        }
                        else
                        {
                            chain /= 2;
                            Debug.Log("時間切れ！　Chain数：" + chain);
                        }
                    }
                    else if (getKey_Right)
                    {
                        Debug.Log("ギブアップ不可！");
                    }
                }

            }
        }
    }

    private IEnumerator WaitThreeSeconds()
    {
        Debug.Log("...");
        yield return new WaitForSeconds(1);
        Debug.Log("..");
        yield return new WaitForSeconds(1);
        Debug.Log(".");
        yield return new WaitForSeconds(1);
    }
    private RewardType SelectReward_N()
    {
        int randomValue = Random.Range(0, 100);

        if (randomValue < 75)
        {
            return RewardType.Chain_Normal;
        }
        else if (randomValue < 95)
        {
            return RewardType.Chain_Rare;
        }
        else
        {
            return RewardType.ReloadChallenge;
        }
    }

    private RewardType SelectReward_B()
    {
        int randomValue = Random.Range(0, 100);

        if (Random.value < 1.0f / Mathf.Pow(2, (challengeFail + 1)))
        {
            return RewardType.Reload;
        }
        else
        {
            return RewardType.ReloadRush;
        }
    }

    private IEnumerator Chain_Rare()
    {
        int totalChain = 0;

        for (int i = 0; i < 2; i++)
        {
            Debug.Log("いずれかのアローキーを押してください");
            ResetKey();
            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
            Debug.Log(".");
            yield return new WaitForSeconds(1);

            chain++;
            totalChain++;
            Debug.Log("成功！　Chain：" + chain);
        }

        while (true)
        {
            Debug.Log("いずれかのアローキーを押してください");
            ResetKey();
            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
            Debug.Log(".");
            yield return new WaitForSeconds(1);

            if (Random.value < 0.5f)
            {
                chain++;
                totalChain++;
                Debug.Log("成功！　Chain：" + chain);
            }
            else
            {
                Debug.Log("失敗");
                if (reload > 0)
                {
                    yield return new WaitForSeconds(1);
                    reload--;
                    Debug.Log("Reload！　Reload：" + reload);
                }
                else if (reload_Hide > 0)
                {
                    yield return new WaitForSeconds(1);
                    reload_Hide--;
                    Debug.Log("隠しReload！");
                }
                else
                {
                    yield return new WaitForSeconds(1);
                    Debug.Log("合計" + totalChain + "！　Chain：" + chain);
                    break;
                }
            }
        }

        if (reloadRush)
        {
            reload++;
            yield return new WaitForSeconds(1);
            Debug.Log("ReloadRush！　Reload：" + reload);
        }
    }
    private IEnumerator Chain_Boss()
    {
        Debug.Log("Reloadチャレンジ成功！！！　BossChainを実行します。 Reloadが" + reload + "になりました");
        yield return new WaitForSeconds(2);

        Debug.Log("実　行");

        yield return new WaitForSeconds(1);

        int totalChain = 0;

        for (int i = 0; i < 3; i++)
        {
            Debug.Log("いずれかのアローキーを押してください");
            ResetKey();
            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
            Debug.Log(".");
            yield return new WaitForSeconds(1);

            chain++;
            totalChain++;
            Debug.Log("成功！　Chain：" + chain);
            yield return new WaitForSeconds(1);
        }

        while (true)
        {
            Debug.Log("いずれかのアローキーを押してください");
            ResetKey();
            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
            Debug.Log(".");
            yield return new WaitForSeconds(1);

            if (Random.value < (3.0f / 4.0f))
            {
                chain++;
                totalChain++;
                Debug.Log("成功！　Chain：" + chain);
                yield return new WaitForSeconds(1);
            }
            else
            {
                Debug.Log("失敗");
                yield return new WaitForSeconds(1);
                if (reload > 0)
                {
                    reload--;
                    Debug.Log("Reload！　Reload：" + reload);
                }
                else if (reload_Hide > 0)
                {
                    reload_Hide--;
                    Debug.Log("隠しReload！");
                }
                else
                {
                    Debug.Log("合計" + totalChain + "！　Chain：" + chain);
                    yield return new WaitForSeconds(1);
                    break;
                }
            }
        }

        if (reloadRush)
        {
            Debug.Log("ReloadRush！　Reload：" + reload);
            yield return new WaitForSeconds(1);

            reload++;
        }
    }

    private IEnumerator ReloadChallenge()
    {
        Debug.Log("問題" + qNum);
        yield return new WaitForSeconds(1);
        Debug.Log("↑で正解、↓で不正解、→でギブアップ、左で時間切れ");
        ResetKey();
        yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
        success = getKey_Up;
        yield return StartCoroutine(WaitThreeSeconds());
        ResetKey();

        if (success)
        {
            Debug.Log("正解！");
        }
        else
        {
            if (reload > 0)
            {
                Debug.Log("不正解！リロードを行います");
                reload--;
            }
            else if (reload_Hide > 0)
            {
                Debug.Log("不正解！隠しリロードを行います");
                reload_Hide--;
            }
            else
            {
                qNum++;
                chain = 0;
                reloadRush = false;
                challengeFail = 0;
                Debug.Log("不正解！ゲームオーバー！");
            }
        }
    }
    private void ResetKey()
    {
        getKey_Up = false;
        getKey_Down = false;
        getKey_Left = false;
        getKey_Right = false;
    }
    private enum RewardType
    {
        Chain_Normal,
        Chain_Rare,
        Chain_Boss,
        ReloadChallenge,
        Reload,
        ReloadRush
    }
}
