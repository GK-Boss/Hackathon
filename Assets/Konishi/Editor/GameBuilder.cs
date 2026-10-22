#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 提出用のビルドをコマンド1つで作るためのスクリプト。
///
/// Tomo注: ビルドは手でボタンを押さず、ここから作ります。
///         圧縮形式や出力先が人によってぶれると、
///         「自分の環境では動いたのに相手の環境で動かない」が起きるためです。
///
///         コマンドラインから:
///           Unity -batchmode -quit -projectPath . -executeMethod GameBuilder.BuildMac
///           Unity -batchmode -quit -projectPath . -executeMethod GameBuilder.BuildWebGL
/// </summary>
public static class GameBuilder
{
    private const string MacOutput = "Build/Mac/KNOVEX.app";
    private const string WebGLOutput = "exe";

    [MenuItem("Tools/Learning Game/Build Mac App")]
    public static void BuildMac()
    {
        Run(BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone, MacOutput, "Macアプリ");
    }

    [MenuItem("Tools/Learning Game/Build WebGL")]
    public static void BuildWebGL()
    {
        // Brotli圧縮はサーバー側の設定が要る。どこに置いても開けるよう無圧縮にする。
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;

        Run(BuildTarget.WebGL, BuildTargetGroup.WebGL, WebGLOutput, "WebGL");
    }

    private static void Run(BuildTarget target, BuildTargetGroup group, string output, string label)
    {
        var scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Fail("ビルド対象のシーンがありません。File > Build Settings でシーンを追加してください。");
            return;
        }

        PlayerSettings.runInBackground = true;

        string directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

        Debug.Log("[Build] " + label + "のビルドを開始します。出力先: " + Path.GetFullPath(output));

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = target,
            targetGroup = group,
            options = BuildOptions.None
        });

        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[Build] " + label + " 成功。"
                      + " サイズ " + (summary.totalSize / 1024 / 1024) + " MB"
                      + " / " + Math.Round(summary.totalTime.TotalSeconds) + " 秒"
                      + " / " + Path.GetFullPath(output));
        }
        else
        {
            Fail(label + "のビルドに失敗しました: " + summary.result + "（エラー " + summary.totalErrors + " 件）");
        }
    }

    /// Build Settings で有効になっているシーンだけを返す。
    private static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled) scenes.Add(scene.path);
        }
        return scenes.ToArray();
    }

    private static void Fail(string message)
    {
        Debug.LogError("[Build] " + message);
        if (Application.isBatchMode) EditorApplication.Exit(1);
    }
}
#endif
