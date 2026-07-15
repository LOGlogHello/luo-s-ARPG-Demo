using Luo.Character;
using Luo.Character.Controller;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Luo.Skill
{
    // 条件1：输入检测（攻击键、技能键等）
    [CreateAssetMenu(fileName = "InputActionCondition", menuName = "Skill/Condition/InputAction")]
    [System.Serializable]
    public class InputActionCondition : SkillCondition
    {
        public InputActionReference inputAction;
        public enum InputState { Pressed, Held, Released, Active, Inactive }
        public InputState requiredState = InputState.Pressed;

        // 摇杆方向检测（仅当 action 为 Vector2 时有效）
        [Header("摇杆方向检测（仅当 action 为 Vector2 时有效）")]
        public bool checkDirection = false;
        public Vector2 targetDirection = Vector2.up;
        public float directionThreshold = 0.5f; // 0~1，越接近1要求越精确

        [Tooltip("对Vector2的长度要求")]
        public float deadZone = 0.2f;
        public override bool IsMet(CharacterUnit caster, ActiveSkillManager skillManager)
        {
            var provider = caster.GetComponent<IInputProvider>();
            if (provider == null) return false;
            var inputReader = provider.mInputReader;

            if (inputAction.action.type == InputActionType.Value &&
    inputReader.GetActionValueType(inputAction) == typeof(Vector2))
            {
                Vector2 value = inputReader.ReadValue<Vector2>(inputAction);
                float magnitude = value.magnitude;

                if (!checkDirection)
                {
                    return requiredState switch
                    {
                        InputState.Active => magnitude > deadZone,
                        InputState.Inactive => magnitude <= deadZone,
                        _ => false
                    };
                }
                else
                {
                    // 检测方向是否匹配
                    if (magnitude <= deadZone) return false;
                    float dot = Vector2.Dot(value.normalized, targetDirection.normalized);
                    bool directionMatched = dot >= directionThreshold;

                    return requiredState switch
                    {
                        InputState.Active => directionMatched,
                        InputState.Inactive => !directionMatched,
                        _ => false
                    };
                }
            }

            return requiredState switch
            {
                InputState.Pressed => inputReader.WasPressedThisFrame(inputAction),
                InputState.Held => inputReader.IsPressed(inputAction),
                InputState.Released => inputReader.WasReleasedThisFrame(inputAction),
                _ => false
            };
        }

    }
}
