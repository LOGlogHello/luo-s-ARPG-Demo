using Luo.Character;
using Luo.Character.State;
using Luo.Events;
using Luo.Skill;
using UnityEngine;



namespace Luo.Character.Controller
{
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyController : BaseController
    {
        private EnemyAI _enemyAI;

        private IEnemyState _currentState;
        private EnemyStateType _currentStateType;
        private EnemyStateType _lastStateType =EnemyStateType.None;

        private new void Awake()
        {
            base.Awake();
            _enemyAI = GetComponent<EnemyAI>();
        }

        private void OnEnable()
        {
            EventManager.AddListener<DeathEvent>(OnDeathEvent);
            EventManager.AddListener<DamageTakenEvent>(OnDamageTaken);
            
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<DeathEvent>(OnDeathEvent);
            EventManager.RemoveListener<DamageTakenEvent>(OnDamageTaken);
            
        }


        private void OnDamageTaken(DamageTakenEvent evt)
        {
            // 只处理自己受到的伤害
            if (evt.Target != this.mCharacterUnit) return;
            // 如果已经死亡或正在死亡状态，忽略
            if (_currentStateType == EnemyStateType.Dead) return;

        }

        private void OnDeathEvent(DeathEvent evt)
        {
            if (evt.Character != _characterUnit) return;
            ChangeState(EnemyStateType.Dead);
        }

        private void Start()
        {
            EnemyStateInstanceManager.GetInstance().RegisterAllStates();
            ChangeState(EnemyStateType.Idle);
        }

        private void Update()
        {
            _currentState?.Execute(this);
        }

        public void ChangeState(EnemyStateType newType)
        {
            if (_currentStateType == newType) return;
            if (_currentStateType == EnemyStateType.Dead && newType != EnemyStateType.Dead) return;

            _lastStateType = _currentStateType;
            _currentState?.Exit(this);

            //打印状态切换信息
            Debug.Log($"EnemyStateType changed: {_currentStateType} -> {newType}");

            _currentState = EnemyStateInstanceManager.GetInstance().GetState(newType);
            _currentStateType = newType;
            _currentState.Enter(this);
        }


        // 实现 BaseController 抽象方法
        public override bool TryTriggerSkill()
        {
            if (_activeSkillManager != null && _activeSkillManager.TryGetFirstMatchingSkill(out var skill, out var binding))
            {
                ChangeState(binding.targetStateID);
                _activeSkillManager.ExecuteSkill(skill, binding);
                return true;
            }
            return false;
        }

        public override void ChangeState(int stateID) => ChangeState((EnemyStateType)stateID);
        public override int GetCurrentStateID() => (int)_currentStateType;

        public override ControllerType Type => ControllerType.Enemy;

        public EnemyStateType mCurrentStateType => _currentStateType;
        public EnemyStateType mLastStateType => _lastStateType;

        public EnemyAI AI => _enemyAI;
    }

}


