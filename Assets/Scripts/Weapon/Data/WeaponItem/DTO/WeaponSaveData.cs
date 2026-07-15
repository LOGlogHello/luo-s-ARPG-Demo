using System;
using UnityEngine;

namespace Luo
{
    /// <summary>
    /// 存档专用的武器数据（DTO），只存最小必要信息
    /// </summary>
    [Serializable]
    public class WeaponSaveData
    {
        // ===== 标识符（用于加载时重建模板引用） =====
        public int weaponID;           // 如 "GreatSword1"
        public string weaponName;
        public WeaponType weaponType;     // 枚举，用于快速分类（也可从ID解析）

        // ===== 动态数据（玩家的专属进度） =====
        public int level = 1;
        public float currentDurability = 100f;
        public float extraDamageBonus = 0f;
        public float extraDurabilityBonus = 0f;

        // 未来可扩展：是否装备中、宝石镶嵌ID列表等
        // public bool isEquipped;
        // public List<int> gemIds;
    }

    /// <summary>
    /// 存档文件根节点包装器（因为 JsonUtility 无法直接序列化 List<T>）
    /// </summary>
    [Serializable]
    public class WeaponSaveDataWrapper
    {
        public WeaponSaveData[] weapons;
        public string saveVersion = "1.0";
        public long saveTimestamp;
    }
}