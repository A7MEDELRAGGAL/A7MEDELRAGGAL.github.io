using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Clone.Editor
{
    public static class CloneWebBuild
    {
        private const string DefaultOutput = "Build/WebGL";
        private const string SceneName = "Main";

        [MenuItem("Tidebreak/Build WebGL")]
        public static void BuildFromMenu()
        {
            Build(Path.Combine(DefaultOutput, "Tidebreak"));
        }

        [MenuItem("Tidebreak/Configure WebGL Settings")]
        public static void ConfigureOnly()
        {
            ApplySettings();
            Debug.Log("Tidebreak: WebGL player settings applied.");
        }

        [MenuItem("Tidebreak/Validate WebGL Build Preconditions")]
        public static void Validate()
        {
            var issues = new System.Collections.Generic.List<string>();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                issues.Add("Active build target is not WebGL. Switch first, or let Build() do it.");

            if (!PlayerSettings.WebGL.linkerTarget)
                issues.Add("WebGL: linkerTarget should be Wasm for the default template.");

            if (PlayerSettings.WebGL.template != "APPLICATION:Default")
                issues.Add("WebGL template is not the default; custom templates need their own .js loader.");

            if (EditorUserBuildSettings.development)
                issues.Add("Development build is enabled; turn it off for a shippable build.");

            if (PlayerSettings.runInBackground)
                issues.Add("runInBackground is on; it does nothing meaningful in a browser tab.");

            if (PlayerSettings.WebGL.memorySize < 256)
                issues.Add("WebGL initial memory is low; 256 MB is a safer starting point for IL2CPP + FishNet.");

            if (issues.Count == 0)
            {
                Debug.Log("Tidebreak: WebGL preconditions look good.");
                return;
            }

            Debug.LogWarning("Tidebreak: WebGL preconditions:\n - " + string.Join("\n - ", issues));
        }

        public static void Build(string outputPath)
        {
            if (!EditorUserBuildSettings.activeBuildTarget.Equals(BuildTarget.WebGL)
                && !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                Debug.LogError("Tidebreak: could not switch the active build target to WebGL.");
                return;
            }

            ApplySettings();

            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string[] scenes = EditorBuildSettings.scenes.Length > 0
                ? new[] { System.IO.Path.Combine("Assets", "Scenes", SceneName + ".unity") }
                : new[] { System.IO.Path.Combine("Assets", "Scenes", SceneName + ".unity") };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Tidebreak: WebGL build succeeded in {summary.totalTime} " +
                          $"({summary.totalSize / (1024f * 1024f):0.0} MB) -> {outputPath}");
                return;
            }

            Debug.LogError($"Tidebreak: WebGL build {summary.result} with {summary.totalErrors} error(s).");
        }

        private static void ApplySettings()
        {
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.memorySize = 256;
            PlayerSettings.WebGL.initialMemorySize = 32;
            PlayerSettings.WebGL.maximumMemorySize = 256;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.WebGL.powerPreference = WebGLPowerPreference.HighPerformance;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WebGL, ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
        }
    }
}
