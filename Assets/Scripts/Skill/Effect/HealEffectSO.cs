using Luo.Character;
using UnityEngine;

namespace Luo.Effect
{
    // 具体效果：回血
    [CreateAssetMenu(fileName = "HealEffect", menuName = "Skill/Effect/Heal")]
    public class HealEffectSO : SkillEffectSO
    {
        public override void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {
            float finalHeal = caster.GetAttribute(AttributeType.HealMultipler) * context.value;
            target.Heal(finalHeal, caster);
        }
    }
}
