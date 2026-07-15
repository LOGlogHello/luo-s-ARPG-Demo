using Luo.Buff;
using Luo.Character;
using UnityEngine;

namespace Luo.Effect
{
    /// <summary>
    /// 持续治疗效果（Heal Over Time，HoT），继承自 OverTimeEffect。该类用于在一段时间内对目标单位持续恢复生命值。
    /// </summary>
    [CreateAssetMenu(fileName = "HoTEffect", menuName = "Skill/Effect/HoT")]
    public class HoTEffectSO : OTEffectSO
    {
        public float duration;
        public float tickInterval;

        public override void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {
            // 生成运行时实例，把“怎么执行”的逻辑通过委托传给实例
            var buff = new RuntimeBuff(context.duration, context.tickInterval,buffType,isUnique);
            buff.OnTick = (t) => t.Heal(context.value, caster);
            target.AddBuff(buff);
        }
    }
}
