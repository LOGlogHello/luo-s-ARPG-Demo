using Luo;
using Luo.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Luo
{
    public enum WeaponType
    {
        None = 0,
        GreatSword,
        LongBow,
        SwordAndShield,
        SumNum
    }

    // 文件名：WeaponBaseStats.cs
    [CreateAssetMenu(menuName = "GameData/Core/WeaponStats")]
    public class WeaponBaseStats : ScriptableObject
    {
        public WeaponType weaponType;
        public int weaponID;
        public string weaponName;
        public float baseDamage;      // 基础伤害
        public float baseDurability;  // 基础耐久
        public float damageIncreasedPerLevel = 5;
        public float durabilityIncreasedPerLevel = 2f;

        public List<ActiveSkillController> skills; // 该武器拥有的技能列表（按顺序）
        //public float weight;          // 重量（影响攻速）                                                        
        //public int requiredLevel;     // 需求等级                                                           
        // 注意：这里没有模型，没有技能，只有干巴巴的数字！
    }
}


