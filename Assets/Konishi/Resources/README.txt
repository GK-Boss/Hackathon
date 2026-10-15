このフォルダには2つのファイルを置きます。

1. questions_seed.json
   人が目視検証した固定問題です。デモの本線になります。
   gitに入れるので、テキストエディタで直接編集して問題を増やしてください。
   Unityを開く必要はありません。書き方は README.md を見てください。

2. AIClientSettings.asset
   AIへの接続設定です。接続情報を含むため .gitignore で除外しています。
   リポジトリをクローンした人は、Unity Editorで次を実行してください。

     Tools > Learning Game > Setup Question System

   実行すると AIClientSettings.asset が作られ、SceneのQuestionManagerも用意されます。
   あとは Inspector で接続先を設定するだけです。

   本番は Mode = GameServer（自前サーバー経由）にしてください。
   OpenAIのAPIキーはサーバー側だけに置き、Unityのビルドには含めません。

AIを設定しなくても、questions_seed.json の問題でゲームは動きます。
