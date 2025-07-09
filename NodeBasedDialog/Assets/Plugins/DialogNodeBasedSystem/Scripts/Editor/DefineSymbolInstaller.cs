using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[InitializeOnLoad]
public static class DefineSymbolInstaller
{
    private const string TimelinePackageName = "com.unity.timeline";
    private const string TimelineDefineSymbol = "UNITY_TIMELINE";

    private const string LocalizationPackageName = "com.unity.localization";
    private const string LocalizationDefineSymbol = "UNITY_LOCALIZATION";

    private static ListRequest _listRequest;
    private const string EditorPrefKey = "DefineSymbolInstaller_HasChecked";

    static DefineSymbolInstaller()
    {
        if (!EditorPrefs.GetBool(EditorPrefKey, false))
        {
            EditorPrefs.SetBool(EditorPrefKey, true);
            CheckPackagesAndDefineSymbols();
        }
    }

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
        BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        bool contains = defines.Split(';').Contains(defineSymbol);

        if (shouldEnable && !contains)
        {
            defines = string.IsNullOrEmpty(defines) ? defineSymbol : $"{defines};{defineSymbol}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            Debug.Log($"[DefineSymbolManager] Added {defineSymbol} define symbol for {targetGroup}");
        }
        else if (!shouldEnable && contains)
        {
            IEnumerable<string> newDefines = defines.Split(';').Where(d => d != defineSymbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", newDefines));
            Debug.Log($"[DefineSymbolManager] Removed {defineSymbol} define symbol for {targetGroup}");
        }
    }
}