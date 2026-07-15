using Luo.Character;
using Luo.Events;
using UnityEngine;

namespace Luo.Effect
{
    // 伤害（一次性）
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "Skill/Effect/Damage")]
    public class DamageEffectSO : SkillEffectSO
    {
        public override void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {
            float finalDamage = caster.GetAttribute(AttributeType.Attack) * context.value;
            target.TakeDamage(finalDamage, caster);

            EventManager.Dispatch(new DamageTakenEvent
            {
                Target = target,
                Source = caster,
                Damage = finalDamage,
                hitDirection = context.hitDirection // 直接使用
            });
        }
    }
}
