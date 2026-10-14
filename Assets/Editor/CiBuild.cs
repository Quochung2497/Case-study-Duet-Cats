using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Editor
{
    public static class CiBuild
    {
        public static void BuildWebGL()
        {
            try
            {
                var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                    .Select(scene => scene.path).ToArray();
                if (scenes.Length == 0)
                    throw new InvalidOperationException("Build Settings has no enabled scenes.");

                // GameCI supplies the output path when calling this method in batch mode.
                var arguments = Environment.GetCommandLineArgs();
                var pathIndex = Array.IndexOf(arguments, "-customBuildPath");
                if (pathIndex < 0 || pathIndex + 1 >= arguments.Length)
                    throw new InvalidOperationException("CI requires -customBuildPath.");

                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = arguments[pathIndex + 1],
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException($"WebGL build failed: {report.summary.result}, {report.summary.totalErrors} errors.");

                Debug.Log("CI WebGL build succeeded.");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }
    }
}
