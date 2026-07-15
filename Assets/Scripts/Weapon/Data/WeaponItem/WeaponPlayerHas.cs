using UnityEngine;

namespace Luo
{
    /// <summary>
    /// 玩家持有的具体武器实例（动态数据），应该使用独立类，而不是ScriptObject
    /// </summary>
    [System.Serializable]
    public class WeaponPlayerHas
    {
        [Header("模板引用（只读）")]
        public WeaponBaseStats stats;      // 基础数值模板
        public WeaponCombatData combat;    // 战斗规则模板
        public WeaponViewData view;        // 表现层模板

        [Header("实例动态数据")]
        public int level = 1;              // 当前等级
        public float currentDurability=100f;    // 当前耐久
        public float extraDamageBonus=0f;     // 额外伤害加成（如附魔）
        public float extraDurabilityBonus=0f; // 额外耐久加成

        // ---- 计算最终属性（由模板 + 动态数据组合） ----
        public float GetFinalDamage()
        {
            float baseDamage = stats != null ? stats.baseDamage : 0;
            float levelBonus = (level - 1) * stats.damageIncreasedPerLevel;
            return baseDamage + levelBonus + extraDamageBonus;
        }

        public float GetFinalDurability()
        {
            float baseDurability = stats != null ? stats.baseDurability : 0;
            float levelBonus = (level - 1) * stats.durabilityIncreasedPerLevel;
            return baseDurability + levelBonus + extraDurabilityBonus;
        }

        // ---- 策划配置的成长系数（可放在 stats 中，或作为全局配置） ----
        // 这里为了演示，暂时写死，实际项目中可以放到 WeaponBaseStats 中
        
    }
}