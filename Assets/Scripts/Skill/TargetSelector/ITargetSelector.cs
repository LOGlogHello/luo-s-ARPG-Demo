using Luo.Character;
using Luo.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Skill
{
    public struct TargetResult
    {
        public CharacterUnit target;
        [Tooltip("近战武器寻敌时会用到，传递武器Collider碰撞时与Target的相对方向，便于播放受击动画")]
        public Vector3 direction; 

        public TargetResult(CharacterUnit target, Vector3 direction)
        {
            this.target = target;
            this.direction = direction;
        }
    }
    public interface ITargetSelector
    {
        public List<TargetResult> GetTargets(CharacterUnit caster, Vector3 origin, ActiveSkillDataSO skillData);
    }
}


