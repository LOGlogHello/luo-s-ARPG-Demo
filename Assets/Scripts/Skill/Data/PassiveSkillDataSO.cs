using UnityEngine;

namespace Luo.Skill
{

    //被动技能数据（继承自 SkillDataSO，新增触发条件）
    [CreateAssetMenu(fileName = "NewPassiveSkill", menuName = "Skill/Passive")]
    public class PassiveSkillDataSO : SkillDataSO
    {
        //public TriggerCondition triggerCondition; // 如 OnHit, OnLowHP
    }
}

