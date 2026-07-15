using Luo;
using UnityEngine;


namespace Luo
{
    // 文件名：WeaponCombatData.cs
    [CreateAssetMenu(menuName = "GameData/Behavior/WeaponCombat")]
    public class WeaponCombatData : ScriptableObject
    {
        // 引用第一层的纯数值
        public WeaponBaseStats stats;

        //public float attackSpeed;     // 攻击速度系数（规则层独有）
        //public string attackAnimName; // 攻击动画状态机名称

        // 这里甚至可以封装复杂的计算方法（逻辑内聚）
        //public float CalculateDurabilityCost(float baseCost, int playerLevel)
        //{
        //    // 规则：每级减少0.5%耐久消耗，但引用 stats.baseDurability 计算上限
        //    float cost = baseCost * (1 - playerLevel * 0.005f);
        //    return Mathf.Min(cost, stats.baseDurability * 0.1f);
        //}
    }
}


