using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AutoBuild : MonoBehaviour
{
    public static string AssetBundleBasePath = "D:\\Temp\\Assets";
    public static string AssetBundleAndroidPath = AssetBundleBasePath + "\\Android";
    public static string AssetWin64Path = AssetBundleBasePath + "\\Windows64";


    [MenuItem("Thinkin/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        if (!Directory.Exists(AssetBundleAndroidPath)) Directory.CreateDirectory(AssetBundleAndroidPath);
        if (!Directory.Exists(AssetWin64Path)) Directory.CreateDirectory(AssetWin64Path);

        Debug.Log("Begin asset bundle build for Android");
        BuildPipeline.BuildAssetBundles(AssetBundleAndroidPath, BuildAssetBundleOptions.None, BuildTarget.Android);
        Debug.Log("Android build complete");

        Debug.Log("Begin asset bundle build for Win64");
        BuildPipeline.BuildAssetBundles(AssetWin64Path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        Debug.Log("Win64 build complete");

        OpenUploadWindows();
    }

    public static void BuildAssetBundlesByName(params string[] assetBundleNames)
    {
        if (assetBundleNames == null || assetBundleNames.Length == 0) return;

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        foreach (string assetBundle in assetBundleNames)
        {
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundle);

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = assetBundle;
            build.assetNames = assetPaths;

            builds.Add(build);
        }

        Debug.Log("Begin asset bundle build for Android");
        BuildPipeline.BuildAssetBundles(AssetBundleAndroidPath, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.Android);
        Debug.Log("Android build complete");

        Debug.Log("Begin asset bundle build for Win64");
        BuildPipeline.BuildAssetBundles(AssetWin64Path, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        Debug.Log("Win64 build complete");
    }

    [MenuItem("Thinkin/Open Upload Windows")]
    public static void OpenUploadWindows()
    {
        System.Diagnostics.Process.Start(@"https://console.cloud.google.com/storage/browser/matriculate-assets");
        string path = "D:\\Temp\\Assets";
        System.Diagnostics.Process.Start(@"c:\windows\explorer.exe", path);
    }
}

public class BundlesWindow : EditorWindow
{
    private string[] bundleNames;

    [MenuItem("Thinkin/Show Asset Bundles")]
    public static void OpenWindow()
    {
        var window = EditorWindow.GetWindow<BundlesWindow>();
        window.Show();
    }

    public BundlesWindow()
    {
        refreshBundleNames();
    }

    private void refreshBundleNames()
    {
        Debug.Log("Refreshing bundle names");
        bundleNames = AssetDatabase.GetAllAssetBundleNames();
    }

    private void OnGUI()
    {
        GUILayout.Label("Project Asset Bundles", EditorStyles.boldLabel);

        if (bundleNames == null) refreshBundleNames();
        
        foreach (var name in bundleNames)
        {
            if (GUILayout.Button(name))
            {
                Debug.Log("Build asset bundle for: " + name);
                AutoBuild.BuildAssetBundlesByName(name);
            }
        }

        GUILayout.Space(10);
        if(GUILayout.Button("Refresh List"))
        {
            refreshBundleNames();
        }
    }
}