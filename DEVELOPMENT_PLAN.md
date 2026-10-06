# 学習ゲーム 開発計画

## 1. 開発目標

「わからない」を選ぶことが報酬になる学習ゲームを、短時間で遊べるデモとして完成させる。

最初の完成基準は、以下のゲームループが安定して動くこととする。

```text
分野選択
  ↓
問題表示
  ↓
5択回答
  ├─ 正解 → Chain +1 → 次の問題
  ├─ 不正解 → Chain・スコア0 → 結果
  └─ わかりません → Chain² → 結果
```

## 2. 優先順位

### 最優先：コアゲーム

- 分野選択
- 問題表示
- 5択回答
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
| **6択への移行** | UI・固定問題・プロンプトの同時変更が必要。移行手順は9章に記載済み。企画書(10/05)は6択だが、デモは5択で通す |
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
ChainManager        Chainだけを管理
ScoreCalculator     スコアだけを計算
QuestionManager     どの問題を出すかだけを管理
QuestionUI          問題画面だけを制御
FeedbackUI          フィードバック画面だけを制御
ResultUI            結果画面だけを制御
GameFlowController  画面遷移と全体の接続だけを担当
PromptTemplates     AIへ送るプロンプト文だけを保持
AIAPIClient         OpenAI APIとの送受信だけを担当
AIQuestionGenerator 生成の先読みと検証だけを担当
AIQuestionDto       AIの応答JSONの受け取りと検証だけを担当
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
│   ├── AIClient（AIAPIClient + AIQuestionGenerator）
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

`Assets/Konishi/Editor/Tests/` のEditModeテストで測る。回し方は `DEVELOPMENT_POLICY.md`「測ってから次へ進む」を参照。

- [x] 正解するとChainが増える（`ChainManagerTests`）
- [x] 「わかりません」でChain²のスコアになる（`ScoreCalculatorTests`）
- [x] 固定問題がAPIなしで出題でき、進めても尽きない（`QuestionManagerTests`）
- [x] AIの壊れた応答を弾ける（`AIQuestionDtoTests`）
- [x] SceneのInspector参照に空が無い（`CommonSceneWiringTests`）
- [x] 選択肢のUIが5個並んでいる（`CommonSceneWiringTests`）

### 人が触って測るもの

**自動テストでは埋まらない。** 実際にUnityで再生して確認する。

- [ ] 分野選択からゲームを開始できる
- [ ] 5択問題に回答できる
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

## 9. AI連携（2026-10-06 追加）

### 使用するAI

ChatGPT（OpenAI Chat Completions API）を使用する。

### 構成

```text
Assets/Konishi/Scripts/AI/
├── OpenAISettings.cs       APIキー・モデル・タイムアウトの設定（ScriptableObject）
├── PromptTemplates.cs      プロンプト文の組み立て
├── AIAPIClient.cs          OpenAI APIへの送受信
└── AIQuestionGenerator.cs  次問題の先読み生成と検証

Assets/Konishi/Scripts/Data/
└── AIQuestionDto.cs        応答JSONの受け取りと検証
```

### 動作方針

- プレイヤーが回答している間に、次の問題を1問だけバックグラウンドで生成する
- 生成が間に合わない／通信失敗／検証に通らない場合は、固定問題へフォールバックする
- 生成に失敗しても即時に再試行せず、次の機会に別の問題を生成する
- 検証内容は「問題文が空でない」「選択肢が規定数ある」「選択肢が重複していない」
  「correctIndexが範囲内」「解説が空でない」の5点

### APIキーの扱い

`Assets/Konishi/Resources/OpenAISettings.asset` に保存し、`.gitignore` で除外する。
Unityメニュー `Tools > Learning Game > Create OpenAI Settings` から各自が作成する。

注意：現在はクライアントから直接APIを呼んでいるため、ビルドしたアプリにキーが含まれる。
ハッカソンのデモでは許容するが、ストア公開前には中継サーバーを挟む必要がある。

### 選択肢数について

企画書（10/05版）は6択だが、現行のUI・固定問題・プロンプトはすべて5択で統一している。
6択へ移行する場合は、`PromptTemplates.ChoiceCount`、`QuestionUI`、`SceneUIBuilder`、
`QuestionManager` の固定問題を同時に変更すること。

## 10. 現在地（2026-10-06）

済んだもの。

- コアループのプロトタイプと、責務分割したスクリプト構成
- AI問題生成（ChatGPT／フォールバック付き）とSceneへの配線
- **EditModeテスト34本**。1コマンドで、ロジックとSceneの参照をまとめて測れる状態

**次に行うのは「人が測る」作業。** 自動テストで埋まらない項目が残っている。

1. Unityでコアループを1周再生し、完成判定「人が触って測るもの」を埋める
2. 落ちた項目を `BUGS.md` に記録してから直す
3. UIの見た目を人間が調整する
4. 検証済み固定問題を10〜15問に増やす（**人が目視検証する。デモの本線になる**）
5. `OpenAISettings.asset` にAPIキーを入れ、AI生成問題の品質を確認する
6. `exe/` を現行コードで再ビルドする（ビルド担当は11章）

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
