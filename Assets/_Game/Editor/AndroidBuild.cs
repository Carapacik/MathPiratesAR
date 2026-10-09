using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MathPirates.Editor
{
    public static class AndroidBuild
    {
        [MenuItem("Math Pirates/Build Android development APK")]
        public static void Development() => Build(false);

        [MenuItem("Math Pirates/Build Android signed release APK and AAB")]
        public static void Release() => Build(true);

        private static void Build(bool release)
        {
            ProjectSetup.Prepare();
            string keyName = PlayerSettings.Android.keystoreName;
            string alias = PlayerSettings.Android.keyaliasName;
            string keyPass = PlayerSettings.Android.keystorePass;
            string aliasPass = PlayerSettings.Android.keyaliasPass;
            bool custom = PlayerSettings.Android.useCustomKeystore;
            bool bundle = EditorUserBuildSettings.buildAppBundle;
            try
            {
                PlayerSettings.Android.useCustomKeystore = release;
                if (release) ConfigureSigning();
                Directory.CreateDirectory("Builds/Android");
                void BuildArtifact(bool appBundle)
                {
                    EditorUserBuildSettings.buildAppBundle = appBundle;
                    var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                    {
                        scenes = new[] { ProjectSetup.ScenePath },
                        locationPathName = appBundle ? "Builds/Android/MathPiratesAR-2.0.0.aab" : release ? "Builds/Android/MathPiratesAR-2.0.0.apk" : "Builds/Android/MathPiratesAR-2.0.0-dev.apk",
                        target = BuildTarget.Android,
                        options = release ? BuildOptions.None : BuildOptions.Development
                    });
                    if (report.summary.result != BuildResult.Succeeded)
                        throw new BuildFailedException("Android build failed. See the Unity build log.");
                }
                BuildArtifact(release);
                if (release) BuildArtifact(false);
            }
            finally
            {
                PlayerSettings.Android.keystoreName = keyName;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keystorePass = keyPass;
                PlayerSettings.Android.keyaliasPass = aliasPass;
                PlayerSettings.Android.useCustomKeystore = custom;
                EditorUserBuildSettings.buildAppBundle = bundle;
                AssetDatabase.SaveAssets();
            }
        }

        private static void ConfigureSigning()
        {
            string file = Environment.GetEnvironmentVariable("MATHPIRATES_SIGNING_PROPERTIES");
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                throw new BuildFailedException("Set MATHPIRATES_SIGNING_PROPERTIES to an external key.properties file.");
            var values = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(file))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("#") || trimmed.StartsWith("!")) continue;
                int separator = trimmed.IndexOf('=');
                if (separator > 0) values[trimmed.Substring(0, separator).Trim()] = trimmed.Substring(separator + 1).Trim();
            }
            foreach (string key in new[] { "storeFile", "storePassword", "keyAlias", "keyPassword" })
                if (!values.ContainsKey(key) || string.IsNullOrEmpty(values[key])) throw new BuildFailedException("Signing properties are incomplete.");
            string keystore = Environment.GetEnvironmentVariable("MATHPIRATES_KEYSTORE");
            if (string.IsNullOrEmpty(keystore)) keystore = values["storeFile"].Replace("\\\\", "\\").Replace("\\:", ":");
            if (!Path.IsPathRooted(keystore)) keystore = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(file)), keystore);
            if (!File.Exists(keystore)) throw new BuildFailedException("The external signing keystore was not found.");
            PlayerSettings.Android.keystoreName = Path.GetFullPath(keystore);
            PlayerSettings.Android.keystorePass = values["storePassword"];
            PlayerSettings.Android.keyaliasName = values["keyAlias"];
            PlayerSettings.Android.keyaliasPass = values["keyPassword"];
        }
    }

    // Pin both Gradle modules; Unity otherwise derives compileSdk from installed platform versions.
    public sealed class AndroidSdkPolicy : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string root = Directory.GetParent(path).FullName;
            foreach (string module in new[] { "unityLibrary", "launcher" })
            {
                string file = Path.Combine(root, module, "build.gradle");
                string text = File.ReadAllText(file);
                if (!Regex.IsMatch(text, @"(?m)^\s*compileSdk(?:Version)?\s+.+$"))
                    throw new BuildFailedException("Could not verify the Android compile SDK declaration.");
                text = Regex.Replace(text, @"(?m)^(\s*)compileSdk(?:Version)?\s+.+$", "$1compileSdk 37");
                File.WriteAllText(file, text);
            }
        }
    }
}

