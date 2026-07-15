using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Luo.Character.Controller
{
    public interface IInputProvider
    {
        // 定义属性
        public InputReader mInputReader { get; }
    }

    public class InputReader : MonoBehaviour
    {
        private GameControl _controls; // 你的 New Input System 生成的类
        private InputBuffer _inputBuffer;         // 负责存历史信号（在Awake中 new 出来）

        void Awake()
        {
            _controls = new GameControl();
            _inputBuffer = new InputBuffer();
        }

        private void Update()
        {
            // 处理攻击输入
            if (IsAttackingPressed)
            {
                _inputBuffer.BufferAttack();
            }
        }

        //通过约束MoveX\MoveY，来限制角色只能十字移动
        public Vector2 MoveValue
        {
            get
            {
                // 应用死区（可选）
                float deadZone = 0.2f;
                Vector2 input = RawMoveValue;
                if (input.sqrMagnitude < deadZone * deadZone)
                    return Vector2.zero;

                // 取绝对值较大的轴，另一个置零
                float absX = Mathf.Abs(input.x);
                float absY = Mathf.Abs(input.y);

                if (absX >= absY)
                    return new Vector2(input.x, 0f);
                else
                    return new Vector2(0f, input.y);
            }
        }

        public bool WasPressedThisFrame(InputActionReference actionRef)
        {
            if(actionRef.action.id== _controls.Player.Attack.id)
            {
                return _controls.Player.Attack.WasPressedThisFrame()
                    && !(Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);
            }
            return actionRef.action.WasPressedThisFrame();
        }

        public bool IsPressed(InputActionReference actionRef)
        {
            return actionRef.action.IsPressed();
        }

        public bool WasReleasedThisFrame(InputActionReference actionRef)
        {
            return actionRef.action.WasReleasedThisFrame();
        }

        public T ReadValue<T>(InputActionReference actionRef) where T : struct
        {
            return actionRef.action.ReadValue<T>();
        }

        public Type GetActionValueType(InputActionReference actionRef)
        {
            if (actionRef == null || actionRef.action == null || actionRef.action.controls.Count == 0)
                return null;
            return actionRef.action.controls[0].valueType;
        }

        // 如果你仍然需要原始输入（比如用于UI或调试），可以单独暴露
        public Vector2 RawMoveValue => _controls.Player.Move.ReadValue<Vector2>();

        /// <summary>
        /// 按x执行普通攻击
        /// 手柄左肩键按住时，按x不执行普通攻击。
        /// </summary>
        public bool IsAttackingPressed =>
            _controls.Player.Attack.WasPressedThisFrame()
        && !(Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);

        public bool IsJumpPressed => _controls.Player.Jump.WasPressedThisFrame();
        public bool IsCrouchPressed => _controls.Player.Crouch.WasPressedThisFrame();

        /// <summary>
        /// 按手柄左肩键+X键，执行重击
        /// </summary>
        public bool IsHeavyAttackPressed => _controls.Player.HeavyAttack.WasPressedThisFrame();

        public InputBuffer mInputBuffer => _inputBuffer;

        //public bool IsDodgePressed => controls.Player.Dodge.WasPressedThisFrame();

        //给controls的某个action绑定一个回调
        public void BindAction(string actionName, System.Action callback)
        {
            var action = _controls.FindAction(actionName);
            if (action != null)
            {
                action.performed += ctx => callback?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Action '{actionName}' not found in GameControl.");
            }
        }


        void OnEnable() => _controls.Enable();
        void OnDisable() => _controls.Disable();

        public GameControl mControls=> _controls;
    }
}

