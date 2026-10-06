このフォルダには OpenAISettings.asset（OpenAIのAPIキー設定）を置きます。

OpenAISettings.asset はAPIキーを含むため .gitignore で除外しています。
リポジトリをクローンした人は、Unity Editorで次のメニューを実行してください。

  Tools > Learning Game > Setup AI Client

実行すると OpenAISettings.asset が作られ、SceneのAIClientも自動で用意されます。
あとは Inspector で Api Key を入力するだけです。

キーを入れなくても、ゲームは固定問題で動きます。
