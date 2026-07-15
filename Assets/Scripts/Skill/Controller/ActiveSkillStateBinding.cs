using Luo.Character.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Skill
{
    // 暂时只有 主动技能与状态机有关
    [CreateAssetMenu(fileName = "NewActiveSkillStateBinding", menuName = "Skill/ActiveSkillStateBinding")]
    public class ActiveSkillStateBinding : ScriptableObject
    {
        [Tooltip("适用哪种控制器类型（Player/Enemy）")]
        public ControllerType controllerType;

        [Tooltip("触发前必须处于的状态 ID（例如 Player 的 Idle=1，Enemy 的 Chase=10）")]
        public int triggerStateID;

        [Tooltip("技能执行时进入的状态 ID")]
        public int targetStateID;

        [Tooltip("技能结束后回到的状态 ID")]
        public int exitStateID;

        [Header("全部满足才会执行该技能")]
        public List<SkillCondition> conditions;
    }
}