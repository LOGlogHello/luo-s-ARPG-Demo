using Luo.Character;
using UnityEngine;

namespace Luo.Skill
{
    /// <summary>
    /// 过渡条件的基类（所有条件必须实现此接口）
    /// </summary>
    public abstract class SkillCondition : ScriptableObject
    {
        public abstract bool IsMet(CharacterUnit caster, ActiveSkillManager skillManager);
        //{
        //    return false;

        //}
    }

}