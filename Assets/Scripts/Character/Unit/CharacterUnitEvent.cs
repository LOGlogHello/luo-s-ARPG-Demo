// ===== Events/CharacterEvents.cs =====
using Luo.Buff;
using Luo.Character;
using UnityEngine;


namespace Luo.Events
{
    public struct HealthChangedEvent : IEvent
    {
        public CharacterUnit Character;
        public float CurrentHealth;
        public float MaxHealth;
        public float Delta; // 正=治疗，负=受伤
    }

    public struct DamageTakenEvent : IEvent
    {
        public CharacterUnit Target;
        public CharacterUnit Source;
        public float Damage;
        public Vector3 hitDirection;      // 新增：受击方向（世界空间）
    }

    public struct DeathEvent : IEvent
    {
        public CharacterUnit Character;
        public CharacterUnit Killer;
    }

    public struct BuffAddedEvent : IEvent
    {
        public CharacterUnit Target;
        public RuntimeBuff Buff;
    }

    public struct AttackChangedEvent : IEvent
    {
        public CharacterUnit Target;
        public CharacterUnit Source;
        public float Delta;
    }

}