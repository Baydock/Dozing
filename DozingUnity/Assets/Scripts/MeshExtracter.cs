#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;

public class FBXMeshExtractor {
    private const string progressTitle = "Extracting Meshes";
    private static readonly string[] sourceExtensions = { ".fbx", ".obj" };


    [MenuItem("Assets/Extract Meshes and Animations", validate = true)]
    private static bool ExtractMeshesMenuItemValidate() {
        for (int i = 0; i < Selection.objects.Length; i++) {
            if (!EndsWithAnyValidExt(AssetDatabase.GetAssetPath(Selection.objects[i])))
                return false;
        }
        return true;
    }

    private static bool EndsWithAnyValidExt(string path) {
        foreach (string ext in sourceExtensions)
            if (path.EndsWith(ext))
                return true;
        return false;
    }

    [MenuItem("Assets/Extract Meshes and Animations")]
    private static void ExtractMeshesMenuItem() {
        ShowProgress("", 0, 1);
        for (int i = 0; i < Selection.objects.Length; i++) {
            ExtractAssets(Selection.objects[i]);
            ShowProgress(Selection.objects[i].name, i, Selection.objects.Length);
        }
        EditorUtility.ClearProgressBar();
    }

    private static void ExtractAssets(Object selectedObject) {
        string selectedObjectPath = AssetDatabase.GetAssetPath(selectedObject);
        string parentfolderPath = selectedObjectPath.Substring(0, selectedObjectPath.Length - (selectedObject.name.Length + 5));

        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(selectedObjectPath);

        for (int i = 0; i < objects.Length; i++) {
            Object obj = objects[i];
            string info = $"{selectedObject.name} : {obj.name}";
            if (obj is Mesh)
                ExtractAsset(selectedObject, obj, $"{parentfolderPath}/{obj.name}.asset", info, i, objects.Length);
            else if (obj is AnimationClip)
                ExtractAsset(selectedObject, obj, $"{parentfolderPath}/{obj.name.Substring(obj.name.IndexOf('|') + 1)}.asset", info, i, objects.Length);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ShowProgress(string info, int index, int length) => EditorUtility.DisplayProgressBar(progressTitle, info, (float)(index + 1) / length);

    private static void ExtractAsset(Object selectedObject, Object obj, string path, string info, int index, int length) {
        ShowProgress($"{selectedObject.name} : {obj.name}", index, length); ;

        AssetDatabase.CreateAsset(Object.Instantiate(obj), path);
    }
}

#endif