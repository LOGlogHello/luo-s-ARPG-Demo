using Luo.Character;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Luo.Buff
{
    public enum BuffType
    {
        None = 0,

        /// <summary>
        /// 灼烧：持续伤害（Damage Over Time）
        /// </summary>
        Burning,

        /// <summary>
        /// 中毒：持续伤害
        /// </summary>
        Poisoning,
        // 其他类型...
    }

    /// <summary>
    /// 一个Buff的运行时实例，包含持续时间、计时器和效果逻辑。该类用于在游戏中动态管理Buff的生命周期和效果触发。
    /// 由CharacterUnit类持有和管理，确保Buff在游戏逻辑中正确应用和更新。
    /// </summary>
    public class RuntimeBuff
    {
        public float RemainingTime { get; private set; }
        public float TickTimer { get; private set; }
        public float Interval { get; private set; }

        [Tooltip("RuntimeBuff的类型")]
        public BuffType Type { get; private set; }
        private bool _isUnique = false;

        // 核心：由 BuffEffect 注入具体逻辑
        public System.Action<CharacterUnit> OnTick; // 由 BuffEffect 注入具体逻辑                                             
        public System.Action<CharacterUnit> OnApply;   // 挂载时执行（加攻/加速）
        public System.Action<CharacterUnit> OnExpire;  // 结束时执行（恢复攻速）
        public RuntimeBuff(float duration, float interval, BuffType type, bool isUnique) 
        { 
            this.RemainingTime= duration;
            this.Interval= interval;
            this.Type = type;
            this._isUnique = isUnique;
        }

        public bool Tick(CharacterUnit target, float delta)
        {
            RemainingTime -= delta;
            TickTimer += delta;
            if (TickTimer >= Interval)
            {
                OnTick?.Invoke(target);
                TickTimer = 0;
            }
            return RemainingTime <= 0;
        }

        public bool mIsUnique=> _isUnique;
    }
}


