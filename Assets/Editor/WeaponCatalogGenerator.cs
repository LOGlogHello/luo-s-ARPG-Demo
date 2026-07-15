// WeaponCatalogGenerator.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Luo;

public class WeaponCatalogGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Weapon Catalog")]
    public static void GenerateCatalog()
    {
        // 1. 找到 Resources/Weapon/WeaponItem 文件夹
        string rootPath = "Assets/Resources/Weapon/WeaponItem";
        if (!Directory.Exists(rootPath))
        {
            Debug.LogError($"路径不存在: {rootPath}");
            return;
        }

        // 2. 创建 Catalog 资产
        WeaponCatalog catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
        catalog.entries.Clear();

        // 3. 遍历所有子文件夹（武器类型目录）
        foreach (string typeDir in Directory.GetDirectories(rootPath))
        {
            string typeName = Path.GetFileName(typeDir);
            // 尝试将文件夹名转换为 WeaponType 枚举
            if (!System.Enum.TryParse(typeName, true, out WeaponType weaponType))
            {
                Debug.LogWarning($"跳过非枚举类型文件夹: {typeName}");
                continue;
            }

            // 4. 遍历该类型下的所有武器 Item 文件夹
            // WeaponCatalogGenerator.cs (核心修改部分)
            foreach (string itemDir in Directory.GetDirectories(typeDir))
            {
                string itemName = Path.GetFileName(itemDir);

                // 1. 获取三个资产路径
                string statPath = FindAssetPath(itemDir, itemName, "Stat");
                string combatPath = FindAssetPath(itemDir, itemName, "Combat");
                string viewPath = FindAssetPath(itemDir, itemName, "View");

                if (string.IsNullOrEmpty(statPath) || string.IsNullOrEmpty(combatPath) || string.IsNullOrEmpty(viewPath))
                {
                    Debug.LogWarning($"武器 {itemName} 缺少必要的资产文件，跳过。");
                    continue;
                }

                // 2. 加载 Stat 资产（编辑器下使用 AssetDatabase）
                WeaponBaseStats statsAsset = AssetDatabase.LoadAssetAtPath<WeaponBaseStats>(statPath);
                if (statsAsset == null)
                {
                    Debug.LogWarning($"无法加载 Stat 资产：{statPath}，跳过。");
                    continue;
                }

                // 3. 提取 ID 和名称
                int weaponID = statsAsset.weaponID;          // 假设 WeaponBaseStats 中有 int weaponID
                string weaponName = statsAsset.weaponName;   // 如果有，就用；否则 fallback 到文件夹名
                if (string.IsNullOrEmpty(weaponName))
                    weaponName = itemName;                   // 保证不为空

                // 4. 加入 Catalog
                catalog.entries.Add(new WeaponCatalogEntry
                {
                    weaponType = weaponType,
                    weaponID = weaponID,
                    weaponName = weaponName,
                    statPath = TrimResourcesPath(statPath),
                    combatPath = TrimResourcesPath(combatPath),
                    viewPath = TrimResourcesPath(viewPath)
                });
            }
        }

        // 5. 保存 Catalog 到 Resources 文件夹（以便运行时加载）
        string savePath = "Assets/Resources/Weapon/WeaponItem/WeaponCatalog.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        AssetDatabase.CreateAsset(catalog, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"武器清单生成完成，共 {catalog.entries.Count} 件武器。");
    }

    private static string FindAssetPath(string dir, string itemName, string suffix)
    {
        // 查找文件：itemName_suffix.asset（忽略大小写）
        string[] files = Directory.GetFiles(dir, $"{itemName}_{suffix}*.asset");
        if (files.Length > 0)
            return files[0]; // 返回完整路径
        return null;
    }

    private static string TrimResourcesPath(string fullPath)
    {
        // 从 "Assets/Resources/Weapon/WeaponItem/GreatSword/GreatSword1/GreatSword1_Stat.asset"
        // 变为 "Weapon/WeaponItem/GreatSword/GreatSword1/GreatSword1_Stat"
        const string resourcesPrefix = "Assets/Resources/";
        if (fullPath.StartsWith(resourcesPrefix))
        {
            string relative = fullPath.Substring(resourcesPrefix.Length);
            return relative.Replace(".asset", ""); // Resources.Load 不需要扩展名
        }
        return fullPath;
    }
}
#endif