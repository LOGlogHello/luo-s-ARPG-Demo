// TargetSelectorBase.cs
using UnityEngine;
using System.Collections.Generic;
using Luo.Character;

namespace Luo.Skill
{
    /// <summary>
    /// 目标选择器基类（所有具体选择器都继承此 SO）
    /// </summary>
    public abstract class TargetSelector : ScriptableObject,ITargetSelector
    {


        public abstract List<TargetResult> GetTargets(CharacterUnit caster, Vector3 origin, ActiveSkillDataSO skillData);
    }
}