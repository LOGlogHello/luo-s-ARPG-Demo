// SkillDataSO.cs
using Luo.Character;
using Luo.Effect;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Skill
{
    [System.Serializable]
    public class EffectConfig
    {
        [Tooltip("行为资产（DamageEffect、DoTEffect 等）")]
        public SkillEffectSO effectSO;

        [Tooltip("数值（伤害系数、治疗量）")]
        public float value = 1f;

        [Tooltip("持续时间（仅 DoT/HoT 需要）")]
        public float duration = 0f;

        [Tooltip("触发间隔（仅 DoT/HoT 需要）")]
        public float tickInterval = 1f;

        public void Execute(CharacterUnit caster, CharacterUnit target)
        {
            var context = new EffectContext
            {
                value = value,
                duration = duration,
                tickInterval = tickInterval
            };
            effectSO.Execute(caster, target, context);
        }

        public void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {
            context.value = value;
            context.duration = duration;
            context.tickInterval = tickInterval;
            effectSO.Execute(caster, target, context);
        }
    }

}