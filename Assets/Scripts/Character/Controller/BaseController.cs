using Luo.Character;
using Luo.Skill;
using UnityEngine;

namespace Luo.Character.Controller
{
    //状态机类型枚举
    public enum ControllerType
    {
        Player,
        Enemy,
        NPC
    }

    [RequireComponent(typeof(Luo.Character.Locomotion))]
    [RequireComponent(typeof(Luo.Character.CharacterView))]
    [RequireComponent(typeof(CharacterUnit))]
    public abstract class BaseController : MonoBehaviour
    {
        protected Luo.Character.Locomotion _locomotion;
        protected Luo.Character.CharacterView _characterView; // 角色表现层
        protected ActiveSkillManager _activeSkillManager;
        protected CharacterUnit _characterUnit;


        protected void Awake()
        {
            _locomotion = GetComponent<Luo.Character.Locomotion>();
            _characterView = GetComponent<Luo.Character.CharacterView>();
            _activeSkillManager = GetComponent<ActiveSkillManager>();
            _characterUnit = GetComponent<CharacterUnit>();
        }

        // 子类必须提供自己的控制器类型
        public abstract ControllerType Type { get; }

        // 状态切换接口（子类实现）
        public abstract void ChangeState(int stateID);
        public abstract int GetCurrentStateID();

        public abstract bool TryTriggerSkill();

        public Luo.Character.Locomotion mLocomotion => _locomotion;
        public Luo.Character.CharacterView mCharacterView => _characterView;

        public ActiveSkillManager mActiveSkillManager => _activeSkillManager;

        public CharacterUnit mCharacterUnit => _characterUnit;
    }

}

