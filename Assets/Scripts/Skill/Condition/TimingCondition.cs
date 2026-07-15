using Luo.Character;
using UnityEngine;

namespace Luo.Skill
{
    // 条件2：满足一定时间要求（比如技能进度在20%到60%之间）
    [CreateAssetMenu(fileName = "TimingCondition", menuName = "Skill/Condition/Timing")]
    [System.Serializable]
    public class TimingCondition : SkillCondition
    {
        public float minProgress = 0.2f;
        public float maxProgress = 0.6f;

        public override bool IsMet(CharacterUnit caster, ActiveSkillManager skillManager)
        {
            float progress = skillManager.mCurrentSkillProcess; // 当前技能进度
            return progress >= minProgress && progress <= maxProgress;
        }
    }
}
