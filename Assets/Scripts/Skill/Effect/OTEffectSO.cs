using Luo.Buff;
using UnityEngine;

namespace Luo.Effect
{
    [Tooltip("持续起作用的效果，即Buff")]
    public class OTEffectSO : SkillEffectSO
    {
        public BuffType buffType;
        public bool isUnique = false;
    }
}
