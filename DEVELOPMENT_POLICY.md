# 学習ゲーム 開発方針

## 目的

AIだけで実装を完結させず、人間の開発メンバーがUnity Editor上で内容を確認・調整・拡張できるプロジェクト構成にする。

ゲームの動作確認を早く行えることと、後から人間が編集しやすいことを両立する。

## 基本方針

### 1. コードは責務ごとに分割する

1つのスクリプトに、ゲーム全体の処理を詰め込まない。

原則として、Unityのオブジェクトまたは機能単位でスクリプトを分ける。

例：

```text
Assets/Scripts/
├── GameLogic/
│   ├── GameFlowController.cs       # 画面遷移とゲーム全体の流れ
│   ├── ChainManager.cs              # Chainの増減・リセット
│   ├── ScoreCalculator.cs           # Chainからスコアを計算
│   └── QuestionManager.cs           # 問題の選択・管理
├── UI/
│   ├── SubjectSelectUI.cs           # 分野選択画面
│   ├── QuestionUI.cs                # 問題と選択肢の表示
│   ├── ChainDisplayUI.cs            # Chain表示
│   ├── FeedbackUI.cs                # 正解・不正解の表示
│   └── ResultUI.cs                  # 結果画面
├── Character/
│   └── CharacterDisplay.cs          # キャラクターの表示・段階解放
└── Data/
    └── QuestionData.cs              # 問題データの定義
```

### 2. 1スクリプト1責務を基本にする

各スクリプトは、名前から役割が分かるようにする。

- `ChainManager` はChainだけを管理する
- `ScoreCalculator` はスコア計算だけを担当する
- `QuestionUI` は問題表示と選択肢の入力を担当する
- `ResultUI` は結果表示を担当する

別の機能を呼び出す場合は、直接すべてを実装せず、参照やイベントを使って連携する。

### 3. UIはコードで自動生成しない

UIのボタン、テキスト、パネル、画像などは、原則としてUnity Scene上に配置する。

人間の開発メンバーが以下をUnity Editorから直接調整できるようにする。

- 位置
- サイズ
- 色
- フォント
- 文字サイズ
- 画像
- ボタンの間隔
- 表示・非表示
- アニメーション

スクリプトはUIを生成するのではなく、Scene上に配置されたUIを参照して動かす。

```csharp
[SerializeField] private Button answerButton;
[SerializeField] private TMP_Text questionText;
[SerializeField] private GameObject resultPanel;
```

このような参照をInspectorから設定する。

## 推奨Hierarchy

```text
GameRoot
├── Environment
│   ├── Main Camera
│   └── Background
├── Gameplay
│   ├── Character
│   └── GameObjects
├── Systems
│   ├── GameFlowController
│   ├── ChainManager
│   ├── QuestionManager
│   └── AudioManager
└── UI
    └── Canvas
        ├── SubjectSelectPanel
        │   ├── TitleText
        │   ├── SubjectButton_Unity
        │   ├── SubjectButton_General
        │   └── SubjectButton_Programming
        ├── QuizPanel
        │   ├── SubjectText
        │   ├── ChainText
        │   ├── DifficultyText
        │   ├── QuestionText
        │   ├── AnswerButton_1
        │   ├── AnswerButton_2
        │   ├── AnswerButton_3
        │   ├── AnswerButton_4
        │   ├── AnswerButton_5
        │   └── DontKnowButton
        ├── FeedbackPanel
        │   ├── FeedbackText
        │   └── NextButton
        └── ResultPanel
            ├── ResultTitleText
            ├── ScoreText
            └── RetryButton
```

## UI実装ルール

### Sceneで管理するもの

- Canvas
- Panel
- Button
- TextMeshProUGUI
- Image
- キャラクター画像
- レイアウトグループ
- アニメーション対象

### スクリプトで管理するもの

- 表示する文字の差し替え
- 問題データの反映
- ボタン入力への反応
- パネルの表示・非表示
- Chainやスコアの更新
- 正解・不正解の判定

### 禁止事項

- `new GameObject()` でゲーム中にUIを大量生成しない
- 1つの巨大なスクリプトから全UIを操作しない
- UIの位置やサイズをコード内の数値だけで決めない
- Inspectorで調整できる値をハードコードしない

## データ設計

問題文、選択肢、正解、解説はUIスクリプトに直接書かない。

最初はScriptableObjectまたはJSONで管理し、固定問題からAI生成問題へ差し替えられる構造にする。

```text
QuestionData
├── subject
├── questionText
├── choices[5]
├── correctIndex
├── explanation
├── difficulty
└── relatedTerm
```

## 開発の進め方

1. Sceneに必要なGameObjectとUIを配置する
2. Inspectorで人間が見た目を調整する
3. オブジェクトごとに小さなスクリプトを作る
4. スクリプト間の参照をInspectorで設定する
5. 1機能ずつ動作確認する
6. 問題データを固定データからAI連携へ置き換える

## 現在のプロトタイプからの移行方針

現在の `LearningGamePrototype.cs` は、動作確認用の一体型プロトタイプとして扱う。

今後は以下の順に分割する。

1. `ChainManager`
2. `ScoreCalculator`
3. `QuestionManager`
4. `SubjectSelectUI`
5. `QuestionUI`
6. `FeedbackUI`
7. `ResultUI`
8. `GameFlowController`

移行中も、ゲームのコアループが動く状態を維持する。

## 判断基準

新しいコードを書く前に、次を確認する。

- これは既存スクリプトの責務か、新しい責務か
- Unity Editor上で人間が調整すべき要素ではないか
- Inspectorから設定できるようにすべき値ではないか
- 1ファイルが大きくなりすぎていないか
- AI APIが使えなくてもデモできるか

## 最優先の完成条件

- Scene上のUIを人間が調整できる
- 分野選択から結果表示まで動く
- Chainとスコア計算が正しい
- 固定問題で安定してデモできる
- AI APIは後から追加できる
