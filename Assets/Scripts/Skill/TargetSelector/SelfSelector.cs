using Luo.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Skill
{
    [CreateAssetMenu(fileName = "SelfSelector", menuName = "Skill/TargetSelector/Self")]
    public class SelfSelector : TargetSelector
    {
        public override List<TargetResult> GetTargets(CharacterUnit caster, Vector3 origin, ActiveSkillDataSO skillData)
        {
            //将自身作为目标，返回
            TargetResult result = new TargetResult();
            result.target = caster;
            var targets = new List<TargetResult>();
            targets.Add(result);
            return targets;
        }
    }
}
