// 放在 Assets/Editor 文件夹下
using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimationClipExtractor : EditorWindow
{
    [MenuItem("Tools/Extract FBX Animations")]
    static void ShowWindow()
    {
        GetWindow<AnimationClipExtractor>("提取 FBX 动画");
    }

    private void OnGUI()
    {
        GUILayout.Label("将选中文件夹（或 FBX 文件）中的动画提取为 .anim 文件", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("提取选中项", GUILayout.Height(30)))
        {
            ExtractSelected();
        }
    }

    [MenuItem("Assets/Extract FBX Animations", false, 1000)]
    static void ExtractFromContextMenu()
    {
        ExtractSelected();
    }

    static void ExtractSelected()
    {
        var selected = Selection.GetFiltered<Object>(SelectionMode.Assets);
        int extractedCount = 0;

        foreach (var obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (Directory.Exists(path))
            {
                // 选中的是文件夹，处理文件夹内所有 FBX
                extractedCount += ExtractFromDirectory(path);
            }
            else if (Path.GetExtension(path).ToLower() == ".fbx")
            {
                // 选中的是单个 FBX
                extractedCount += ExtractFromFBX(path);
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", $"共提取 {extractedCount} 个动画文件。", "确定");
    }

    static int ExtractFromDirectory(string directoryPath)
    {
        int count = 0;
        var fbxFiles = Directory.GetFiles(directoryPath, "*.fbx", SearchOption.AllDirectories);

        foreach (var fbxPath in fbxFiles)
        {
            count += ExtractFromFBX(fbxPath);
        }

        return count;
    }

    static int ExtractFromFBX(string fbxPath)
    {
        int count = 0;
        var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(fbxPath);

        foreach (var obj in objs)
        {
            if (obj is AnimationClip clip)
            {
                // 跳过已提取的（名字带 _Copy 或已有同名 .anim）
                string clipName = clip.name;
                string directory = Path.GetDirectoryName(fbxPath);
                string newPath = Path.Combine(directory, clipName + ".anim").Replace("\\", "/");

                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(newPath) != null)
                {
                    Debug.LogWarning($"跳过已存在: {newPath}");
                    continue;
                }

                // 复制动画
                AnimationClip newClip = new AnimationClip();
                EditorUtility.CopySerialized(clip, newClip);
                newClip.name = clipName;

                AssetDatabase.CreateAsset(newClip, newPath);
                Debug.Log($"提取: {newPath}");
                count++;
            }
        }

        return count;
    }
}