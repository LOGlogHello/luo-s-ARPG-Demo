// Enemy/EnemyStates.cs
using Luo.Character.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Character.State
{
    #region 接口与枚举

    /// <summary>
    /// 敌人状态接口
    /// </summary>
    public interface IEnemyState
    {
        void Enter(EnemyController enemy);
        void Execute(EnemyController enemy);
        void Exit(EnemyController enemy);
    }

    /// <summary>
    /// 敌人状态类型枚举
    /// </summary>
    public enum EnemyStateType
    {
        None = 0,
        Idle = 1,      // 待机
        Patrol = 2,    // 巡逻
        Chase = 3,     // 追击
        Attack = 4,    // 攻击
        Dead = 5       // 死亡
        // Hit 已移除，由 HitReaction 组件独立处理
    }

    #endregion

    #region 状态管理器（单例）

    /// <summary>
    /// 敌人状态实例管理器（单例）
    /// </summary>
    public class EnemyStateInstanceManager : BaseManager<EnemyStateInstanceManager>
    {
        private Dictionary<EnemyStateType, IEnemyState> _stateCache = new();

        public void RegisterAllStates()
        {
            RegisterState(EnemyStateType.Idle, new EnemyIdleState());
            RegisterState(EnemyStateType.Patrol, new EnemyPatrolState());
            RegisterState(EnemyStateType.Chase, new EnemyChaseState());
            RegisterState(EnemyStateType.Attack, new EnemyAttackState());
            RegisterState(EnemyStateType.Dead, new EnemyDeadState());
        }

        private void RegisterState(EnemyStateType type, IEnemyState state)
        {
            _stateCache[type] = state;
        }

        public IEnemyState GetState(EnemyStateType type)
        {
            if (_stateCache.TryGetValue(type, out var state))
                return state;
            Debug.LogError($"状态 {type} 未注册！");
            return null;
        }
    }

    #endregion

    #region 具体状态实现

    // ============================================================
    // 1. 待机状态
    // ============================================================
    /// <summary>
    /// 待机状态：敌人站在原地发呆，等待 AI 决定下一步
    /// </summary>
    public class EnemyIdleState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            // 停止移动
            enemy.mLocomotion.Move(Vector3.zero);
            // 动画：进入 Idle
            enemy.mCharacterView.mAnimator.SetFloat(AnimatorParams.IsEnemyMoving, 0);
        }

        public void Execute(EnemyController enemy)
        {
            // 逻辑决策由 EnemyAI 负责，状态本身不做任何事
            // 等待 AI 调用 ChangeState 切换到 Patrol / Chase / Attack
        }

        public void Exit(EnemyController enemy) { }
    }

    // ============================================================
    // 2. 巡逻状态
    // ============================================================
    /// <summary>
    /// 巡逻状态：敌人在巡逻点之间移动
    /// </summary>
    public class EnemyPatrolState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            enemy.mCharacterView.mAnimator.SetFloat(AnimatorParams.IsEnemyMoving, 0.5f);
        }

        public void Execute(EnemyController enemy)
        {
            var ai = enemy.AI;
            if (ai == null || !ai.HasPatrolTarget)
                return;

            // 计算到目标的距离
            float distance = Vector3.Distance(enemy.transform.position, ai.PatrolTarget);

            // 如果已经到达目标附近，清除巡逻目标并停止移动
            if (distance < 0.5f)
            {
                enemy.mLocomotion.Move(Vector3.zero);
                enemy.ChangeState(EnemyStateType.Idle);
                ai.ClearPatrolTarget(); // 关键：清除目标，让 AI 生成新点
                return;
            }

            // 向目标移动
            Vector3 dir = (ai.PatrolTarget - enemy.transform.position).normalized;
            dir.y = 0f;
            enemy.mLocomotion.Move(dir);
        }

        public void Exit(EnemyController enemy)
        {
            enemy.mCharacterView.mAnimator.SetFloat(AnimatorParams.IsEnemyMoving, 0);
        }
    }

    // ============================================================
    // 3. 追击状态
    // ============================================================
    /// <summary>
    /// 追击状态：敌人朝玩家移动
    /// </summary>
    public class EnemyChaseState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            enemy.mCharacterView.mAnimator.SetFloat(AnimatorParams.IsEnemyMoving, 1.0f);
        }

        public void Execute(EnemyController enemy)
        {
            var player = enemy.AI?.Player;
            if (player == null)
                return;

            // 计算朝向玩家的方向
            Vector3 dir = (player.position - enemy.transform.position).normalized;
            dir.y = 0f;

            // 向玩家移动
            enemy.mLocomotion.Move(dir);

            // 尝试触发攻击技能（索引 0 为普攻）
            if (enemy.AI.isAttacking)
            {
                if (!enemy.TryTriggerSkill())
                {
                    // 如果技能无法触发，直接回到追击
                    enemy.ChangeState(EnemyStateType.Chase);
                }
            }
        }

        public void Exit(EnemyController enemy)
        {
            enemy.mCharacterView.mAnimator.SetFloat(AnimatorParams.IsEnemyMoving, 0);
        }
    }

    // ============================================================
    // 4. 攻击状态
    // ============================================================
    /// <summary>
    /// 攻击状态：敌人执行攻击技能，技能结束后自动回到追击状态
    /// </summary>
    public class EnemyAttackState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {

            
        }

        public void Execute(EnemyController enemy)
        {

        }

        public void Exit(EnemyController enemy) 
        {
            enemy.AI.isAttacking=false;
        }
    }

    // ============================================================
    // 5. 死亡状态
    // ============================================================
    /// <summary>
    /// 死亡状态：敌人死亡，播放死亡动画，禁用移动
    /// </summary>
    public class EnemyDeadState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            // 表现层：触发死亡动画
            enemy.mCharacterView.mAnimator.SetTrigger(AnimatorParams.Death);
            // 运动层：禁用移动
            enemy.mLocomotion.SetDead(true);
        }

        public void Execute(EnemyController enemy)
        {
            // 死亡状态下不做任何操作
        }

        public void Exit(EnemyController enemy) { }
    }

    #endregion
}