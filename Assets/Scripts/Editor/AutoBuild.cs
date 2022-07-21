using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public class CreateAssetBundles
    {
        [MenuItem("Thinkin/Build AssetBundles")]
        static void BuildAllAssetBundles()
        {
            string basePath = "D:\\Temp\\Assets";
            string androidPath = basePath + "\\Android";
            string win64Path = basePath + "\\Windows64";

            if (!Directory.Exists(androidPath)) Directory.CreateDirectory(androidPath);
            if (!Directory.Exists(win64Path)) Directory.CreateDirectory(win64Path);

            BuildPipeline.BuildAssetBundles(win64Path, BuildAssetBundleOptions.None, BuildTarget.Android);
            BuildPipeline.BuildAssetBundles(win64Path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }
    }
}
