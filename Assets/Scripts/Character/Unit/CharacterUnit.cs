using Luo.Buff;
using Luo.Events;
using Luo.Character.Controller;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Character
{
    /// <summary>
    /// 角色单元基类（玩家、敌人、NPC、召唤物）
    /// 负责：生命/法力管理、属性系统、Buff 管理、事件派发
    /// </summary>
    [RequireComponent(typeof(CharacterController))] // 可选，但大多数角色都需要
    public class CharacterUnit : MonoBehaviour
    {
        [Header("角色配置")]
        [SerializeField] private CharacterStatsSO _stats;

        private BaseController _controller; //后续这里改成 通用基类Controller，可以是 EnemyController

        [Header("状态")]
        // ===== 运行时数据 =====
        private float _currentHealth;
        private float _currentMana;
        private float _attack;
        private float _healMultipler;
        private float _defense;
        private float _moveSpeed;
        private bool _isDead;
        private bool _isStunned;
        private bool _isInvincible;

        [Tooltip("RuntimeBuff list。同一时间，每种RuntimeBuff只能有一个挂载在CharacterUnit上")]
        private List<RuntimeBuff> _activeBuffs = new List<RuntimeBuff>();

        // ===================== 公开属性 =====================
        public float CurrentHealth => _currentHealth;
        public float CurrentMana => _currentMana;
        public float Attack => _attack;

        public float HealMultipler => _healMultipler;
        public float Defense => _defense;
        public float MoveSpeed => _moveSpeed;
        public bool IsDead => _isDead;
        public bool IsStunned => _isStunned;
        public bool IsInvincible => _isInvincible;

        // ===================== 生命周期 =====================

        private void Awake()
        {
            _controller=GetComponent<BaseController>();
        }

        protected virtual void Start()
        {
            _currentHealth = _stats.maxHealth;
            _currentMana = _stats.maxMana;
            _attack = _stats.attack;
            _healMultipler=_stats.healMultipler;
            _defense =_stats.defense;
            _moveSpeed=_stats.defense;
            _isDead=_stats.isDead;
            _isStunned=_stats.isStunned;
            _isInvincible=_stats.isInvincible;

            EventManager.Dispatch(new HealthChangedEvent
            {
                Character = this,
                CurrentHealth = _currentHealth,
                MaxHealth = _stats.maxHealth,
                Delta = 0 
            });
        }

        protected virtual void Update()
        {
            // 更新所有 Buff
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                if (_activeBuffs[i].Tick(this, Time.deltaTime))
                {
                    RemoveBuff(_activeBuffs[i]);
                }
            }
        }

        // ===================== 受伤 & 治疗 =====================
        public virtual void TakeDamage(float damage, CharacterUnit source)
        {
            // 检查条件
            if (_isDead || _isInvincible) return;

            // 计算最终伤害（防御减免）
            float finalDamage = Mathf.Max(1f, damage - _defense * 0.5f); // 简单防御公式
            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Max(0f, _currentHealth - finalDamage);

            // ===== 派发生命变化事件 =====
            EventManager.Dispatch(new HealthChangedEvent
            {
                Character = this,
                CurrentHealth = _currentHealth,
                MaxHealth = _stats.maxHealth,
                Delta = _currentHealth - oldHealth // 负值
            });


            // ===== 死亡判定 =====
            if (_currentHealth <= 0f)
            {
                Die(source);
            }
        }

        public virtual void Heal(float amount, CharacterUnit source)
        {
            if (_isDead) return;

            float oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(_stats.maxHealth, _currentHealth + amount);

            EventManager.Dispatch(new HealthChangedEvent
            {
                Character = this,
                CurrentHealth = _currentHealth,
                MaxHealth = _stats.maxHealth,
                Delta = _currentHealth - oldHealth // 正值
            });
        }

        // ===================== 法力管理 =====================
        public virtual bool ConsumeMana(float amount)
        {
            if (_currentMana < amount) return false;
            _currentMana -= amount;
            return true;
        }

        public virtual void RestoreMana(float amount)
        {
            _currentMana = Mathf.Min(_stats.maxMana, _currentMana + amount);
        }

        // ===================== 死亡 =====================
        protected virtual void Die(CharacterUnit killer)
        {
            if (_isDead) return;
            _isDead = true;
            _currentHealth = 0f;

            // ===== 派发死亡事件 =====
            EventManager.Dispatch(new DeathEvent
            {
                Character = this,
                Killer = killer
            });
        }


        // ===================== Buff 管理 =====================
        /// <summary>
        /// 添加一个 Buff，如果其isUnique成员为真，且已存在相同类型的 Buff，则替换（刷新持续时间）
        /// （注意，buff指OT Effect，不指瞬时效果）
        /// </summary>
        public void AddBuff(RuntimeBuff buff)
        {
            if (_isDead) return;

            // 如果 Buff 是唯一的，检查是否已有相同类型的 Buff
            if (buff.mIsUnique && buff.Type != BuffType.None) // 假设 BuffType 有 None 表示未指定
            {
                // 查找同类型且正在生效的 Buff
                var existing = _activeBuffs.Find(b => b.Type == buff.Type);
                if (existing != null)
                {
                    // 移除旧的（会触发 OnExpire）
                    RemoveBuff(existing);
                }
            }

            // 执行挂载逻辑（加攻/加速等）
            buff.OnApply?.Invoke(this);
            _activeBuffs.Add(buff);

            // 派发 Buff 添加事件
            EventManager.Dispatch(new BuffAddedEvent
            {
                Target = this,
                Buff = buff
            });
        }


        public bool RemoveBuff(RuntimeBuff buff)
        {
            if (_activeBuffs.Contains(buff))
            {
                buff.OnExpire?.Invoke(this);
                _activeBuffs.Remove(buff);
                return true;
            }
            return false;
        }

        public bool HasBuff<T>() where T : RuntimeBuff
        {
            foreach (var b in _activeBuffs)
                if (b is T) return true;
            return false;
        }

        // ===================== 控制状态 =====================
        public void SetStun(bool active)
        {
            _isStunned = active;
        }

        public void SetInvincible(bool active)
        {
            _isInvincible = active;
        }

        // ===================== 属性系统（扩展接口） =====================
        // 如果未来要扩展更多属性（暴击、攻速等），可以用字典
        public virtual float GetAttribute(AttributeType type)
        {
            return type switch
            {
                AttributeType.Attack => _attack,
                AttributeType.Defense => _defense,
                AttributeType.MoveSpeed => _moveSpeed,
                AttributeType.HealMultipler=> _healMultipler, 
                _ => 0f
            };
        }

        public virtual void ModifyAttribute(AttributeType type, float value, CharacterUnit source = null)
        {
            // 如果角色已死亡，不允许修改属性
            if (_isDead) return;

            switch (type)
            {
                case AttributeType.Attack:
                    _attack += value;
                    if (_attack < 0f) _attack = 0f;
                    EventManager.Dispatch(new AttackChangedEvent { Target = this, Source = source, Delta = value });
                    break;
                case AttributeType.Defense:
                    _defense += value;
                    if (_defense < 0f) _defense = 0f;
                    break;
                case AttributeType.MoveSpeed:
                    _moveSpeed += value;
                    if (_moveSpeed < 0f) _moveSpeed = 0f;
                    break;
                    // 其他属性...
            }
        }

        // ===================== 可选的调试辅助 =====================
        protected virtual void OnDrawGizmosSelected()
        {
            // 在场景中显示生命值（调试用）
             UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"HP: {_currentHealth}/{_stats.maxHealth}");
        }

        public float MaxHealth => _stats.maxHealth;
    }

    // ===================== 配套枚举 =====================
    public enum AttributeType
    {
        Attack,
        Defense,
        MoveSpeed,
        CritRate,
        CritDamage,
        HealMultipler
    }
}