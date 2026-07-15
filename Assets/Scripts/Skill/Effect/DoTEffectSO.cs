using Luo.Buff;
using Luo.Character;
using UnityEngine;

namespace Luo.Effect
{
    // ³ÖÐøÉËº¦£¨DoT£©
    [CreateAssetMenu(fileName = "DoTEffect", menuName = "Skill/Effect/DoT")]
    public class DoTEffectSO : OTEffectSO
    {
        public override void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {
            var buff = new RuntimeBuff(context.duration, context.tickInterval, buffType,isUnique);
            buff.OnTick = (t) => t.TakeDamage(context.value, caster);
            target.AddBuff(buff);
        }
    }
}
