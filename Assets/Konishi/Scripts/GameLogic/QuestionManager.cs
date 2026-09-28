using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    private readonly List<QuestionData> unityQuestions = new List<QuestionData>
    {
        new QuestionData("Unityで2D画像を表示するコンポーネントは？", new[] { "SpriteRenderer", "AudioSource", "Light", "Animator", "Camera" }, 0, "SpriteRendererはSpriteアセットを画面に描画します。", "SpriteRenderer"),
        new QuestionData("ゲーム開始時に一度だけ呼ばれるUnityメソッドは？", new[] { "Start", "Update", "LateUpdate", "FixedUpdate", "OnGUI" }, 0, "Startは有効なコンポーネントの初期化時に一度呼ばれます。", "ライフサイクル"),
        new QuestionData("UIボタンのクリック処理を登録するイベントは？", new[] { "onClick", "onDraw", "onMove", "onLoad", "onTick" }, 0, "Button.onClickにリスナーを登録するとクリック時に処理を呼べます。", "UIイベント"),
        new QuestionData("2D物理演算で使うコンポーネントは？", new[] { "Rigidbody2D", "Rigidbody3D", "PhysicsCamera", "ColliderUI", "ForceManager" }, 0, "Rigidbody2Dは2D物理演算の質量や速度を管理します。", "2D Physics")
    };

    private readonly List<QuestionData> generalQuestions = new List<QuestionData>
    {
        new QuestionData("水が海面で蒸発して雲になる過程は？", new[] { "蒸発", "凝固", "融解", "沈殿", "燃焼" }, 0, "蒸発は液体が気体へ変化する現象です。", "水循環"),
        new QuestionData("日本の国鳥は？", new[] { "キジ", "ツル", "ハト", "カラス", "ワシ" }, 0, "キジは日本の国鳥です。", "日本文化"),
        new QuestionData("光合成で植物が空気中から取り込む気体は？", new[] { "二酸化炭素", "酸素", "窒素", "水素", "ヘリウム" }, 0, "植物は二酸化炭素と水から養分を作ります。", "生物")
    };

    private List<QuestionData> activeQuestions;
    public int CurrentIndex { get; private set; }

    public void SelectSubject(string subject)
    {
        activeQuestions = subject == "Unity" ? unityQuestions : generalQuestions;
        CurrentIndex = 0;
    }

    public QuestionData Current => activeQuestions[CurrentIndex % activeQuestions.Count];
    public void MoveNext() => CurrentIndex = (CurrentIndex + 1) % activeQuestions.Count;
}
