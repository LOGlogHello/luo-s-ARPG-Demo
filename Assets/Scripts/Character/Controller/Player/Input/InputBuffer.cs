using UnityEngine;

namespace Luo.Character.Controller
{
    public class InputBuffer
    {
        private float _attackBufferEndTime;  // 攻击缓冲的截止时间戳
        private float _dodgeBufferEndTime;   // 闪避缓冲的截止时间戳
        private const float BUFFER_DURATION = 0.3f;

        // 由 InputReader 调用（当检测到按键按下时）
        public void BufferAttack()
        {
            _attackBufferEndTime = Time.time + BUFFER_DURATION;
        }

        public void BufferDodge()
        {
            _dodgeBufferEndTime = Time.time + BUFFER_DURATION;
        }

        // 由状态机（如 AttackState）调用：消费缓冲
        public bool ConsumeAttack()
        {
            if (Time.time < _attackBufferEndTime)
            {
                _attackBufferEndTime = 0f; // 消费掉，防止重复触发
                return true;
            }
            return false;
        }

        public bool ConsumeDodge()
        {
            if (Time.time < _dodgeBufferEndTime)
            {
                _dodgeBufferEndTime = 0f;
                return true;
            }
            return false;
        }

        // 可选：清空缓冲（用于状态退出时防止残留）
        public void Clear()
        {
            _attackBufferEndTime = 0f;
            _dodgeBufferEndTime = 0f;
        }
    }

}

