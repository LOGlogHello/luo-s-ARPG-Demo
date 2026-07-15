using UnityEngine;
using UnityEditor;
using System.IO;

public class RenameEmbeddedAnimations
{
    [MenuItem("Tools/Rename Embedded Animations to FBX Name")]
    private static void RenameAllEmbeddedAnimations()
    {
        string folderPath = GetSelectedFolderPath();
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogWarning("请在 Project 窗口中选中一个文件夹，再执行此菜单。");
            return;
        }

        // 查找所有 FBX 模型
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
        if (guids.Length == 0)
        {
            Debug.Log("未找到任何 FBX 模型文件。");
            return;
        }

        int totalRenamed = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                continue;

            Debug.Log($"处理 FBX: {assetPath}");

            // 获取 ModelImporter
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"无法获取 ModelImporter: {assetPath}");
                continue;
            }

            // 确保导入动画
            if (!importer.importAnimation)
            {
                Debug.Log($"  └─ 未启用 Import Animation，跳过");
                continue;
            }

            // 获取当前动画剪辑设置（优先使用已存在的自定义设置）
            ModelImporterClipAnimation[] clipAnimations = importer.clipAnimations;
            if (clipAnimations == null || clipAnimations.Length == 0)
            {
                // 如果从未自定义过，则使用默认动画剪辑
                clipAnimations = importer.defaultClipAnimations;
                if (clipAnimations == null || clipAnimations.Length == 0)
                {
                    Debug.Log($"  └─ 没有动画剪辑");
                    continue;
                }
            }

            string fbxName = Path.GetFileNameWithoutExtension(assetPath);
            bool anyRenamed = false;

            // 遍历每个动画剪辑设置
            foreach (var clip in clipAnimations)
            {
                // 如果名称已经相同，跳过
                if (clip.name == fbxName)
                {
                    Debug.Log($"  └─ 动画 '{clip.name}' 已命名为 '{fbxName}'，跳过");
                    continue;
                }

                // 修改名称
                string oldName = clip.name;
                clip.name = fbxName;
                anyRenamed = true;
                totalRenamed++;
                Debug.Log($"  ✅ 重命名动画: '{oldName}' → '{fbxName}'");
            }

            // 如果有修改，应用并重新导入
            if (anyRenamed)
            {
                importer.clipAnimations = clipAnimations;
                importer.SaveAndReimport();
                Debug.Log($"  └─ 已重新导入 FBX，重命名生效");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"🎉 完成！共重命名了 {totalRenamed} 个内嵌动画剪辑。");
    }

    private static string GetSelectedFolderPath()
    {
        if (Selection.activeObject == null)
            return null;

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (AssetDatabase.IsValidFolder(path))
            return path;

        if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(Path.GetExtension(path)))
        {
            string dir = Path.GetDirectoryName(path);
            if (AssetDatabase.IsValidFolder(dir))
                return dir;
        }

        return null;
    }
}