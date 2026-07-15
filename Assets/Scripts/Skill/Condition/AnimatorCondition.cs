using Luo.Character;
using Luo.Character.Controller;
using UnityEngine;

namespace Luo.Skill
{
    /// <summary>
    /// 检测 Animator 参数条件的 SkillCondition
    /// </summary>
    [CreateAssetMenu(fileName = "AnimatorCondition", menuName = "Skill/Condition/Animator")]
    public class AnimatorCondition : SkillCondition
    {
        [Header("Animator 参数")]
        [Tooltip("参数名称（建议使用 AnimatorParams 中的常量）")]
        public string parameterName;

        [Tooltip("参数类型（自动根据名称匹配，也可手动指定）")]
        public AnimatorControllerParameterType parameterType = AnimatorControllerParameterType.Bool;

        [Header("比较方式")]
        public ComparisonType comparison = ComparisonType.Equals;

        [Header("目标值")]
        public bool boolTarget = true;
        public int intTarget = 1;
        public float floatTarget = 1f;

        // 缓存的参数哈希（避免每帧转换）
        private int _parameterHash = -1;

        public override bool IsMet(CharacterUnit caster, ActiveSkillManager skillManager)
        {
            if (caster == null) return false;

            var animator = caster.GetComponentInChildren<Animator>();
            if (animator == null) return false;

            // 初始化哈希（如果未缓存）
            if (_parameterHash == -1)
            {
                _parameterHash = Animator.StringToHash(parameterName);
            }

            // 根据参数类型执行检测
            return parameterType switch
            {
                AnimatorControllerParameterType.Bool => EvaluateBool(animator),
                AnimatorControllerParameterType.Int => EvaluateInt(animator),
                AnimatorControllerParameterType.Float => EvaluateFloat(animator),
                AnimatorControllerParameterType.Trigger => EvaluateTrigger(animator),
                _ => false
            };
        }

        private bool EvaluateBool(Animator animator)
        {
            bool current = animator.GetBool(_parameterHash);
            return comparison switch
            {
                ComparisonType.Equals => current == boolTarget,
                ComparisonType.NotEquals => current != boolTarget,
                _ => false
            };
        }

        private bool EvaluateInt(Animator animator)
        {
            int current = animator.GetInteger(_parameterHash);
            return comparison switch
            {
                ComparisonType.Equals => current == intTarget,
                ComparisonType.NotEquals => current != intTarget,
                ComparisonType.Less => current < intTarget,
                ComparisonType.LessOrEqual => current <= intTarget,
                ComparisonType.Greater => current > intTarget,
                ComparisonType.GreaterOrEqual => current >= intTarget,
                _ => false
            };
        }

        private bool EvaluateFloat(Animator animator)
        {
            float current = animator.GetFloat(_parameterHash);
            return comparison switch
            {
                ComparisonType.Equals => Mathf.Approximately(current, floatTarget),
                ComparisonType.NotEquals => !Mathf.Approximately(current, floatTarget),
                ComparisonType.Less => current < floatTarget,
                ComparisonType.LessOrEqual => current <= floatTarget,
                ComparisonType.Greater => current > floatTarget,
                ComparisonType.GreaterOrEqual => current >= floatTarget,
                _ => false
            };
        }

        private bool EvaluateTrigger(Animator animator)
        {
            // Trigger 检测比较特殊，需要检查当前状态中是否有该触发器被触发
            // 简化实现：直接返回 GetBool（Unity 中 Trigger 也是 Bool 的一种）
            bool current = animator.GetBool(_parameterHash);
            return comparison switch
            {
                ComparisonType.Equals => current == boolTarget,
                ComparisonType.NotEquals => current != boolTarget,
                _ => false
            };
        }
    }

    /// <summary>
    /// 比较方式枚举
    /// </summary>
    public enum ComparisonType
    {
        Equals,
        NotEquals,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual
    }
}