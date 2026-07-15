using Luo.Buff;
using Luo.Character;
using UnityEngine;

namespace Luo.Effect
{
    // ³ÖÐøÉËº¦£¨DoT£©
    [CreateAssetMenu(fileName = "OTAttackBoostSO", menuName = "Skill/Effect/OTAttackBoostSO")]
    public class OTAttackBoostSO : OTEffectSO
    {
        public override void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {
            var buff = new RuntimeBuff(context.duration, context.tickInterval, buffType, isUnique);

            buff.OnApply = (t) => t.ModifyAttribute(AttributeType.Attack, context.value, caster);
            buff.OnExpire = (t) => t.ModifyAttribute(AttributeType.Attack, -context.value, caster);
            target.AddBuff(buff);
        }
    }
}