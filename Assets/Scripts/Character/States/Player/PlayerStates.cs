// Player/PlayerStates.cs
using Luo.Character;
using Luo.Character.Controller;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;


namespace Luo.Character.State
{

    public interface IPlayerStateContext
    {
        // 只读属性
        InputReader Input { get; }
        Locomotion Movement { get; }
        InputBuffer InputBuffer { get; }
        PlayerStateType CurrentState { get; }

        // 动作方法
        void ChangeState(PlayerStateType newState);
    }

    // 行为状态接口（与战斗状态ICombatState区分）
    public interface IPlayerState
    {
        void Enter(PlayerController player);
        void Execute(PlayerController player);
        void Exit(PlayerController player);
    }

    // 可选：如果状态有内部数据，可以显式重置
    public interface IResettable
    {
        void ResetState();
    }


    public enum PlayerStateType
    {
        None=0,
        Idle,
        Move,
        Attack,
        Skill,
        Dodge,
        Dead
    }

    public class PlayerStateInstanceManager : BaseManager<PlayerStateInstanceManager>
    {
        private Dictionary<PlayerStateType, IPlayerState> _stateCache = new();

        // 游戏启动时调用一次，注册所有状态（单例）
        public void RegisterAllStates()
        {
            RegisterState(PlayerStateType.Idle, new PlayerIdleState());
            RegisterState(PlayerStateType.Move, new PlayerMoveState());
            RegisterState(PlayerStateType.Attack, new PlayerAttackState());
            // RegisterState(PlayerStateType.Skill, new PlayerSkillState());
            // RegisterState(PlayerStateType.Dodge, new PlayerDodgeState());
            RegisterState(PlayerStateType.Dead, new PlayerDeathState());
        }

        private void RegisterState(PlayerStateType type, IPlayerState state)
        {
            _stateCache[type] = state;
        }

        public IPlayerState GetState(PlayerStateType type)
        {
            if (_stateCache.TryGetValue(type, out var state))
                return state;
            Debug.LogError($"状态 {type} 未注册，请确保在 RegisterAllStates 中添加！");
            return null;
        }
    }

    // 待机状态
    public class PlayerIdleState : IPlayerState
    {
        public void Enter(PlayerController player)
        {

        }

        public void Execute(PlayerController player)
        {
            //表现层
            player.mCharacterView.mAnimator.SetFloat(AnimatorParams.MoveX, 0f);
            player.mCharacterView.mAnimator.SetFloat(AnimatorParams.MoveY, 0f);


            // 蹲下/蹲起
            bool isCrouching = player.mCharacterView.mAnimator.GetBool(AnimatorParams.IsCrouching);
            if (player.mInputReader.IsCrouchPressed)
            {
                //表现层
                player.mCharacterView.mAnimator.SetBool(AnimatorParams.IsCrouching, !isCrouching);
                //运动层
                player.mLocomotion.SetCrouch(!isCrouching);
            }

            //如果当前处于蹲下状态，则不允许移动或跳跃
            if (isCrouching)
            {
                return;
            }

            // 移动
            if (player.mInputReader.MoveValue.sqrMagnitude > 0.01f && !isCrouching)
            {
                player.ChangeState(PlayerStateType.Move);
                return;
            }

            // 跳跃
            if (player.mInputReader.IsJumpPressed && player.mLocomotion.IsGrounded)
            {
                //表现层
                player.mCharacterView.mAnimator.SetTrigger(AnimatorParams.IdleJump);
                //运动层
                player.mLocomotion.Jump();
                return;
            }


            //如果按下Attack键，就调用ActiveSkillManager的ExecuteSkill方法
            //if (player.mInputReader.IsAttackingPressed && player.mCharacterView.mAnimator.GetBool(AnimatorParams.IsAbleToAttack))
            //{
            //    player.ChangeState(PlayerStateType.Attack);
            //    return;
            //}
            player.TryTriggerSkill();

            // 技能键 1
            //if (Input.GetKeyDown(KeyCode.Alpha1))
            //{
            //    player.ChangeState(new PlayerSkillState("Skill_1"));
            //}
        }

        public void Exit(PlayerController player) { }
    }

    // 移动状态
    public class PlayerMoveState : IPlayerState
    {
        public void Enter(PlayerController player)
        {

        }

        public void Execute(PlayerController player)
        {

            if (player.mInputReader.MoveValue.sqrMagnitude > 0.01f)
            {
                //表现层
                Debug.Log($"MoveX: {player.mInputReader.MoveValue.x}, MoveY: {player.mInputReader.MoveValue.y}");
                player.mCharacterView.mAnimator.SetFloat(AnimatorParams.MoveX, player.mInputReader.MoveValue.x);
                player.mCharacterView.mAnimator.SetFloat(AnimatorParams.MoveY, player.mInputReader.MoveValue.y);

                //运动层
                Vector3 camForward = player.mMainCamera.transform.forward;
                camForward.y = 0f; // 只取水平分量
                if (camForward.sqrMagnitude > 0.001f)
                    camForward.Normalize();
                player.mLocomotion.Move(camForward);


                //if (player.mInputReader.MoveValue.sqrMagnitude > 0.3f && player.mInputReader.MoveValue.sqrMagnitude < 0.8f)
                //{
                //    //如果按下Attack键，就调用ActiveSkillManager的ExecuteSkill方法
                //    if (player.mInputReader.IsAttackingPressed && player.mCharacterView.mAnimator.GetBool(AnimatorParams.IsAbleToAttack))
                //    {

                //        player.ChangeState(PlayerStateType.Attack);
                //        return;
                //    }
                //}


                if (player.mInputReader.IsJumpPressed && player.mLocomotion.IsGrounded && player.IsFacingforward())
                {
                    player.mLocomotion.Jump();
                    player.mCharacterView.mAnimator.SetTrigger(AnimatorParams.RunJump);
                    return;
                }

                player.TryTriggerSkill();
            }
            else
            {
                player.ChangeState(PlayerStateType.Idle);
                return;
            }

        }

        public void Exit(PlayerController player)
        {

        }
    }

    // 普攻状态
    public class PlayerAttackState : IPlayerState
    {
        private int _initialSkills = 0;
        public void Enter(PlayerController player)
        {
            //player.mActiveSkillManager.ExecuteSkill(0);
        }

        public void Execute(PlayerController player)
        {
            //如果技能执行完毕，则返回状态controller的 mActiveSkillManager 的当前技能状态机上下文的 退出ID对应的状态
            if (player.mActiveSkillManager.IsSkillFullyCompleted)
            {
                player.ChangeState(player.mActiveSkillManager.mCurrentBinding.exitStateID);
                return;
            }
        }

        public void Exit(PlayerController player)
        {

        }

        public int InitialSkills => _initialSkills;
    }

    // 技能状态
    public class PlayerSkillState : IPlayerState
    {
        private string skillID;
        private float castTime;

        public PlayerSkillState(string id) { skillID = id; }

        public void Enter(PlayerController player)
        {


            // 示例：变身技能直接添加状态
            if (skillID == "Skill_Werewolf")
            {
                //player.StateMgr.AddState(new WerewolfFormState(10f)); // 变身10秒
            }
        }

        public void Execute(PlayerController player)
        {
            castTime += Time.deltaTime;
            if (castTime > 1.0f) // 假设技能后摇1秒
            {
                player.ChangeState(PlayerStateType.Idle);
            }
        }

        public void Exit(PlayerController player) { }
    }

    public class PlayerDeathState : IPlayerState
    {
        public void Enter(PlayerController player)
        {
            //表现层
            player.mCharacterView.mAnimator.SetTrigger(AnimatorParams.Death);
        }
        public void Execute(PlayerController player)
        {
            //死亡状态下不做任何操作
        }
        public void Exit(PlayerController player) { }
    }
}
