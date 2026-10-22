# AI連携のセットアップ手順（各自の手元で1回だけ）

このゲームはAIで問題・解説を生成します。その接続設定を書くファイルが
`Assets/Konishi/Resources/AIClientSettings.asset` です。

**このファイルは、リポジトリをクローンしても存在しません。**
`.gitignore` で意図的に除外しています（接続情報をGitHubに置かないため）。
探しても見つからないのが正常です。各自がUnityのメニューから作ります。

**設定しなくてもゲームは動きます。** その場合は `questions_seed.json` の
固定問題だけで出題され、AIによる新規生成が止まるだけです。
まず動かしたいだけなら、この手順は後回しでかまいません。

---

## 手順1. 設定ファイルを作る（Unity、30秒）

Unity Editorでプロジェクトを開き、メニューから実行します。

```text
Tools > Learning Game > Create AI Client Settings
```

`Assets/Konishi/Resources/AIClientSettings.asset` ができて、
Projectウィンドウで選択された状態になります。すでにある場合は作り直さず、
その場所を示すだけなので、何度押しても入力済みの設定は消えません。

Inspectorで開くと、次の項目があります。

| 項目 | 意味 |
|---|---|
| `Mode` | `GameServer`（本番）か `DirectOpenAI_DevelopmentOnly`（開発用）か |
| `Server Url` | 本番。問題生成を中継する自前サーバーのURL |
| `Server Token` | 本番。サーバー側が発行するクライアント用トークン（**OpenAIのキーではない**） |
| `Open AI Api Key` | 開発用。OpenAIのAPIキー |
| `Open AI Endpoint` | 既定のまま（`https://api.openai.com/v1/chat/completions`） |
| `Model` | 既定は `gpt-4o-mini` |
| `Temperature` | 既定 0.8。小さいほど安定、大きいほど出題がばらける |
| `Timeout Seconds` | 既定 20。超えたら生成をあきらめ、既存問題で続行する |

---

## 手順2. どちらのモードで使うか

### 本番：`GameServer`（キーを配らなくていい）

```text
Unity  →(HTTPS)→  自前サーバー  →  OpenAI API
```

OpenAIのAPIキーはサーバー側だけに置きます。
`Mode` を `GameServer` にして、`Server Url` に教わったURLを入れるだけです。
**この場合、各自がOpenAIに登録する必要はありません。手順3は不要です。**

サーバーは現在未着手です（担当: Tomo）。立ったらURLを共有します。

サーバー側に用意してもらう仕様:

```text
POST  Content-Type: application/json
リクエスト  { "system": "...", "user": "...", "model": "...", "temperature": 0.8 }
レスポンス  { "content": "AIが返した本文" }
```

### サーバーができるまで：`DirectOpenAI_DevelopmentOnly`

Unityから直接OpenAIを呼びます。各自のAPIキーが必要です（手順3へ）。
**開発中のみ。この設定のままビルドしないでください。**
ビルドしたアプリからキーが抜き出せてしまいます。

---

## 手順3. OpenAIのAPIキーを登録する（`DirectOpenAI_DevelopmentOnly` のときだけ）

ChatGPTの有料プランとは**別の契約**です。ChatGPT Plusを払っていてもAPIは使えません。
APIは従量課金で、先にクレジットを入れないと動きません。

1. <https://platform.openai.com> にログインする（ChatGPTのアカウントでログインできます）
2. 初回は組織（Organization）の作成を求められる。右上の「Start building」から、
   組織名などを入れて作成する
3. **先に支払いを済ませる。** 左メニューの Billing から支払い方法を追加し、
   クレジットを入金する（最低 $5 USD）。
   このとき、使い放題の請求ではなく**前払い（プリペイド／pay-as-you-go credit）**を選ぶ。
   入れた分しか使えないので、想定外の請求が起きません
4. 左メニューの API keys（<https://platform.openai.com/api-keys>）を開く
5. 「Create new secret key」を押す。名前は `hackathon-<自分の名前>` のように、
   後で誰のキーか分かるものにする
6. **表示されたキー（`sk-` で始まる文字列）をその場でコピーする。**
   ダイアログを閉じると二度と表示されません。
   なくしたら作り直せばよいので、無理に保存先を探さなくて大丈夫です
7. Unityに戻り、`AIClientSettings.asset` の Inspector で
   - `Mode` → `DirectOpenAI_DevelopmentOnly`
   - `Open AI Api Key` → コピーしたキーを貼る

費用の目安：`gpt-4o-mini` で問題1問あたり概ね $0.001 未満です。
1回の攻略で生成するのは最大20問（`QuestionManager` の `Max Generations Per Run`）なので、
$5 入れておけば開発中に使い切ることはまずありません。

---

## やってはいけないこと

- **キーを入れたままビルドしない。** 公開するビルドは必ず `Mode` = `GameServer`
- **キーをGitにコミットしない。** `AIClientSettings.asset` は除外済みなので、
  普通に作業すれば入りません。別の場所にメモして commit しないよう注意
- **キーをSlack・LINE・メール・スクリーンショットで共有しない。** キーは1人1本。
  共有が必要になった時点で、それはサーバーを立てるべき合図です
- 流出に気づいたら、API keys の画面で該当キーを Revoke して作り直す

---

## 動いているかの確認

Unityで再生して、Consoleを見ます。

| Consoleの表示 | 意味 |
|---|---|
| `[AI] OpenAIのAPIキーが未設定です。既存問題だけで動作します。` | キーが空。固定問題だけで動いている状態（ゲームは遊べる） |
| `[AI] サーバーURLが未設定です。既存問題だけで動作します。` | `GameServer` なのに `Server Url` が空 |
| `[AI] OpenAIへの問い合わせに失敗しました: ...` | キーが違う／クレジットが無い／通信エラー。ゲームは既存問題で続行する |
| 何も出ない | 生成が通っている |

どの失敗でも**ゲームは止まりません**。生成が間に合わなければ既存問題にフォールバックします。

## よくある詰まり

| 症状 | 原因 |
|---|---|
| メニューに `Create AI Client Settings` が無い | スクリプトのコンパイルが通っていない。Consoleの赤いエラーを先に直す |
| `.asset` ファイルが見つからない | 正常。`.gitignore` で除外しているので、手順1で各自が作る |
| キーを入れたのに `401` で失敗する | キーのコピーミス、またはキーを Revoke 済み。作り直す |
| `429` や `insufficient_quota` で失敗する | クレジットが入っていない。手順3の3を先にやる |
| 問題が増えず、同じ問題が出る | 固定問題13問のうち、薬理学6問は `verified: false` の下書き。`Verified Only` がONだと出題から外れる |

---

## 関連ドキュメント

- [開発計画](DEVELOPMENT_PLAN.md) — 9章にAIと問題システムの設計
- [開発方針](DEVELOPMENT_POLICY.md)
- [間違えた記録](BUGS.md)
