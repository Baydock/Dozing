#if UNITY_EDITOR

using System.IO;
using UnityEditor;

public class CreateAssetBundles : Editor {
    private const string Path = "Assets/AssetBundles/";
    [MenuItem("Assets/Build Asset Bundles")]
    static void CreateBundle() {
        if (!Directory.Exists(Path))
            Directory.CreateDirectory(Path);
        BuildPipeline.BuildAssetBundles(Path, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);
    }
}

#endif