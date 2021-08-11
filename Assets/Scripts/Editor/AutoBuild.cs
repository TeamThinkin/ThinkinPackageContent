using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AutoBuild : MonoBehaviour
{
    [MenuItem("Thinkin/Build Addressables")]
    public static void ExecuteBuild()
    {
        Debug.Log("Building addressables packages");
        
        Debug.Log("Switching build platform to Android");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        Debug.Log("Building Android package");
        AddressableAssetSettings.BuildPlayerContent();

        Debug.Log("Switching build platform to Windows (x86)");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
        Debug.Log("Building Windows package");
        AddressableAssetSettings.BuildPlayerContent();

        Debug.Log("Addressables Build Complete");

        OpenUploadWindows();
    }

    [MenuItem("Thinkin/Open Upload Windows")]
    public static void OpenUploadWindows()
    {
        System.Diagnostics.Process.Start(@"https://console.cloud.google.com/storage/browser/matriculate-assets");
        string path = Application.dataPath.Replace('/', '\\') + @"\..\ServerData";
        System.Diagnostics.Process.Start(@"c:\windows\explorer.exe", path);
    }


}
