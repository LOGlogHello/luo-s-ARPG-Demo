// Enemy/EnemyAI.cs
using Luo.Character.Controller;
using Luo.Character.State;
using UnityEngine;

namespace Luo.Character.Controller
{
    /// <summary>
    /// 敌人 AI 控制器（大脑）
    /// 职责：感知玩家、做出决策、切换状态
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("感知参数")]
        [Tooltip("检测玩家的最大距离")]
        public float detectionRange = 10f;

        [Tooltip("攻击距离（进入此范围会攻击）")]
        public float attackRange = 2f;

        [Tooltip("丢失玩家的距离（必须大于 detectionRange，避免在边缘反复横跳）")]
        public float lostRange = 15f;

        [Tooltip("玩家所在的 Layer（用于检测）")]
        public LayerMask playerLayer;

        [Header("巡逻参数")]
        [Tooltip("巡逻半径（以出生点为中心）")]
        public float patrolRadius = 5f;

        [Tooltip("到达巡逻点后的等待时间（秒）")]
        public float patrolWaitTime = 2f;

        [Header("攻击参数")]
        [Tooltip("攻击冷却时间（秒）")]
        public float attackCooldown = 1.5f;

        // ==================== 运行时状态 ====================
        private EnemyController _enemy;          // 敌人控制器引用
        private Transform _player;               // 玩家 Transform
        private Vector3 _spawnPosition;          // 出生位置（用于巡逻范围限制）

        // 巡逻相关
        private Vector3 _patrolTarget;           // 当前巡逻目标点
        private bool _hasPatrolTarget;           // 是否有巡逻目标
        private float _patrolTimer;              // 巡逻等待计时器

        // 攻击相关
        private float _attackTimer;              // 攻击冷却计时器
        public bool isAttacking = false;

        // ==================== 公共属性 ====================
        public Transform Player => _player;
        public Vector3 PatrolTarget => _patrolTarget;
        public bool HasPatrolTarget => _hasPatrolTarget;

        // ==================== 生命周期 ====================
        private void Awake()
        {
            _enemy = GetComponent<EnemyController>();
            _spawnPosition = transform.position;
        }

        private void Start()
        {
            // 尝试通过 Tag 查找玩家
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;
        }

        private void Update()
        {
            // 死亡时不执行 AI
            if (_enemy.mCurrentStateType == EnemyStateType.Dead)
                return;

            // 1. 更新计时器
            UpdateTimers();

            // 2. 检测玩家
            bool playerDetected = DetectPlayer();
            float distanceToPlayer = _player != null
                ? Vector3.Distance(transform.position, _player.position)
                : float.MaxValue;

            // 3. 根据当前状态执行决策
            switch (_enemy.mCurrentStateType)
            {
                case EnemyStateType.Idle:
                    OnIdle(playerDetected, distanceToPlayer);
                    break;
                case EnemyStateType.Patrol:
                    OnPatrol(playerDetected, distanceToPlayer);
                    break;
                case EnemyStateType.Chase:
                    OnChase(playerDetected, distanceToPlayer);
                    break;
                case EnemyStateType.Attack:
                    OnAttack(playerDetected, distanceToPlayer);
                    break;
                    // Hit 由 HitReaction 处理，AI 不干预
            }
        }

        // ==================== 计时器更新 ====================
        private void UpdateTimers()
        {
            _attackTimer -= Time.deltaTime;
        }

        // ==================== 玩家检测 ====================
        private bool DetectPlayer()
        {
            if (_player == null)
                return false;

            float dist = Vector3.Distance(transform.position, _player.position);

            // 如果在检测范围内，视为检测到
            if (dist <= detectionRange)
                return true;

            // 如果在丢失范围内，仍然视为检测到（防止在边缘反复丢失）
            if (dist <= lostRange)
                return true;

            return false;
        }

        // ==================== 决策方法 ====================

        /// <summary>
        /// 待机状态下的决策
        /// </summary>
        private void OnIdle(bool playerDetected, float distanceToPlayer)
        {

            if (playerDetected)
            {
                if(distanceToPlayer<=attackRange)
                {
                    _enemy.ChangeState(EnemyStateType.Attack);
                }
                else if(distanceToPlayer <= detectionRange)
                {
                    // 检测到玩家 → 追击
                    _enemy.ChangeState(EnemyStateType.Chase);
                    return;
                }
            }

            // 没检测到玩家 → 进入巡逻
            // 如果已经等了足够久，开始巡逻
            _patrolTimer -= Time.deltaTime;
            if (_patrolTimer <= 0f)
            {
                _enemy.ChangeState(EnemyStateType.Patrol);
                _patrolTimer = patrolWaitTime;
            }
        }

        /// <summary>
        /// 巡逻状态下的决策
        /// </summary>
        private void OnPatrol(bool playerDetected, float distanceToPlayer)
        {
            if (playerDetected && distanceToPlayer <= detectionRange)
            {
                // 检测到玩家 → 切换到追击
                _enemy.ChangeState(EnemyStateType.Chase);
                return;
            }

            // 如果当前没有巡逻目标，生成一个
            if (!_hasPatrolTarget)
            {
                GeneratePatrolTarget();
            }
        }

        /// <summary>
        /// 追击状态下的决策
        /// </summary>
        private void OnChase(bool playerDetected, float distanceToPlayer)
        {
            if (!playerDetected)
            {
                // 丢失玩家 → 回到待机
                _enemy.ChangeState(EnemyStateType.Idle);
                // 重置巡逻等待计时器（让敌人不会立刻又去巡逻）
                _patrolTimer = patrolWaitTime;
                return;
            }
            if (distanceToPlayer <= attackRange && _attackTimer <= 0f)
            {
                isAttacking = true;
                _attackTimer = attackCooldown;
            }

        }

        /// <summary>
        /// 攻击状态下的决策
        /// </summary>
        private void OnAttack(bool playerDetected, float distanceToPlayer)
        {
            // 攻击状态由 AttackState 内部检测技能完成
            // AI 只负责：如果玩家跑出攻击范围，且攻击已冷却，再追上去
            if (playerDetected && distanceToPlayer > attackRange * 1.2f)
            {
                // 玩家跑远了 → 切回追击
                // 注意：AttackState 的 Execute 会在技能完成后自动切回 Chase
                // 这里是兜底：如果玩家跑远且技能已经结束
                if (!_enemy.mActiveSkillManager.mIsExecuting)
                {
                    _enemy.ChangeState(EnemyStateType.Chase);
                }
            }
        }

        // ==================== 巡逻目标生成 ====================
        private void GeneratePatrolTarget()
        {
            // 在巡逻半径内生成一个随机点
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 target = _spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // 可选：使用 NavMesh 采样确保目标在可行走区域
            // 如果未使用 NavMesh，直接接受随机点
            _patrolTarget = target;
            _hasPatrolTarget = true;
        }

        // ==================== 公共接口（供 PatrolState 调用） ====================
        /// <summary>
        /// 到达巡逻目标后调用，清除当前目标
        /// </summary>
        public void ClearPatrolTarget()
        {
            _hasPatrolTarget = false;
            _patrolTimer = patrolWaitTime;
        }
    }
}