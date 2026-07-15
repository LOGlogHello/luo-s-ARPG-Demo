using Luo.Character;
using UnityEngine;

namespace Luo.Skill
{
    // 条件3：目标状态（比如敌人血量低于30%）
    [CreateAssetMenu(fileName = "TargetHealthCondition", menuName = "Skill/Condition/TargetHealth")]
    [System.Serializable]
    public class TargetHealthCondition : SkillCondition
    {
        public float healthThreshold = 0.3f; // 30%
        public bool belowThreshold = true;

        public override bool IsMet(CharacterUnit caster, ActiveSkillManager skillManager)
        {
            //if (target == null) return false;
            //float healthPercent = target.CurrentHealth / target.MaxHealth;
            //return belowThreshold ? healthPercent <= healthThreshold : healthPercent > healthThreshold;
            return false;
        }
    }
}
