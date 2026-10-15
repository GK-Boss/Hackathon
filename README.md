# 学習ゲーム開発プロジェクト

「わからない」を認めることが報酬になる、RPG風の学習ゲームです。

正解を続けるとChainが増え、「わかりません」を選ぶと、その時点のChainを二乗してスコアに変換します。

## ドキュメント一覧

### [DEVELOPMENT_POLICY.md](DEVELOPMENT_POLICY.md)

開発時に守るルールをまとめたファイルです。

- コードを責務ごとに分割する方針
- UIをUnity Scene上に配置する方針
- Inspectorから人間が調整できる構成
- 推奨Hierarchy
- UIとスクリプトの役割分担
- アプリ変更時にドキュメントも更新するルール

### [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md)

ゲームを完成させるまでの作業順と優先順位をまとめたファイルです。

- 開発目標
- コアゲームを最優先する方針
- Phase 1〜6の開発手順
- 必須機能と追加候補
- 担当分担
- 完成判定
- 現在地と次に行う作業

### [BUGS.md](BUGS.md)

間違えた記録です。同じ間違いを2回しないために残します。

- 何が起きたか
- **なぜそうなったか**
- どう直したか

テストが落ちたら、直す前にここへ書きます。

### 仕様書

ゲームのコンセプト、Chain、スコア、AI連携、キャラクター解放などの詳細仕様をまとめた資料です。

## 現在の実装状況

現在は、以下のコアループを実装するプロトタイプ段階です。

```text
分野選択
  ↓
問題表示
  ↓
6択回答
  ├─ 正解 → Chain +1 → 次の問題
  ├─ 不正解 → スコア0 → 結果画面
  └─ わかりません → Chain² → 結果画面
```

ゲームロジックとUI制御は、役割ごとに別スクリプトへ分割しています。

```text
Assets/Konishi/Scripts/
├── Question/    問題まわり一式（下の表を参照）
├── AI/          AIへの接続（設定・通信・プロンプト）
├── GameLogic/   Chain・スコア・画面遷移
└── UI/          各画面の表示
```

### 問題まわりのクラス

**ゲーム側が触るのは `QuestionManager` だけです。** AI・DB・関連語の選び方は、その奥に隠れています。

```csharp
QuestionData question = questionManager.GetNextQuestion();
questionManager.RegisterQuestionAnswered(question, isCorrect);
questionManager.RegisterQuestionReloaded(question);
QuestionData boss = await questionManager.GenerateBossQuestion();
```

| クラス | 役割 |
|---|---|
| `QuestionManager` | **窓口**。ゲーム側はこれだけ使う |
| `QuestionData` | 問題そのもの。ただのクラスで、JSONでやりとりする |
| `RelatedTerm` | 関連語と関連度（1〜5） |
| `QuestionRequest` | 「どんな問題を作ってほしいか」の注文 |
| `QuestionSelector` | 次にどれを出すか（関連語の重み付け＋1/Nルール） |
| `KnowledgeTracker` | そのプレイ中に触れた知識を記録（ボス問題の材料） |
| `IQuestionRepository` | 問題の保存・取得の窓口 |
| `LocalJsonQuestionRepository` | その実装。サーバーができたら差し替える |
| `AIQuestionGenerator` | AIに問題を作らせる |
| `AIQuestionValidator` | 作られた問題が出題できる形かを検証 |

直したい場所の見つけ方の例:

- 選択肢が長すぎる → `AIQuestionValidator`
- 同じ問題が続く → `QuestionSelector`
- ボス問題の中身 → `KnowledgeTracker` と `PromptTemplates`
- 問題の保存先 → `LocalJsonQuestionRepository`

### 問題を増やすには

`Assets/Konishi/Resources/questions_seed.json` をテキストエディタで編集します。
**Unityを開く必要はありません。**

```json
{
    "id": "unity-005",
    "subject": "Unity",
    "difficulty": 1,
    "questionText": "問題文",
    "choices": ["選択肢1", "選択肢2", "選択肢3", "選択肢4", "選択肢5", "選択肢6"],
    "correctIndex": 0,
    "explanation": "解説",
    "relatedTerms": [
        { "term": "関連キーワード", "relevance": 5 },
        { "term": "周辺のキーワード", "relevance": 2 }
    ],
    "verified": true
}
```

- `choices` は**必ず6個**、各**20文字以内**。長い文が必要なときは、
  `questionText` に「A. 〜 / B. 〜」と書き、`choices` は `["A","B","C","D","E","F"]` にします
- `relatedTerms` は**最低1つ**。関連度は1〜5で、5がこの問題の中心です。次の問題はここから選ばれます
- `verified` は、人が目視で正しさを確認したら `true` に。**デモで使うのは `true` のものだけです**
  - `false` のまま置いておけます（下書き）。`QuestionManager` の `Verified Only` をONにすると出題から外れます
  - **いま薬理学の6問は `verified: false` です。** AIが書いた下書きで、人の確認がまだです
- 編集したらテストを回してください。形式が壊れていれば `LocalJsonQuestionRepositoryTests` が教えてくれます

AIが生成した問題は `Application.persistentDataPath/questions.json` に別で保存されます（gitには入りません）。

## UIの作成

UIは実行時に自動生成せず、Unity Scene上に配置して人間が調整できる構成にします。

Unity Editorで以下のメニューを実行すると、プロトタイプ用のUIをSceneに作成できます。

```text
Tools > Learning Game > Create Scene UI
```

作成後は、Canvas、Panel、Button、TextなどをInspectorから調整できます。

## テストの回し方

**変更したら、次へ進む前にこれを回します。** Unityを開く必要はありません。

```bash
/Applications/Unity/Hub/Editor/6000.3.15f1/Unity.app/Contents/MacOS/Unity \
  -runTests -batchmode -projectPath . -testPlatform EditMode \
  -testResults /tmp/results.xml -logFile /tmp/test.log
```

結果は `/tmp/results.xml` に出ます。Unity Editor から回す場合は `Window > General > Test Runner`。

テストは `Assets/Konishi/Editor/Tests/` にあります。

| テスト | 見ているもの |
|---|---|
| `ChainManagerTests` | 正解で+1、ミスで0 |
| `ScoreCalculatorTests` | Chainの二乗がスコアになる |
| `QuestionDataTests` | 6択であること、関連語と関連度を持てること |
| `LocalJsonQuestionRepositoryTests` | JSONの保存・読み込み、`questions_seed.json` が読めること |
| `AIQuestionValidatorTests` | 壊れた問題を弾けること（6択・関連語・関連度・選択肢の長さ） |
| `QuestionSelectorTests` | 関連度の重み付けと1/Nルール |
| `KnowledgeTrackerTests` | 知識の記録、Reloadで二重カウントしないこと |
| `AIQuestionGeneratorTests` | 修正は1回だけで、無限リトライしないこと |
| `QuestionRequestTests` | 難易度の変換、注文がプロンプトに反映されること |
| `QuestionManagerTests` | 窓口だけで問題とボス問題を取得できること |
| `CommonSceneWiringTests` | SceneのInspector参照に空が無いこと |

`CommonSceneWiringTests` が要です。
Unityで一番多い「Inspectorの参照が外れていて再生した瞬間に落ちる」は、
コンパイルが通ってしまうためコードを読んでも気づけません。これが唯一の検出手段です。

**落ちたら、直す前に [BUGS.md](BUGS.md) に書いてください。**
直してから書こうとすると、なぜそうなったかが消えます。

## AI連携のセットアップ

問題文・選択肢・解説・関連語はAIで生成できます。
**設定していなくても、`questions_seed.json` の問題でそのまま遊べます。**

### 本番の構成

```text
Unity  →(HTTPS)→  自前サーバー  →  OpenAI API
```

OpenAIのAPIキーは**サーバー側だけ**に置き、Unityのビルドには含めません。

サーバー側に用意してもらう仕様:

```text
POST  Content-Type: application/json
リクエスト  { "system": "...", "user": "...", "model": "...", "temperature": 0.8 }
レスポンス  { "content": "AIが返した本文" }
```

### 手順

1. Unity Editorで `Tools > Learning Game > Setup Question System` を実行する
2. できた `Assets/Konishi/Resources/AIClientSettings.asset` をInspectorで開く
3. `Mode` を `GameServer` にし、`Server Url` を設定する

サーバーができるまでは `Mode` を `DirectOpenAI_DevelopmentOnly` にして
`Open AI Api Key` を入れれば動きますが、**この設定のままビルドしないでください**。
APIキーがアプリから抜き出せてしまいます。

`AIClientSettings.asset` は `.gitignore` で除外しています。

### 動作

プレイヤーが答えている間に、裏で次の問題を1問だけ作ります。
生成が間に合わない・通信に失敗した・検証に通らなかった場合は既存問題を使うので、ゲームは止まりません。

検証に落ちた場合は**1回だけ**修正を依頼し、それでも落ちたら破棄します。同じ生成を繰り返しません。

## 次の作業

自動テストで測れる部分は済んでいます。**次は人が触って測る番です。**

1. Unityで固定問題のままコアループを1周再生する
2. 落ちた項目を [BUGS.md](BUGS.md) に記録してから直す
3. UIの見た目を調整する
4. 検証済み問題を10〜15問に増やす（人が目視検証する。デモの本線）
5. APIキーを入れてAI生成問題の品質を確認する
6. `exe/` を現行コードで再ビルドする（担当は [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md) 11章で決定）

## 開発ルール

アプリに変更を加えた場合は、変更内容に対応する開発Markdownも更新します。

実装とドキュメントの内容を一致させ、チームの誰でも現在の方針と作業状況を把握できる状態を維持します。
