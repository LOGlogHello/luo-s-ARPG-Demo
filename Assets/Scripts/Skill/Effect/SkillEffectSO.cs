using Luo.Buff;
using Luo.Character;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Luo.Effect
{
    public  class SkillEffectSO : ScriptableObject
    {
        public virtual void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {

        }
    }

    // 上下文：携带所有运行时数据
    public struct EffectContext
    {
        public float value;
        public float duration;
        public float tickInterval;
        public Vector3 hitDirection; // 新增：受击方向（世界空间）

        // 可扩展：伤害类型、元素属性等
    }
}


