// 1. 基础技能数据（所有技能共有）
using Luo.Effect;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Skill
{
    public abstract class SkillDataSO : ScriptableObject 
    {
        public string skillName;
        public float cooldown; 
        public bool isActive; // true=主动, false=被动

        // 【组合】效果列表（由多个 Effect 组成）
        public List<EffectConfig> effects = new List<EffectConfig>();

        public SkillViewSO view;

        [Header("目标获取策略（近战/远程/范围等）")]
        public TargetSelector targetSelector;
    }
    
}

