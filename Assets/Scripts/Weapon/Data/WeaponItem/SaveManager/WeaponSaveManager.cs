using Luo;
using Luo.Events;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WeaponSaveManager : MonoBehaviour
{
    public static WeaponSaveManager Instance { get; private set; }

    [Header("武器清单（用于加载时重建）")]
    public WeaponCatalog weaponCatalog;

    // 存档文件路径
    private string SavePath => Path.Combine(Application.persistentDataPath, "weapon_save.json");

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ========================================================
    // 1. 保存武器数据
    // ========================================================
    public void SaveWeapons()
    {
        // 从 Model 中获取所有武器
        var allWeapons = WeaponItemModel.Instance.GetAllPlayerWeapons(); // 稍后实现这个方法

        // 转换为存档数据
        List<WeaponSaveData> saveDataList = new List<WeaponSaveData>();
        foreach (var weapon in allWeapons)
        {
            // 通过模板引用反向获取 ID（假设模板的 weaponID 存了标识符）
            // 前提：WeaponBaseStats 中有 weaponID 字段（之前已定义）
            int id = weapon.stats != null ? weapon.stats.weaponID : 0;

            saveDataList.Add(new WeaponSaveData
            {
                weaponID = id,
                weaponName = weapon.stats != null ? weapon.stats.weaponName : "Unknown",
                weaponType = weapon.stats != null ? weapon.stats.weaponType : WeaponType.None,
                level = weapon.level,
                currentDurability = weapon.currentDurability,
                extraDamageBonus = weapon.extraDamageBonus,
                extraDurabilityBonus = weapon.extraDurabilityBonus
            });
        }

        // 包装并序列化为 JSON
        var wrapper = new WeaponSaveDataWrapper
        {
            weapons = saveDataList.ToArray(),
            saveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        string json = JsonUtility.ToJson(wrapper, true); // true = 格式化缩进
        File.WriteAllText(SavePath, json);

        Debug.Log($"武器存档成功！共 {saveDataList.Count} 件武器，路径：{SavePath}");
    }

    // ========================================================
    // 2. 加载武器数据
    // ========================================================
    public void LoadWeapons()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("没有找到存档文件，使用默认初始化。");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var wrapper = JsonUtility.FromJson<WeaponSaveDataWrapper>(json);

        if (wrapper == null || wrapper.weapons == null)
        {
            Debug.LogError("存档文件损坏！");
            return;
        }

        // 清空 Model 中的现有武器
        WeaponItemModel.Instance.ClearAllWeapons(); // 稍后实现

        // 遍历存档数据，重建 WeaponPlayerHas 实例
        foreach (var saveData in wrapper.weapons)
        {
            // 1. 通过 ID 查找 Catalog 条目
            var entry = weaponCatalog.GetEntry(saveData.weaponType,saveData.weaponID);
            if (entry == null)
            {
                Debug.LogWarning($"未找到武器 ID：{saveData.weaponID}，跳过加载。");
                continue;
            }

            // 2. 从 Resources 加载三个模板
            WeaponBaseStats stats = Resources.Load<WeaponBaseStats>(entry.statPath);
            WeaponCombatData combat = Resources.Load<WeaponCombatData>(entry.combatPath);
            WeaponViewData view = Resources.Load<WeaponViewData>(entry.viewPath);

            if (stats == null || combat == null || view == null)
            {
                Debug.LogWarning($"武器 {saveData.weaponID} 模板加载失败，跳过。");
                continue;
            }

            // 3. 创建运行时 WeaponPlayerHas 实例，并恢复动态数据
            WeaponPlayerHas weapon = new WeaponPlayerHas
            {
                stats = stats,
                combat = combat,
                view = view,
                level = saveData.level,
                currentDurability = saveData.currentDurability,
                extraDamageBonus = saveData.extraDamageBonus,
                extraDurabilityBonus = saveData.extraDurabilityBonus
            };

            // 4. 添加到 Model
            WeaponItemModel.Instance.AddWeapon(saveData.weaponType, weapon);
        }

        Debug.Log($"武器加载成功！共加载 {wrapper.weapons.Length} 件武器。");

        // 加载完成后，主动派发一次列表变化事件，刷新 UI
        var currentType = WeaponItemModel.Instance.GetCurrentType();
        EventManager.Dispatch(new WeaponListChangedEvent
        {
            type = currentType,
            weapons = WeaponItemModel.Instance.GetWeaponsByType(currentType)
        });
    }

    // ========================================================
    // 3. 新增：从 Catalog 加载默认武器（首次启动或重置）
    // ========================================================
    public void LoadDefaultWeapons()
    {
        if (weaponCatalog == null || weaponCatalog.entries == null || weaponCatalog.entries.Count == 0)
        {
            Debug.LogError("武器清单为空，无法加载默认武器。");
            return;
        }

        // 清空现有武器（可选，根据需求决定是否清空）
        WeaponItemModel.Instance.ClearAllWeapons();

        // 遍历 Catalog 中的每个条目，创建默认武器
        foreach (var entry in weaponCatalog.entries)
        {
            // 加载三个模板
            WeaponBaseStats stats = Resources.Load<WeaponBaseStats>(entry.statPath);
            WeaponCombatData combat = Resources.Load<WeaponCombatData>(entry.combatPath);
            WeaponViewData view = Resources.Load<WeaponViewData>(entry.viewPath);

            if (stats == null || combat == null || view == null)
            {
                Debug.LogWarning($"武器 {entry.weaponName} (ID:{entry.weaponID}) 模板加载失败，跳过。");
                continue;
            }

            // 创建默认武器实例（等级1，满耐久，无额外加成）
            WeaponPlayerHas weapon = new WeaponPlayerHas
            {
                stats = stats,
                combat = combat,
                view = view,
                level = 1,
                currentDurability = stats.baseDurability, // 假设有 baseDurability
                extraDamageBonus = 0f,
                extraDurabilityBonus = 0f
            };

            WeaponItemModel.Instance.AddWeapon(entry.weaponType, weapon);
            Debug.Log($"加载默认武器：{entry.weaponName} (ID:{entry.weaponID})");
        }

        Debug.Log($"默认武器加载完成，共 {WeaponItemModel.Instance.GetAllPlayerWeapons().Count} 件武器。");

        // 派发刷新事件，让 UI 更新
        var currentType = WeaponItemModel.Instance.GetCurrentType();
        EventManager.Dispatch(new WeaponListChangedEvent
        {
            type = currentType,
            weapons = WeaponItemModel.Instance.GetWeaponsByType(currentType)
        });
    }
}