# 学習ゲーム 開発計画

## 1. 開発目標

「わからない」を選ぶことが報酬になる学習ゲームを、短時間で遊べるデモとして完成させる。

最初の完成基準は、以下のゲームループが安定して動くこととする。

```text
分野選択
  ↓
問題表示
  ↓
6択回答
  ├─ 正解 → Chain +1 → 次の問題
  ├─ 不正解 → Chain・スコア0 → 結果
  └─ わかりません → Chain² → 結果
```

## 2. 優先順位

### 最優先：コアゲーム

- 分野選択
- 問題表示
- 6択回答
- 正解判定
- Chain管理
- 不正解時の終了
- 「わかりません」時のスコア確定
- 結果表示

### 次に優先：人間が調整できる構造

- UIをScene上に配置
- InspectorからUIを調整可能にする
- オブジェクト・機能単位でコードを分割
- 問題データをUIコードから分離
- Unity Editor上で担当箇所を見つけやすいHierarchyにする

### 捨てた範囲（と理由）

**理由を書いておかないと、翌週また同じ迷いをする。** 迷ったらここを開く。

| 捨てたもの | 理由 |
|---|---|
| キャラクターの段階解放 | 画像素材の用意が間に合わない。シルエット版も含めて未着手 |
| 正解時の演出 | コアループが人の手で1周できてから。演出は後から足せる |
| 分野レベル・コレクション | セーブ機能が前提になる。デモ1回分では効果が出ない |
| セーブ機能 | デモは1プレイで完結するため不要 |
| ランキング | サーバーが要る。3週間では割に合わない |
| ~~6択への移行~~ | **2026-10-15に実施済み**。企画書(1005版)が正式仕様のため6択にした（9章） |
| **AI検証工程** | 検証するAIも間違えるため保証にならず、コストと待ち時間が倍になる。デモは人が目視検証した固定問題で通す |
| **PlayModeでの1周自動化** | ボタン操作の再現に時間がかかる。1周の確認は人がやる |

### 済ませたもの

- AIによる問題生成（実装済み。フォールバック付き）
- AIによる解説生成（問題生成と同時に取得）

## 3. 開発フェーズ

### Phase 1：プロジェクト整理

- SceneのHierarchyを整理する
- `GameRoot`、`Environment`、`Gameplay`、`Systems`、`UI`を用意する
- Scriptsフォルダを責務ごとに分ける
- UnityのInput System設定を確認する
- UIはSceneに配置する方針を確定する

### Phase 2：コアループ実装

- `ChainManager`を実装する
- `ScoreCalculator`を実装する
- `QuestionManager`を実装する
- `QuestionUI`を実装する
- `FeedbackUI`を実装する
- `ResultUI`を実装する
- `GameFlowController`で各機能を接続する

### Phase 3：固定問題でのデモ完成

- 1分野分の検証済み問題を10〜15問用意する
- APIなしで最後までプレイできるようにする
- 正解・不正解・わかりませんの3パターンを確認する
- UIの文字サイズ、配置、色をUnity Editorで調整する
- スマートフォン画面比率でレイアウトを確認する

### Phase 4：演出追加

- キャラクター表示を追加する
- Chainに応じてキャラクターを段階表示する
- 正解時の演出を追加する
- 「わからない」を選んだときの肯定的な演出を追加する
- 結果画面を見やすくする

### Phase 5：AI連携

- 問題生成用のインターフェースを作る
- 固定問題とAI生成問題を切り替えられるようにする
- AIレスポンス失敗時は固定問題へ戻す
- AIの解説をFeedback画面に表示する
- APIキーなどの秘密情報をリポジトリに保存しない

### Phase 6：デモ・発表準備

- APIなしでも動くことを確認する
- 5分以内にゲームを開始できることを確認する
- iOS / Androidで表示を確認する
- 発表用のプレイ手順を作る
- READMEを作成する
- デモ中に発生しうるエラーと対応方法を整理する

## 4. コード設計方針

### 1ファイル1責務

1つのスクリプトに複数の役割を持たせない。

```text
ChainManager          Chainだけを管理
ScoreCalculator       スコアだけを計算
QuestionUI            問題画面だけを制御
FeedbackUI            フィードバック画面だけを制御
ResultUI              結果画面だけを制御
GameFlowController    画面遷移と全体の接続だけを担当

QuestionManager       問題システムの窓口。中の部品をまとめるだけ
QuestionData          問題のデータだけを持つ（ただのクラス／JSON）
RelatedTerm           関連語と関連度だけを持つ
QuestionRequest       生成の注文だけを持つ
QuestionSelector      次にどれを出すかだけを決める
KnowledgeTracker      プレイ中の知識の記録だけを担当
IQuestionRepository   保存と取得だけを担当
AIQuestionGenerator   AIに作らせることだけを担当
AIQuestionValidator   検証だけを担当
PromptTemplates       プロンプト文だけを保持
IQuestionAIClient     AIとの送受信だけを担当
```

### UIはSceneで管理する

UIの位置・サイズ・色・フォント・画像は、可能な限りUnity Editorで調整する。

スクリプトはUIを生成せず、Scene上に存在するUIを参照して動かす。

### データと表示を分離する

問題文や選択肢をUIスクリプトに直接記述しない。

将来的に以下のいずれにも差し替えられる構造にする。

- 固定問題
- ScriptableObject
- JSON
- AI API

## 5. 推奨Hierarchy

```text
GameRoot
├── Environment
│   ├── Main Camera
│   └── Background
├── Gameplay
│   └── Character
├── Systems
│   ├── GameFlowController
│   ├── ChainManager
│   ├── ScoreCalculator
│   ├── QuestionManager
│   └── AudioManager
└── UI
    └── LearningGameCanvas
        ├── SubjectSelectPanel
        ├── QuizPanel
        ├── FeedbackPanel
        └── ResultPanel
```

## 6. 担当とレビュー方法

### 実装担当

- ゲームロジック：GK_Knight
- UI・演出：GK_Knight
- AI連携：Tomo
- コード整理・説明コメント：Tomo

### レビュー単位

大きな機能を一度に作らず、以下の単位で確認する。

1. 1スクリプト
2. 1画面
3. 1ゲーム状態
4. 1つの画面遷移

変更後は、Unityで実際に再生してから次の機能に進む。

## 7. 変更時のルール

- アプリに変更を加えたら、必ずこの計画書または`DEVELOPMENT_POLICY.md`も更新する
- 変更内容・変更理由・影響範囲・次の作業を記録する
- 実装とドキュメントを同じ作業単位で更新する
- 既存のコアゲームを壊す変更は避ける
- 新機能追加前に、固定問題でのデモ動作を維持する
- UIの調整はSceneとInspectorを優先する
- 既存スクリプトに責務を追加しすぎない
- 不明な設定や一時的な処理にはコメントを付ける
- API障害が起きてもゲームを続けられるようにする

## 8. 完成判定

**項目ごとに「どうやって測るか」を決めておく。** 測り方が無い項目は、永遠にチェックが入らない。

### 自動テストで測るもの

`Assets/Konishi/Editor/Tests/` のEditModeテスト139本で測る。回し方は `DEVELOPMENT_POLICY.md` を参照。

- [x] 正解するとChainが増える（`ChainManagerTests`）
- [x] 「わかりません」でChain²のスコアになる（`ScoreCalculatorTests`）
- [x] 問題が6択である（`QuestionDataTests`）
- [x] 関連語と関連度(1〜5)を持てる（`QuestionDataTests`）
- [x] AIの壊れた問題を弾ける（`AIQuestionValidatorTests`）
- [x] 選択肢が20文字以内に保たれる（`AIQuestionValidatorTests`）
- [x] 関連語の重み付け選択と1/Nルールが仕様どおり（`QuestionSelectorTests`）
- [x] Reloadで知識が二重カウントされない（`KnowledgeTrackerTests`）
- [x] 検証失敗時の修正が1回だけで、無限リトライしない（`AIQuestionGeneratorTests`）
- [x] 問題をJSONで保存・読み込みできる（`LocalJsonQuestionRepositoryTests`）
- [x] 種データが全問とも6択で検証に通る（`LocalJsonQuestionRepositoryTests`）
- [x] `GetNextQuestion()` だけで問題を取得できる（`QuestionManagerTests`）
- [x] `GenerateBossQuestion()` で今回の知識からボス問題を取得できる（`QuestionManagerTests`）
- [x] APIなしでも出題でき、進めても尽きない（`QuestionManagerTests`）
- [x] SceneのInspector参照に空が無い（`CommonSceneWiringTests`）
- [x] 選択肢のUIが6個並んでいる（`CommonSceneWiringTests`）

### 人が触って測るもの

**自動テストでは埋まらない。** 実際にUnityで再生して確認する。

- [ ] 分野選択からゲームを開始できる
- [ ] 6択問題に回答できる
- [ ] 不正解するとスコア0で終了し、結果画面が出る
- [ ] 「わかりません」から結果画面まで進める
- [ ] UIをScene上で人間が調整できる
- [ ] 検証済み固定問題10〜15問でデモが通る

### 理想

- [ ] AI問題生成（実装済み。実機での品質確認が未）
- [ ] AI解説
- [ ] キャラクター段階解放
- [ ] 複数分野
- [ ] セーブ
- [ ] ランキング

## 9. 問題システム（2026-10-15 全面改訂）

指示書「AI・問題データベース担当 実装指示書」に合わせて作り直した。

### ゲーム側から見える窓口

**ゲーム側が触るのは `QuestionManager` だけ。** AI・DB・関連語選択・検証・保存は中に隠す。

```csharp
QuestionData question = questionManager.GetNextQuestion();
questionManager.RegisterQuestionAnswered(question, isCorrect);
questionManager.RegisterQuestionReloaded(question);
QuestionData boss = await questionManager.GenerateBossQuestion();
```

### 構成

```text
GameFlowController
       ↓
QuestionManager（Facade）
       ↓
 ├── IQuestionRepository    問題の保存・取得（いまはローカルJSON、将来サーバー）
 ├── QuestionSelector       次の問題を決める（関連語の重み付け＋1/Nルール）
 ├── AIQuestionGenerator    AI問題生成
 ├── AIQuestionValidator    生成結果の検証
 └── KnowledgeTracker       プレイ中の知識を記録
```

```text
Assets/Konishi/Scripts/Question/
├── QuestionData.cs                   問題そのもの（6択・関連語つき）
├── RelatedTerm.cs                    関連語と関連度(1〜5)
├── QuestionRequest.cs                生成の注文（通常・ボス両対応）
├── QuestionManager.cs                窓口
├── QuestionSelector.cs               関連語の重み付け選択と1/Nルール
├── KnowledgeTracker.cs               プレイ中の知識
├── IQuestionRepository.cs            保存・取得の窓口
├── LocalJsonQuestionRepository.cs     その実装
├── AIQuestionGenerator.cs            AI生成
└── AIQuestionValidator.cs            検証

Assets/Konishi/Scripts/AI/
├── IQuestionAIClient.cs       AI通信の窓口
├── GameServerAIClient.cs      本番：自前サーバー経由
├── DirectOpenAIClient.cs      開発用：OpenAI直叩き
├── AIClientSettings.cs        接続設定
└── PromptTemplates.cs         プロンプト（通常・ボス・修正）
```

### 通常問題の選び方（1/Nルール）

```text
現在の問題の関連語を、関連度で重み付けして1つ選ぶ
  ↓
その関連語に紐づく既存問題数を N とする
  ↓
N = 0 または 1 → 必ず新規生成
N = 2 → 1/2 で新規生成
N = 3 → 1/3 で新規生成
  ↓
生成が間に合わなければ既存問題を使う（ゲームは止めない）
```

完全ランダムにはしない。生成した問題は、今回使わなくても必ず保存する。

### ボス問題

通常問題から選ばない。そのプレイ中に触れた知識から新しく作る。

- `KnowledgeTracker` が「アドレナリン×3」のように出現回数を記録する
- 正解したかではなく、**その問題に触れたか**で記録する
- Reloadで同じ問題に再挑戦しても二重カウントしない
- 生成時には**関連度(1〜5)を渡さず、出現回数だけを渡す**（企画書の指定）
- 複数の知識を組み合わせた難問をAIに作らせる

### 検証と、失敗したときの扱い

```text
生成 → 検証 → 失敗 → 【1回だけ】修正を依頼 → 再検証 → 失敗 → 破棄
```

同じ生成をその場で繰り返さない。次の機会に別の問題を作る。

検証項目（`AIQuestionValidator`）:
6択である／選択肢が空でない／選択肢が重複していない／correctIndexが範囲内／
問題文が成立している／解説がある／関連語がある／関連度が1〜5／関連語が重複していない／
分野が注文どおり／選択肢が20文字以内／既存問題と重複していない

**内容そのものの正しさはコードでは判定できない。** 人が確認して `verified` を立てる。

### APIキーの扱い（2026-10-15 変更）

```text
Unity  →(HTTPS)→  自前サーバー  →  OpenAI API
```

OpenAIのAPIキーはサーバー側だけに置き、Unityのビルドに含めない。

サーバーができるまでは `AIClientSettings.mode` を `DirectOpenAI_DevelopmentOnly` にすれば
Unityから直接呼べるが、**この設定のままビルドしないこと**。

サーバー側に用意してもらう仕様:

```text
POST  Content-Type: application/json
リクエスト  { "system": "...", "user": "...", "model": "...", "temperature": 0.8 }
レスポンス  { "content": "AIが返した本文" }
```

### 問題データの置き場

| 置き場 | 中身 | gitに入れるか |
|---|---|---|
| `Assets/Konishi/Resources/questions_seed.json` | 人が検証した固定問題。**デモの本線** | 入れる |
| `Application.persistentDataPath/questions.json` | AIが生成した問題。端末ごと | 入れない |

JSONは「Unityとサーバーの間のデータ形式」であって、JSONファイル自体をDBとは考えない。
サーバーができたら `IQuestionRepository` の実装を差し替える。`QuestionManager` 側は無修正でよい。

### 選択肢数について（2026-10-15 変更）

**企画書(1005版)が正式仕様なので6択にした。** 以前の5択実装は途中段階だった。

選択肢数の定義は `QuestionRequest.DefaultChoiceCount` の1か所だけ。
UI・種データ・プロンプト・検証はすべてこの値を見ている。

変更したいときは次の順で行う。

1. `QuestionRequest.DefaultChoiceCount` を変える
2. `Tools > Learning Game > Match Choice Buttons To Spec` を実行する
   （`Create Scene UI` はUIを作り直すので、手で整えた配置が消える。こちらは足りないボタンを複製するだけ）
3. `questions_seed.json` の全問の選択肢を増減する
4. テストを回す。直し漏れは `CommonSceneWiringTests` と `LocalJsonQuestionRepositoryTests` が教えてくれる

## 10. 現在地（2026-10-15）

済んだもの。

- コアループのプロトタイプと、責務分割したスクリプト構成
- **問題システムを指示書どおりに全面改訂**（6択・関連語・1/Nルール・ボス問題・Facade化）
- **EditModeテスト139本**。1コマンドで、ロジックとSceneの参照をまとめて測れる状態

**次に行うのは「人が測る」作業と、中身の用意。**

1. Unityでコアループを1周再生し、完成判定「人が触って測るもの」を埋める
2. 6個目の選択肢ボタンの位置をInspectorで整える（複製しただけなので要調整）
3. 落ちた項目を `BUGS.md` に記録してから直す
4. 検証済み固定問題を10〜15問に増やす（**人が目視検証する。デモの本線**）
5. 自前サーバーを立てるか、当面 `DirectOpenAI_DevelopmentOnly` で進めるかを決める
6. `exe/` を現行コードで再ビルドする（ビルド担当は11章）

### まだ無いもの（意図的に未実装）

| 項目 | 理由 |
|---|---|
| 自前サーバーと `ServerQuestionRepository` | サーバーが未着手。`IQuestionRepository` を差し替えるだけで載る形にはしてある |
| 問題内容の正しさの自動検証 | コードでは判定できない。人が `verified` を立てる運用 |
| Reload・タイマー・報酬抽選・ボス戦の画面 | ゲーム側（Knight担当）の実装待ち。`RegisterQuestionReloaded` と `GenerateBossQuestion` は受け口だけ用意済み |

## 11. ビルド担当（未決定）

`exe/` のWebGLビルドは9/28で止まったまま、20コミット分古い。
`DEVELOPMENT_POLICY.md`「デプロイまでを1つの作業にする」を守るには、ビルドする人を決める必要がある。

現状、Tomoのマシンには **WebGLモジュールが入っていない**（Mac Standaloneのみ）。
iOS / Androidモジュールも未インストール。

| 案 | 内容 |
|---|---|
| A | Unity Hub で Tomo のマシンに WebGL モジュールを追加し、以後 Tomo がビルドする |
| B | ビルドは Knight 側で行う。機能が1つ終わったらビルドまでをセットにする |

**次回ミーティングで決める。**

## 12. 関連ドキュメント

- [開発方針](DEVELOPMENT_POLICY.md)
- [間違えた記録](BUGS.md)
- [Unityプロジェクト](Assets/)
