using Luo.Character;
using Luo.Character.Controller;
using UnityEngine;

namespace Luo.Skill
{
    // 条件1：输入检测（攻击键、技能键等）
    [CreateAssetMenu(fileName = "InputBufferCondition", menuName = "Skill/Condition/InputBuffer")]
    [System.Serializable]
    public class InputBufferCondition : SkillCondition
    {
        public string inputActionName; // "Attack", "Skill1", "Dodge" 等

        public override bool IsMet(CharacterUnit caster, ActiveSkillManager skillManager)
        {
            // 尝试获取输入提供者（仅玩家拥有）
            var provider = caster.GetComponent<IInputProvider>();
            if (provider == null)
            {
                Debug.LogWarning("持有 输入类触发条件技能的角色 没有实现 IInputProvider 接口，无法检测输入。");
                return false;
            }

            return provider.mInputReader.mInputBuffer.ConsumeAttack();
        }
    }
} 
