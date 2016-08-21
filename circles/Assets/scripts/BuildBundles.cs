#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class BuildBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles/Windows", 
            BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles/Android",
            BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
#endif