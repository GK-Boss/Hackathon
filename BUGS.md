# 間違えた記録

**同じ間違いを2回しないための記録。**

正解だけ書いても見返したときに何も思い出せないので、**なぜそうなったか**を必ず書く。
推測ではなく、実際に確認した原因を書くこと。

開くとき：似たことが起きたとき。デモ前。

| 日付 | 何が起きたか | なぜそうなったか | どう直したか | 状態 |
|---|---|---|---|---|
| 2026-10-06 | `AudioManager.cs` と `Simulate_Log.cs` の `Debug.Log` が文字化けしている（`Debug.Log("SE�ߏ�")`） | ファイルがShift-JISで保存され、UTF-8として読まれている。Visual Studioの既定エンコードがUTF-8でない環境で編集したため | 未対応 | 未解決 |
| 2026-10-06 | 選択肢の数が、仕様書=5択・企画書(10/05)=6択・実装=5択でバラバラだった | 企画書を更新したときに、実装とドキュメントを同じ作業単位で直さなかった。`DEVELOPMENT_PLAN.md` 7章のルールが守られていなかった | 5択に統一すると決定。6択への移行手順を `DEVELOPMENT_PLAN.md` 9章に記載 | 解決 |
| 2026-10-06 | 完成判定14項目が全部未チェックのまま3週間進んでいた | 測る仕組みが無く、測るたびに手作業が必要だったため後回しになった。「あとで」は来なかった | EditModeテスト34本を追加し、1コマンドで測れるようにした | 解決 |
| 2026-10-06 | 分野を選んだあと、2問目で1問目と同じ問題が出た | `QuestionManager.SelectSubject` が1問目を「出題済み」に記録していなかった。`MoveNext` 側だけ記録していたため、1問目だけが漏れた | `SelectSubject` でも `MarkUsed` を呼ぶようにした。`QuestionManagerTests.一周するまで同じ問題は出ない` が検出 | 解決 |
| 2026-10-06 | Questionまわりのクラス名を変えたら、Sceneの参照が全部外れた | Unityはスクリプト参照を`.cs`ファイルのGUIDで持つ。ファイルを削除して作り直したためGUIDが変わり、`QuestionManager`・`QuestionGenerator`の参照が切れた | `Tools > Learning Game > Setup Question System` で再割り当て。`CommonSceneWiringTests` が3件とも検出した | 解決 |
| 2026-10-15 | Scene UIを自動再生成しようとして、手で組み替えられたUIを壊しかけた | `SceneUIBuilder` はCanvasを丸ごと作り直す。Sceneには `AnswerButtons` という手作りの親オブジェクトがあり、ボタンがグループ化されていた。再生成の想定と実物がずれていた | 再生成をやめ、足りないボタンだけ複製する `Match Choice Buttons To Spec` を作った。batchmodeでは未保存のまま終了していたため実害なし | 解決 |
| 2026-10-15 | `QuestionManager` が `Task.Result` でリポジトリを待っていた | ローカル実装は同期で返るので動いていたが、サーバー実装に差し替えた瞬間にメインスレッドでデッドロックする書き方だった | 出題候補を手元にキャッシュする方式に変更し、`.Result` を全廃した | 解決 |
| 2026-10-15 | 再生すると `GameFlowController.Start()` で NullReferenceException になる状態だった | `modeSelectPanel` のフィールドは追加されていたが、Sceneで未割り当てだった。`Start()` が `modeSelectPanel.SetActive(true)` を呼ぶので、起動直後に必ず落ちる。コンパイルは通るため気づけない | `Setup Question System` が、空のパネル参照を同名のGameObjectで自動で埋めるようにした。`CommonSceneWiringTests` が検出 | 解決 |
| 2026-10-15 | 再生するたびにConsoleへ「The referenced script (Unknown) on this Behaviour is missing!」が出ていた | 旧 `QuestionManager.cs` を削除したとき、Sceneの QuestionManager オブジェクトに壊れたコンポーネントが1件残った。動作には影響しないが、放っておくと本物の警告が埋もれる | `Setup Question System` が Missing コンポーネントを取り除くようにした。実機で再生して警告が消えたことを確認済み | 解決 |
| 2026-10-15 | 分野選択に進んでも、タイトル画面が後ろに残ったまま重なって表示される | `GameFlowController.ShowSubjectSelect()` が `modeSelectPanel` を非表示にしていない。`ShowQuestion()` も同様 | 未対応（Knightさんの画面設計に関わるため確認待ち） | 未解決 |
| 2026-10-15 | 分野ボタンを押してもゲームが始まらない | `SubjectButton.cs` は分野名を保持するだけで、`GameFlowController` を呼び出していない。分野一覧をScroll Viewで動的生成する作りへの移行途中 | 未対応（Knightさんの作業中） | 未解決 |
| 2026-10-15 | 壊れたコンポーネントを除去したコミット(364ebba)が、`GameFlowController.questionManager` の参照を null にしていた | Missing スクリプトを取り除くと、同じGameObject上の生きた参照まで外れた。**テストを回さずにコミットしたため気づけなかった**（Unityを開いていてbatchmodeが使えず、シーンのGUID検査と再生だけで済ませた。再生は出題画面まで到達しないので露見しなかった） | `Setup Question System` で再割り当て。Test Runnerで141本すべて緑を確認してからコミットした | 解決 |
| 2026-10-15 | AI生成コードに、通信関連の不具合が4件あった | ①`StartRun` が `GetAwaiter().GetResult()` でブロックしており、サーバー実装にするとメインスレッドが固まる（BUGS.mdに「解決」と書いたが実際は未解決だった） ②投げっぱなしの非同期処理に catch が無く、例外が消える ③攻略を抜けても生成が走り続ける ④生成回数に上限が無く、API料金が青天井 | ①非ブロッキング化し `IsReady` を公開 ②`RunInBackground` で必ずログ ③`CancellationToken` で打ち切り ④`maxGenerationsPerRun`(既定20)で上限 | 解決 |
| 2026-10-16 | READMEのAI連携手順が、存在しない操作を指していた（`Setup Question System` を実行しても `AIClientSettings.asset` はできない） | 設定アセットを作るメニューは `Create AI Client Settings`（`OpenAISettingsCreator`）で、`Setup Question System` はSceneの参照を直すだけ。メニューを2つに分けたときREADMEを直さなかった。手順どおりにやっても失敗するため、チーム内で「ファイルが見つからない」と探す時間が発生した | READMEを `Create AI Client Settings` に修正し、配布用の手順書 `AI_SETUP.md` を追加した | 解決 |
| 2026-10-16 | プル後、プロジェクトがコンパイルできなくなった（`QuestionSystemDebugTest.cs(35,30): error CS1009: Unrecognized escape sequence`）。Unityが起動時にSafe Modeに入る | 新規追加された `QuestionSystemDebugTest.cs` がShift-JISで保存されていた。Shift-JISの日本語は2バイト目に `0x5C`（バックスラッシュ）を含むことがあり、UTF-8として読むと文字列リテラル内の不正なエスケープになる。**コミットした本人の環境では既存のキャッシュで動くため気づけない** | `iconv -f CP932 -t UTF-8` で変換（SHIFT_JIS指定だと `0x5C` が `¥` になり `\n` が壊れるのでCP932を使う）。Consoleの日本語が正しく出ることを再生して確認 | 解決 |
| 2026-10-16 | 画面遷移の2件（タイトルの重なり／分野ボタンが効かない）が、プル後も直っていないことを確認 | `ebb6ef8`「試験してみた」が触ったのはデバッグ用スクリプトとシーンへのその配置のみで、`SubjectButton.cs` と `GameFlowController` の画面制御は未着手。`SubjectButton` は `subjectName` を保持するだけで `GameFlowController` を呼んでいない | 未対応（Knightさん担当）。再生して再現を確認済み | 未解決 |
| 2026-10-22 | `ChoiceButtons.cs` に `using UnityEditor;` が入っている（ランタイムのスクリプト） | `UnityEditor` 名前空間はエディタ専用で、プレイヤービルドには含まれない。**iOS/Android/WebGLのビルドがコンパイルエラーで落ちる。** Editor上では動くので気づけない | 未対応（Knightさん担当ファイル）。1行消すだけで直る | 未解決 |
| 2026-10-22 | `QuizManager.cs` の日本語コメントとログがすべて文字化けしている | Shift-JISで保存されている。文字化けの4件目で、同じ原因が繰り返されている。今回はエスケープ事故にはならなかったが、ログが読めずデバッグの手がかりを失う | 未対応（Knightさん担当ファイル）。`iconv -f CP932 -t UTF-8` で変換できる。**エディタの既定文字コードをUTF-8にしないと再発し続ける** | 未解決 |
| 2026-10-22 | 選択肢UIの作り替えで `CommonSceneWiringTests` が NullReferenceException で落ちた | `QuestionUI` の `answerTexts`/`answerButtons` が廃止され、選択肢の管理が `ChoiceButtons` に移った。テストが古い構造を見ていた | テストを `ChoiceButtons.choiceButtons` の個数検査に書き換えた。6個前提でシャッフルしているので、数がずれると実行時に範囲外アクセスで落ちる | 解決 |

---

## 書き方

### 「なぜそうなったか」の書き分け

| 書くもの | 例 |
|---|---|
| ❌ 推測 | 「たぶんエンコードの問題」 |
| ✅ 確認したこと | 「ファイルがShift-JISで保存され、UTF-8として読まれている」 |

### いつ書くか

**その場で書く。** あとでまとめようとすると、なぜそう思ったかが消える。
1行でよい。きれいにするのは後。
