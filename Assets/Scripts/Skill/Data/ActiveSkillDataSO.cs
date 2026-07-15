using System.Collections.Generic;
using UnityEngine;

namespace Luo.Skill
{
    // 主动技能数据（继承自 SkillDataSO，新增窗口和连击）
    [CreateAssetMenu(fileName = "NewActiveSkill", menuName = "Skill/Active")]
    public class ActiveSkillDataSO : SkillDataSO
    {
        // 【组合】时间轴窗口（放在数据层，因为这是逻辑判定时间）
        public SkillTimeline timeline;

        [Header("进入该技能的条件（满足任意一个即可）")]
        public List<SkillCondition> conditions; // 新增

        // ===== 连击配置（多分支） =====
        [Header("连击分支列表，必须同时满足才会触发")]
        public List<SkillTransition> comboTransitions = new List<SkillTransition>();
        public bool canCombo;

        [Header("后摇可打断分支（触发后立刻打断当前技能），\n必须同时满足才会触发")]
        public List<SkillTransition> recoveryTransitions = new List<SkillTransition>();
        public bool canRecovery;

        
    }
}

