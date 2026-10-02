using System.Collections;
using UnityEngine;

public class Simulate : MonoBehaviour
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
            getKey_Up = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            getKey_Down = true;
        }
        else
        {
            getKey_Down = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            getKey_Left = true;
        }
        else
        {
            getKey_Left = false;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            getKey_Right = true;
        }
        else
        {
            getKey_Right = false;
        }
    }

    private IEnumerator SimulateChain()
    {
        while (true)
        {
            qNum++;
            Debug.Log("問題" + qNum);
            Debug.Log("↑で正解、↓で不正解、→でギブアップ、左で時間切れ");

            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);

            if (getKey_Up)
            {
                ResetKey();

                RewardType reward = SelectReward_N();
                if (Random.value < 0.01f)
                {
                    reload_Hide++;
                }

                Debug.Log("正解！　報酬：" + reward);

                if (reward == RewardType.C_Normal)
                {
                    chain++;

                    Debug.Log("チェイン：" + chain);
                }
                else if (reward == RewardType.C_Rare)
                {
                    int totalChain = 0;

                    Debug.Log("いずれかのアローキーを押してください");
                    yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                    ResetKey();
                    chain++;
                    totalChain++;
                    Debug.Log("成功！　Chain：" + chain);

                    Debug.Log("いずれかのアローキーを押してください");
                    yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                    ResetKey();
                    chain++;
                    totalChain++;
                    Debug.Log("成功！　Chain：" + chain);

                    bool success = true;

                    while (success)
                    {
                        Debug.Log("いずれかのアローキーを押してください");
                        yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                        ResetKey();
                        success = Random.value < 0.5f;
                        if (success)
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
                                reload--;
                                success = true;
                                Debug.Log("Reload！　Reload：" + reload);
                            }
                            else if (reload_Hide > 0)
                            {
                                reload_Hide--;
                                success = true;
                                Debug.Log("隠しReload！");
                            }
                            else
                            {
                                Debug.Log("合計" + totalChain + "！　Chain：" + chain);
                            }
                        }
                    }

                    if (reloadRush)
                    {
                        reload++;

                        Debug.Log("ReloadRush！　Reload：" + reload);
                    }
                }
                else if (reward == RewardType.ReloadChallenge)
                {
                    Debug.Log("いずれかのアローキーを押してください");
                    yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                    ResetKey();

                    if (Random.value < 1.0f / Mathf.Pow(2, (challengeFail + 1)))
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
                            Debug.Log("Reloadチャレンジ成功！！！　BossChainを実行します。 Reloadが" + reload + "になりました");
                            Debug.Log("実　行");

                            int totalChain = 0;

                            Debug.Log("いずれかのアローキーを押してください");
                            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                            ResetKey();
                            chain++;
                            totalChain++;
                            Debug.Log("成功！　Chain：" + chain);

                            Debug.Log("いずれかのアローキーを押してください");
                            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                            ResetKey();
                            chain++;
                            totalChain++;
                            Debug.Log("成功！　Chain：" + chain);

                            Debug.Log("いずれかのアローキーを押してください");
                            yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                            ResetKey();
                            chain++;
                            totalChain++;
                            Debug.Log("成功！　Chain：" + chain);

                            bool success = true;
                            while (success)
                            {
                                Debug.Log("いずれかのアローキーを押してください");
                                yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                                ResetKey();
                                success = Random.value < (2.0f / 3.0f);
                                if (success)
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
                                        reload--;
                                        success = true;
                                        Debug.Log("Reload！　Reload：" + reload);
                                    }
                                    else if (reload_Hide > 0)
                                    {
                                        reload_Hide--;
                                        success = true;
                                        Debug.Log("隠しReload！");
                                    }
                                    else
                                    {
                                        Debug.Log("合計" + totalChain + "！　Chain：" + chain);
                                    }
                                }
                            }

                            if (reloadRush)
                            {
                                Debug.Log("ReloadRush！　Reload：" + reload);
                                reload++;
                            }

                        }
                    }
                }
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
                    qNum++;
                    chain = 0;
                    reloadRush = false;
                    challengeFail = 0;
                    Debug.Log("不正解！ゲームオーバー！");
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
                    qNum++;
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
                    yield return new WaitUntil(() => getKey_Up || getKey_Down || getKey_Left || getKey_Right);
                    if (getKey_Up)
                    {
                        int coin = chain * (chain + 1) / 2;
                        Debug.Log("正解！最終報酬は" + coin + "コインです");
                        qNum++;
                        chain = 0;
                        reloadRush = false;
                        challengeFail = 0;

                        break;
                    }
                    else if (getKey_Down)
                    {
                        chain /= 2;
                        Debug.Log("不正解！　Chain数：" + chain);
                    }
                    else if (getKey_Left)
                    {
                        chain /= 2;
                        Debug.Log("時間切れ！　Chain数：" + chain);
                    }
                    else if (getKey_Right)
                    {
                        Debug.Log("ギブアップ不可！");
                    }
                    ResetKey();
                }

            }
        }
    }

    private RewardType SelectReward_N()
    {
        int randomValue = Random.Range(0, 100);

        if (randomValue < 75)
        {
            return RewardType.C_Normal;
        }
        else if (randomValue < 95)
        {
            return RewardType.C_Rare;
        }
        else
        {
            return RewardType.ReloadChallenge;
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
        C_Normal,
        C_Rare,
        C_Boss,
        ReloadChallenge,
        Reload,
        ReloadRush
    }
}
