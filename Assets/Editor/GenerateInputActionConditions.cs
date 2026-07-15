using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.IO;

namespace Luo.Skill.Editor
{
    public static class GenerateInputActionConditions
    {
        [MenuItem("Tools/Generate InputActionConditions from .inputactions")]
        private static void Generate()
        {
            var selected = Selection.activeObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("错误", "请先在 Project 窗口中选中一个 .inputactions 文件", "确定");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".inputactions"))
            {
                EditorUtility.DisplayDialog("错误", "选中的不是 .inputactions 文件", "确定");
                return;
            }

            var inputActionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            if (inputActionAsset == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载 InputActionAsset", "确定");
                return;
            }

            string outputRoot = Path.GetDirectoryName(assetPath);
            int totalCreated = 0;
            int skipped = 0;

            foreach (var map in inputActionAsset.actionMaps)
            {
                string mapFolder = Path.Combine(outputRoot, map.name);
                if (!Directory.Exists(mapFolder))
                    Directory.CreateDirectory(mapFolder);

                foreach (var action in map.actions)
                {
                    string assetName = $"{map.name}_{action.name}_Condition.asset";
                    string fullPath = Path.Combine(mapFolder, assetName);

                    // ★ 关键修改：已存在则直接跳过，不再询问覆盖
                    if (File.Exists(fullPath))
                    {
                        skipped++;
                        Debug.Log($"已存在，跳过: {assetName}");
                        continue;
                    }

                    // 创建 InputActionReference
                    var actionRef = InputActionReference.Create(action);
                    if (actionRef == null)
                    {
                        Debug.LogWarning($"无法为 action '{action.name}' 创建 InputActionReference，跳过");
                        continue;
                    }

                    // 创建 InputActionCondition
                    var condition = ScriptableObject.CreateInstance<InputActionCondition>();
                    condition.inputAction = actionRef;
                    condition.requiredState = InputActionCondition.InputState.Pressed;

                    // 先创建主资产，再将 actionRef 添加为子资产
                    AssetDatabase.CreateAsset(condition, fullPath);
                    AssetDatabase.AddObjectToAsset(actionRef, condition);

                    EditorUtility.SetDirty(condition);
                    AssetDatabase.SaveAssetIfDirty(condition);

                    totalCreated++;
                    Debug.Log($"已创建: {fullPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", $"新增 {totalCreated} 个，跳过 {skipped} 个已存在", "确定");
        }

        [MenuItem("Assets/Create/Skill/Transition/Generate InputActionConditions from .inputactions", true)]
        private static bool ValidateGenerate()
        {
            var selected = Selection.activeObject;
            if (selected == null) return false;
            string path = AssetDatabase.GetAssetPath(selected);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".inputactions");
        }
    }
}