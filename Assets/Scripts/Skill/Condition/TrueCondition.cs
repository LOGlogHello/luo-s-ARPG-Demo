using Luo.Character;
using UnityEngine;

namespace Luo.Skill
{
    [CreateAssetMenu(fileName = "TrueCondition", menuName = "Skill/Condition/True")]
    [System.Serializable]
    public class TrueCondition : SkillCondition
    {
        public override bool IsMet(CharacterUnit caster, ActiveSkillManager skillManager)
        {
            return true; 
        }
    }
}
