using Luo.Character;
using Luo.Events;
using UnityEngine;

namespace Luo.Character.Behavior
{
    public enum HitDirection
    {
        None = -1,
        Front = 0,   // 前
        Back = 1,    // 后
        Left = 2,    // 左
        Right = 3    // 右
    }
    public class HitReaction : MonoBehaviour
    {
        [Header("受击参数")]
        public float hitLockDuration = 0.5f;

        private CharacterUnit _characterUnit;
        private Animator _animator;
        private bool _isLocked;
        private float _lockTimer;

        public bool IsLocked => _isLocked;

        private void Awake()
        {
            _characterUnit = GetComponent<CharacterUnit>();
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            EventManager.AddListener<DamageTakenEvent>(OnDamageTaken);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<DamageTakenEvent>(OnDamageTaken);
        }

        private void OnDamageTaken(DamageTakenEvent evt)
        {
            if (evt.Target != _characterUnit) return;
            if (_characterUnit.IsDead) return;

            // 计算受击方向
            int direction = CalculateHitDirection(evt.Source);

            // 应用受击
            ApplyHit(direction);
        }

        /// <summary>
        /// 计算受击方向（相对于受击者）
        /// </summary>
        private int CalculateHitDirection(CharacterUnit source)
        {
            if (source == null) return (int)HitDirection.Front;

            // 1. 计算从受击者指向攻击者的方向（世界空间）
            Vector3 directionToAttacker = source.transform.position - transform.position;
            directionToAttacker.y = 0f; // 忽略垂直分量

            if (directionToAttacker.sqrMagnitude < 0.001f)
                return (int)HitDirection.Front;

            // 2. 转换到受击者的本地空间
            Vector3 localDir = transform.InverseTransformDirection(directionToAttacker.normalized);

            // 3. 判断方向：比较 |Z| 和 |X| 的大小，取绝对值较大的轴
            float absZ = Mathf.Abs(localDir.z);
            float absX = Mathf.Abs(localDir.x);

            if (absZ >= absX)
            {
                // 前后方向
                return localDir.z > 0 ? (int)HitDirection.Front : (int)HitDirection.Back;
            }
            else
            {
                // 左右方向
                return localDir.x > 0 ? (int)HitDirection.Right : (int)HitDirection.Left;
            }
        }

        private void ApplyHit(int direction)
        {
            _isLocked = true;
            _lockTimer = hitLockDuration;

            // 触发受击动画
            _animator.SetTrigger(AnimatorParams.IsHit);
            _animator.SetInteger(AnimatorParams.HitDirection, direction);
            // 可选：设置 Layer 权重
            _animator.SetLayerWeight(1, 1f);

            // 可选：从动画获取硬直长度
            // AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            // _lockTimer = stateInfo.length;
        }

        private void Update()
        {
            if (_isLocked)
            {
                _lockTimer -= Time.deltaTime;
                if (_lockTimer <= 0f)
                {
                    _isLocked = false;
                    _animator.SetInteger(AnimatorParams.HitDirection, (int)HitDirection.None);
                    _animator.SetLayerWeight(1, 0f);
                }
            }
        }

        public void CancelHit()
        {
            _isLocked = false;
            _lockTimer = 0f;
            _animator.SetInteger(AnimatorParams.HitDirection, (int)HitDirection.None);
            _animator.SetLayerWeight(1, 0f);
        }
    }

}