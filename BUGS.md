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
