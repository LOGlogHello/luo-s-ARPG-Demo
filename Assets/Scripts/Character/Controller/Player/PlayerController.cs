using Luo.Character;
using Luo.Character.State;
using Luo.Events;
using UnityEngine;



namespace Luo.Character.Controller
{
    [RequireComponent(typeof(InputReader))]
    public class PlayerController : BaseController, IInputProvider
    {
        private InputReader _inputReader;          // Player输入层
        private Camera _mainCamera;

        private IPlayerState currentPlayerState;

        private PlayerStateType currentPlayerStateType = PlayerStateType.None;

        private WeaponType _currentWeaponType;
        private new void Awake()
        {
            base.Awake();
            _inputReader = GetComponent<InputReader>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _mainCamera = Camera.main;
            

            PlayerStateInstanceManager.GetInstance().RegisterAllStates();

            ChangeState(PlayerStateType.Idle);
        }

        // Update is called once per frame
        void Update()
        {
            currentPlayerState?.Execute(this);
        }

        private void OnAnimatorMove()
        {
            _locomotion.SetRootMotionDelta(_characterView.mAnimator.deltaPosition);
        }

        public override bool TryTriggerSkill()
        {
            if (_activeSkillManager.TryGetFirstMatchingSkill(out var skill, out var binding))
            {
                ChangeState(binding.targetStateID);
                _activeSkillManager.ExecuteSkill(skill, binding);
                return true;
            }
            return false;
        }

        public void ChangeState(PlayerStateType type)
        {

            // 1. 相同状态不重复切换
            if (currentPlayerStateType == type)
                return;

            // 2. 死亡状态不可被覆盖
            if (currentPlayerStateType == PlayerStateType.Dead && type != PlayerStateType.Dead) return;

            // 3. 当前状态退出
            if (currentPlayerStateType != PlayerStateType.None)
                currentPlayerState?.Exit(this);

            // 4. 获取新状态
            IPlayerState newState = PlayerStateInstanceManager.GetInstance().GetState(type);

            Debug.Log($"{currentPlayerState} -> {newState}");
            // 5. 切换
            currentPlayerState = newState;
            currentPlayerStateType = type;

            // 6. 进入新状态
            currentPlayerState.Enter(this);
        }

        private void OnEnable()
        {
            // 订阅武器类型选中事件
            EventManager.AddListener<Luo.Events.WeaponTypeChangedEvent>(OnWeaponTypeSelected);
            EventManager.AddListener<DeathEvent>(OnDeathEvent);
        }

        private void OnDisable()
        {
            // 必须移除监听
            EventManager.RemoveListener<Luo.Events.WeaponTypeChangedEvent>(OnWeaponTypeSelected);
            EventManager.RemoveListener<DeathEvent>(OnDeathEvent);
        }

        private void OnDeathEvent(DeathEvent evt)
        {
            if (evt.Character != _characterUnit) return;
            ChangeState(PlayerStateType.Dead);
        }

        // ---- 事件响应 ----
        private void OnWeaponTypeSelected(Luo.Events.WeaponTypeChangedEvent evt)
        {
            if (_currentWeaponType == evt.newData.weaponType)
            {
                //Debug.Log($"角色已装备武器：{evt.newData.weaponType}，无需切换");
                return;
            }

            // 驱动 Animator
            if (_characterView.mAnimator.GetBool("IsAbleToChangeWeapon"))
            {
                _currentWeaponType = evt.newData.weaponType;
                _characterView.mAnimator.SetInteger("WeaponType", (int)_currentWeaponType);
                _characterView.mAnimator.SetTrigger("RefreshWeapon");
                _characterView.mAnimator.SetBool("IsAbleToChangeWeapon", false);
                Debug.Log($"角色切换武器到：{evt.newData.weaponType}");
            }

            // 后续通过事件中心通知其他模块（UI、音效等）
            // EventCenter.Trigger(new OnWeaponEquipArgs { NewWeaponType = _currentWeaponType });

            // 3. 其他逻辑（比如更换角色身上的武器模型）
            // SwitchWeaponModel(evt.selectedType);
        }

        /// <summary>
        /// 只要moveY>0 且moveX在[-0.1,0.1]之间 就认为是向前
        /// </summary>
        /// <returns></returns>
        public bool IsFacingforward()
        {
            //只要moveY>0 且moveX在[-0.1,0.1]之间 就认为是向前
            if (mInputReader.MoveValue.y > 0 && Mathf.Abs(mInputReader.MoveValue.x) < 0.1f)
            {
                return true;
            }
            return false;
        }

        // 状态 ID → 枚举 转换
        private PlayerStateType GetStateFromID(int id)
        {
            return (PlayerStateType)id; // 直接强转，前提是枚举值与 ID 一致
        }

        // 枚举 → 状态 ID（用于通知 ActiveSkillManager）
        public int GetStateID(PlayerStateType state)
        {
            return (int)state;
        }

        public override void ChangeState(int stateID)
        {
            var newState = GetStateFromID(stateID);
            // 执行 Player 的状态切换逻辑...
            ChangeState(newState);
        }

        public override int GetCurrentStateID()
        {
            return (int)currentPlayerStateType;
        }

        public InputReader mInputReader => _inputReader;
        public PlayerStateType mCurrentPlayerStateType => currentPlayerStateType;
        public Camera mMainCamera => _mainCamera;

        public override ControllerType Type => ControllerType.Player;
    }

}


