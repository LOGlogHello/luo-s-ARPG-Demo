using UnityEngine;

namespace Luo.Character
{
    /// <summary>
    /// 角色移动组件（支持 Root Motion、重力、跳跃、蹲起）
    /// 水平位移由动画 Root Motion 提供，垂直位移由物理控制
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Locomotion : MonoBehaviour
    {
        [Header("组件引用")]
        protected CharacterController characterController;

        [Header("移动参数")]
        [Tooltip("跳起的最大高度（米）")]
        public float jumpHeight = 1.0f;
        [Tooltip("重力加速度（负值）")]
        public float gravity = Physics.gravity.y;
        [Tooltip("旋转速度（度/秒），仅用于玩家平滑转向")]
        public float rotateSpeed = 720f;

        [Header("蹲起参数")]
        public float standHeight = 2f;
        public float crouchHeight = 1f;
        [Tooltip("蹲起过渡速度（越大越快）")]
        public float crouchTransitionSpeed = 10f;

        [Tooltip("是否使用rootmotion管理移动")]
        public bool useRootMotion = true;

        private Vector3 direction; // 当前移动方向（水平归一化）

        // 物理速度（仅用于垂直）
        private Vector3 velocity;
        private bool isGrounded =true;

        // 由动画系统传入的 Root Motion 水平位移（每帧累加）

        private Vector3 rootMotionDelta;

        // 当前蹲下状态
        private bool isCrouching;
        private bool isDead=false;

        protected virtual void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        /// <summary>
        /// 外部调用：设置本帧的 Root Motion 位移增量（由 Animator 的 OnAnimatorMove 提供）
        /// </summary>
        public void SetRootMotionDelta(Vector3 delta)
        {
            useRootMotion = true;
            rootMotionDelta += delta; // 累加，以防多次调用
        }

        /// <summary>
        /// 主要的移动接口，由输入或 AI 每帧调用
        /// </summary>
        /// <param name="direction">朝向方向（水平归一化），用于转向</param>
        public void Move(Vector3 direction)
        {
            this.direction=direction.normalized;
        }

        public void SetCrouch(bool active)
        {
            if (isCrouching == active) return;
            isCrouching = active;

            float targetHeight = isCrouching ? crouchHeight : standHeight;
            float heightDelta = targetHeight - characterController.height;
            characterController.height = targetHeight;

            // 脚底固定：中心点下移一半的高度变化
            Vector3 center = characterController.center;
            center.y += heightDelta * 0.5f;
            characterController.center = center;
        }

        public void SetDead(bool dead)
        {
            if(dead==isDead) return;

            if (dead)
            {
                // 1. 禁用物理移动（不再响应 Move）
                characterController.enabled = false;
                // 2. 可选：禁用碰撞响应（但保留碰撞体作为障碍物）
                // characterController.detectCollisions = false; // 如果需要穿透
                // 3. 重置速度，防止残留
                velocity = Vector3.zero;
                rootMotionDelta = Vector3.zero;
                // 4. 转向停止
                direction = Vector3.zero;
            }
            else
            {
                // 复活
                characterController.enabled = true;
                // 恢复位置、旋转等
            }
            isDead= dead;
        }

        // ===== 跳跃 =====
        // ===== 跳跃动作（由状态机调用） =====
        public void Jump()
        {
            if (!IsGrounded) return;
            // 只给速度赋值，不调用 Move！
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // ===== 核心：每帧只调用一次 =====
        private void FixedUpdate() // 或 Update（推荐 FixedUpdate 用于物理）
        {
            if (isDead)
                return;

            // 1. 地面检测（在移动前更新状态）
            isGrounded = characterController.isGrounded;

            // 2. 黏地处理（落地时重置速度，防止微弹跳）
            if (IsGrounded && velocity.y < 0f)
            {
                velocity.y = -2f; // 轻微下拉，让角色牢牢贴地
            }

            
            // 3. 施加重力（每帧都在加速下落）
            velocity.y += gravity * Time.deltaTime;

            // 4. 合成最终位移向量
            Vector3 finalMove = Vector3.zero;


            if (useRootMotion)
            {

                // 水平位移 = Root Motion 增量（由动画提供）
                finalMove.x = rootMotionDelta.x;
                finalMove.z = rootMotionDelta.z;

                // 清空已使用的 Root Motion 增量（每帧只使用一次）
                rootMotionDelta = Vector3.zero;
            }
            else
            {
                // 如果不使用 Root Motion，可使用速度模式（此处作为备用，但当前设计基于 Root Motion）
                // 我们不实现这个分支，但保留占位
                Debug.LogWarning("useRootMotion = false 未实现，请启用 Root Motion 或实现速度移动");
            }

            finalMove.y = velocity.y * Time.deltaTime;

            // 6. 转向（使用传入的方向向量）
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }

            // 应用移动（CharacterController.Move 处理碰撞）
            characterController.Move(finalMove);

            
        }

        /// <summary>
        /// 蹲下碰撞体平滑过渡，并调整中心点保持脚底位置不变
        /// </summary>
        private void UpdateCrouch()
        {
            float targetHeight = isCrouching ? crouchHeight : standHeight;
            float currentHeight = characterController.height;

            // 若高度未变，跳过
            if (Mathf.Approximately(currentHeight, targetHeight))
                return;

            // 插值高度
            float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            // 防止过冲
            newHeight = Mathf.Clamp(newHeight, Mathf.Min(crouchHeight, standHeight), Mathf.Max(crouchHeight, standHeight));

            // 高度变化量
            float heightDelta = newHeight - currentHeight;

            // 更新控制器高度
            characterController.height = newHeight;

            // 调整中心点：保持脚底位置不变（中心点下移一半的高度变化量）
            Vector3 center = characterController.center;
            center.y += heightDelta * 0.5f;
            characterController.center = center;
        }

        /// <summary>
        /// 公共属性：供动画状态机或外部查询
        /// </summary>
        public bool IsGrounded => isGrounded;
        public bool IsCrouching => isCrouching;
        public Vector3 Velocity => characterController.velocity;
    }
}
   