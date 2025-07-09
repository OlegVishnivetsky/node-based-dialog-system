using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace cherrydev
{
    [InitializeOnLoad]
    public static class DefineSymbolInstaller
    {
        private const string TimelinePackageName = "com.unity.timeline";
        private const string TimelineDefineSymbol = "UNITY_TIMELINE";

        private const string LocalizationPackageName = "com.unity.localization";
        private const string LocalizationDefineSymbol = "UNITY_LOCALIZATION";

        private static ListRequest _listRequest;
        private static string EditorPrefKey => $"DefineSymbolInstaller_HasChecked_{Application.dataPath.GetHashCode()}";

        static DefineSymbolInstaller()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool(EditorPrefKey, false))
                {
                    EditorPrefs.SetBool(EditorPrefKey, true);
                    CheckPackagesAndDefineSymbols();
                }
            };
        }

        [MenuItem("Tools/Dialog System/Check Define Symbols")]
        public static void ForceCheckDefineSymbols() => CheckPackagesAndDefineSymbols();

        public static void CheckPackagesAndDefineSymbols()
        {
            Debug.Log("Check");
            _listRequest = Client.List(true, false);
            EditorApplication.update += OnListProgress;
        }

        private static void OnListProgress()
        {
            if (!_listRequest.IsCompleted)
                return;

            if (_listRequest.Status == StatusCode.Success)
            {
                bool timelineInstalled = _listRequest.Result.Any(pkg => pkg.name == TimelinePackageName);
                bool localizationInstalled = _listRequest.Result.Any(pkg => pkg.name == LocalizationPackageName);

                UpdateDefineSymbol(TimelineDefineSymbol, timelineInstalled);
                UpdateDefineSymbol(LocalizationDefineSymbol, localizationInstalled);
            }
        
            EditorApplication.update -= OnListProgress;
        }

        private static void UpdateDefineSymbol(string defineSymbol, bool shouldEnable)
        {
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out string[] currentDefines);
            List<string> definesList = currentDefines.ToList();
        
            bool contains = definesList.Contains(defineSymbol);

            if (shouldEnable && !contains)
            {
                definesList.Add(defineSymbol);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesList.ToArray());
                Debug.Log($"[DefineSymbolManager] Added {defineSymbol} define symbol for {namedBuildTarget.TargetName}");
            }
            else if (!shouldEnable && contains)
            {
                definesList.Remove(defineSymbol);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesList.ToArray());
                Debug.Log($"[DefineSymbolManager] Removed {defineSymbol} define symbol for {namedBuildTarget.TargetName}");
            }
        }
    }
}