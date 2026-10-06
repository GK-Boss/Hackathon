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

### 仕様書

ゲームのコンセプト、Chain、スコア、AI連携、キャラクター解放などの詳細仕様をまとめた資料です。

## 現在の実装状況

現在は、以下のコアループを実装するプロトタイプ段階です。

```text
分野選択
  ↓
問題表示
  ↓
5択回答
  ├─ 正解 → Chain +1 → 次の問題
  ├─ 不正解 → スコア0 → 結果画面
  └─ わかりません → Chain² → 結果画面
```

ゲームロジックとUI制御は、役割ごとに別スクリプトへ分割しています。

```text
Assets/Konishi/Scripts/
├── AI/          ChatGPT連携（プロンプト・通信・生成管理）
├── Data/        問題データの型
├── GameLogic/   Chain・スコア・出題・画面遷移
└── UI/          各画面の表示
```

## UIの作成

UIは実行時に自動生成せず、Unity Scene上に配置して人間が調整できる構成にします。

Unity Editorで以下のメニューを実行すると、プロトタイプ用のUIをSceneに作成できます。

```text
Tools > Learning Game > Create Scene UI
```

作成後は、Canvas、Panel、Button、TextなどをInspectorから調整できます。

## AI（ChatGPT）連携のセットアップ

問題文・選択肢・解説は ChatGPT（OpenAI API）で生成できます。
APIキーが未設定でも、固定問題でそのまま遊べます。

### 手順

1. Unity Editorで `Tools > Learning Game > Create OpenAI Settings` を実行する
2. 作成された `Assets/Konishi/Resources/OpenAISettings.asset` をInspectorで開く
3. `Api Key` にOpenAIのAPIキーを貼り付ける
4. Hierarchyの `Systems` に空のGameObjectを作り、`AIAPIClient` と `AIQuestionGenerator` を付ける
5. `QuestionManager` の `Ai Generator` に、そのGameObjectを割り当てる

`OpenAISettings.asset` はAPIキーを含むため `.gitignore` で除外しています。
リポジトリをクローンした人は、各自で手順1〜3を行ってください。

### 動作

プレイヤーが問題を解いている間に、裏で次の問題を1問だけ先読み生成します。
生成が間に合わない、通信に失敗した、内容の検証に通らなかった場合は、
固定問題へ自動的にフォールバックするため、ゲームが止まることはありません。

## 次の作業

1. Unity Scene上にUIを作成する
2. Inspectorの参照設定を確認する
3. 固定問題でゲームループを再生確認する
4. UIの見た目を調整する
5. 検証済み問題を10〜15問に増やす
6. AI生成問題の品質を実機で確認する

## 開発ルール

アプリに変更を加えた場合は、変更内容に対応する開発Markdownも更新します。

実装とドキュメントの内容を一致させ、チームの誰でも現在の方針と作業状況を把握できる状態を維持します。
