using UnityEngine;
using UnityEngine.Playables;
namespace Luo.Skill.Timeline
{
    public class ColliderControlBehaviour : PlayableBehaviour
    {
        // 可选：是否在退出时禁用
        public bool deactivateOnExit = true;

        private WeaponRoot _weaponRoot;
        private bool _hasActivated;

        public override void OnPlayableCreate(Playable playable)
        {
            // 这里只做初始化，不获取绑定，因为绑定可能在 Playable 创建后才设置
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            // 如果已经激活过，避免重复激活（例如在循环播放时）
            if (_hasActivated) return;

            // 从 playerData 获取绑定的 WeaponRoot（由 Track 自动传入）
            // 注意：playerData 会在 ProcessFrame 中传入，但 OnBehaviourPlay 也可以通过参数获取？
            // 实际上，OnBehaviourPlay 没有 playerData 参数，我们需要从其他地方获取。
            // 正确做法：在 ProcessFrame 中缓存 WeaponRoot。
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // 在 ProcessFrame 中缓存 WeaponRoot 引用
            if (_weaponRoot == null)
            {
                _weaponRoot = playerData as WeaponRoot;
            }

            // 如果是第一次进入播放状态且 WeaponRoot 有效，激活所有碰撞体
            if (_weaponRoot != null && !_hasActivated && playable.GetTime() >= 0)
            {
                _weaponRoot.SetColliderActive(true);
                _hasActivated = true;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // 当 Clip 暂停或结束时，禁用所有碰撞体（可选）
            if (_weaponRoot != null && _hasActivated && deactivateOnExit)
            {
                _weaponRoot.SetColliderActive(false);
                _hasActivated = false;
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            // 清理引用
            _weaponRoot = null;
        }
    }
}


