using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoBuild : MonoBehaviour
{
    public static string AssetBundleBasePath = "D:\\Temp\\Assets";
    public static string AssetBundleAndroidPath = AssetBundleBasePath + "\\Android";
    public static string AssetBundleWin64Path = AssetBundleBasePath + "\\Windows64";


    [MenuItem("Thinkin/Build AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        if (!Directory.Exists(AssetBundleAndroidPath)) Directory.CreateDirectory(AssetBundleAndroidPath);
        if (!Directory.Exists(AssetBundleWin64Path)) Directory.CreateDirectory(AssetBundleWin64Path);

        Debug.Log("Begin asset bundle build for Android");
        BuildPipeline.BuildAssetBundles(AssetBundleAndroidPath, BuildAssetBundleOptions.None, BuildTarget.Android);
        Debug.Log("Android build complete");

        Debug.Log("Begin asset bundle build for Win64");
        BuildPipeline.BuildAssetBundles(AssetBundleWin64Path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
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
        BuildPipeline.BuildAssetBundles(AssetBundleWin64Path, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        Debug.Log("Win64 build complete");
    }

    [MenuItem("Thinkin/Open Upload Windows")]
    public static void OpenUploadWindows()
    {
        System.Diagnostics.Process.Start(@"https://console.cloud.google.com/storage/browser/matriculate-assets");
        string path = "D:\\Temp\\Assets";
        System.Diagnostics.Process.Start(@"c:\windows\explorer.exe", path);
    }

    [MenuItem("Thinkin/Generate Scene Images")]
    public static void GenerateSceneImages()
    {
        var activeScene = EditorSceneManager.GetActiveScene().path;
        //var scenes = AssetDatabase.GetAllAssetBundleNames().SelectMany(i => AssetDatabase.GetAssetPathsFromAssetBundle(i)).Where(i => AssetDatabase.GetMainAssetTypeAtPath(i) == typeof(SceneAsset)).ToArray();

        var bundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (var bundle in bundles)
        {
            var scenePath = AssetDatabase.GetAssetPathsFromAssetBundle(bundle).Where(i => AssetDatabase.GetMainAssetTypeAtPath(i) == typeof(SceneAsset)).FirstOrDefault();
            if(scenePath != null)
            {
                EditorSceneManager.OpenScene(scenePath);
                generate360Image(bundle);
            }
        }

        EditorSceneManager.OpenScene(activeScene);
        Debug.Log("Generate images complete");
    }

    private static string getSceneNameFromScenePath(string scenePath)
    {
        // "Assets/Scenes/USC/Board Room/Board Room.unity"
        Debug.Log(scenePath);

        var start = scenePath.LastIndexOf('/');
        var end = scenePath.LastIndexOf(".unity");
        if(start > -1 && end > -1)
        {
            start++;
            return scenePath.Substring(start, end - start);
        }

        return null;
    }

    private static void generate360Image(string bundleName)
    {
        var cam = GameObject.FindObjectsOfType<Camera>(true).SingleOrDefault(i => i.gameObject.name == "Icon Camera");
        if (cam != null)
        {
            RenderTexture cubeMap = new RenderTexture(4096, 4096, 24);
            cubeMap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            cubeMap.Create();

            RenderTexture equiMap = new RenderTexture(4096, 2048, 24);
            equiMap.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            equiMap.Create();

            Texture2D tex = new Texture2D(equiMap.width, equiMap.height);

            cam.RenderToCubemap(cubeMap);
            cubeMap.ConvertToEquirect(equiMap);

            RenderTexture.active = equiMap;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToJPG();

            File.WriteAllBytes(AssetBundleAndroidPath + "\\" + bundleName + "-360.jpg", bytes);
            File.WriteAllBytes(AssetBundleWin64Path + "\\" + bundleName + "-360.jpg", bytes);

            cubeMap.Release();
            equiMap.Release();

            cam.gameObject.SetActive(false);

            Debug.Log("Scene Images complete");
        }
        else Debug.Log("No Icon Camera found");
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

    private void refreshBundleNames()
    {
        Debug.Log("Refreshing bundle names");
        bundleNames = AssetDatabase.GetAllAssetBundleNames();
    }

    private void OnGUI()
    {
        if (bundleNames == null) refreshBundleNames();

        GUILayout.Label("Auto Build Options", EditorStyles.boldLabel);

        if (GUILayout.Button("Build All"))
        {
            AutoBuild.BuildAllAssetBundles();
        }
        if (GUILayout.Button("Open Upload Windows"))
        {
            AutoBuild.OpenUploadWindows();
        }
        GUILayout.Space(10);
        GUILayout.Label("Project Asset Bundles", EditorStyles.boldLabel);

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