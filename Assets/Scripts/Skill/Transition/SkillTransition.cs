using Luo.Skill;
using UnityEngine;

[System.Serializable]
public class SkillTransition
{
    [Tooltip("目标技能（下一段）")] 
    public ActiveSkillDataSO targetSkill;

    [Tooltip("是否占用连击计数（用于消耗层数）")]
    public bool consumeComboCount = false;
}