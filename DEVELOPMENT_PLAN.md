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

### 余裕があれば追加する機能

- キャラクターの段階解放
- 正解時の演出
- 分野レベル
- 複数キャラクターのコレクション
- AIによる問題生成
- AIによる解説生成
- セーブ機能
- ランキング

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

### 必須

- [ ] 分野選択からゲームを開始できる
- [ ] 5択問題に回答できる
- [ ] 正解するとChainが増える
- [ ] 不正解するとスコア0で終了する
- [ ] 「わかりません」でChain²のスコアになる
- [ ] 結果画面が表示される
- [ ] UIをScene上で人間が調整できる
- [ ] 固定問題でAPIなしでもプレイできる

### 理想

- [ ] AI問題生成
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

## 10. 現在地

現在は、コアループのプロトタイプ、責務分割したスクリプト構成、
およびAI問題生成（フォールバック付き）を作成した段階。

次に行う作業は以下とする。

1. UnityメニューからScene UIを作成する
2. Inspectorの参照設定を確認する（`QuestionManager` の `Ai Generator` を含む）
3. Unityでコアループを再生確認する
4. UIの見た目を人間が調整する
5. 固定問題を10〜15問に増やす
6. AI生成問題の品質を確認し、プロンプトを調整する

## 11. 関連ドキュメント

- [開発方針](DEVELOPMENT_POLICY.md)
- [Unityプロジェクト](Assets/)
