using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CivilJam.Core.Editor
{
    /// <summary>
    /// Teslim build'i (A). Menü: CivilJam > Core > Build Windows (Release).
    /// Batch: Unity.exe -batchmode -quit -projectPath ... -executeMethod CivilJam.Core.Editor.ReleaseBuilder.BuildWindows
    /// Çıktı: Builds/Imza/Imza.exe (git dışında). Sahneler Build Settings'ten (MainMenu, Game).
    /// Release build: IMGUI yedek arayüzü kapalı, ekranı olmayan faz atlanır; F9/F10 demo tuşları açık.
    /// </summary>
    public static class ReleaseBuilder
    {
        const string OutputPath = "Builds/Imza/Imza.exe";

        [MenuItem("CivilJam/Core/Build Windows (Release)")]
        public static void BuildWindows()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) { Debug.LogError("[Build] Build Settings'te sahne yok."); return; }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            if (s.result == BuildResult.Succeeded)
                Debug.Log($"[Build] Başarılı: {OutputPath} ({s.totalSize / (1024 * 1024)} MB, {s.totalTime.TotalSeconds:0} sn, sahneler: {string.Join(", ", scenes)})");
            else
                Debug.LogError($"[Build] Başarısız: {s.result}, {s.totalErrors} hata");

            if (Application.isBatchMode) EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
