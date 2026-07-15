using Luo.Character; // 引入 CharacterUnit
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

namespace Luo.Skill.TargetSelectors
{
    /// <summary>
    /// 近战武器目标选择器：检测由 Timeline 控制激活的武器碰撞体，返回所有命中的角色
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeWeaponSelector", menuName = "Skill/TargetSelector/MeleeWeapon")]
    public class MeleeWeaponSelector : TargetSelector
    {
        [Tooltip("可以击中的层（例如：Enemy）")]
        public LayerMask targetLayers;

        [Tooltip("是否检测触发器（通常武器碰撞体是触发器，应勾选）")]
        public bool queryTriggers = true;

        // 预分配缓存数组，避免 GC（高频调用时推荐）
        private Collider[] _hitResults = new Collider[20];

        public override List<TargetResult> GetTargets(CharacterUnit caster, Vector3 origin, ActiveSkillDataSO skillData)
        {
            var targetResults = new List<TargetResult>();
            var hitBuffer = new HashSet<CharacterUnit>(); //注释掉这行，使统一target可以碰撞检测多次。

            var weaponRoot = caster.GetComponentInChildren<WeaponRoot>();
            if (weaponRoot == null) return targetResults;

            var activeColliders = weaponRoot.GetActiveColliders();
            if (activeColliders.Count == 0) return targetResults;

            var trigger = queryTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

            foreach (var col in activeColliders)
            {
                int hitCount = 0;

                // 根据碰撞体类型，选择最精确的重叠检测
                if (col is CapsuleCollider capsule)
                {
                    // 获取胶囊体世界空间下的两端点（中心 + 方向 * 高度偏移）
                    var center = capsule.transform.TransformPoint(capsule.center);
                    var direction = capsule.direction; // 0= X, 1= Y, 2= Z
                    var height = capsule.height;
                    var radius = capsule.radius;

                    // 计算缩放影响
                    var scale = capsule.transform.lossyScale;
                    float scaleFactor = 1f;
                    if (direction == 0) scaleFactor = Mathf.Abs(scale.x);
                    else if (direction == 1) scaleFactor = Mathf.Abs(scale.y);
                    else if (direction == 2) scaleFactor = Mathf.Abs(scale.z);

                    // 半高（去除两端半球）
                    float halfHeight = Mathf.Max(0, height * 0.5f - radius);
                    // 方向向量
                    Vector3 dirVec = Vector3.zero;
                    if (direction == 0) dirVec = capsule.transform.right;
                    else if (direction == 1) dirVec = capsule.transform.up;
                    else if (direction == 2) dirVec = capsule.transform.forward;

                    // 计算世界空间的两个端点
                    Vector3 point1 = center - dirVec * (halfHeight * scaleFactor);
                    Vector3 point2 = center + dirVec * (halfHeight * scaleFactor);
                    float worldRadius = radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

                    // 使用 NonAlloc 版本，避免每次分配新数组
                    hitCount = Physics.OverlapCapsuleNonAlloc(point1, point2, worldRadius, _hitResults, targetLayers, trigger);
                }
                else if (col is SphereCollider sphere)
                {
                    var center = sphere.transform.TransformPoint(sphere.center);
                    var radius = sphere.radius * Mathf.Max(
                        Mathf.Abs(sphere.transform.lossyScale.x),
                        Mathf.Abs(sphere.transform.lossyScale.y),
                        Mathf.Abs(sphere.transform.lossyScale.z)
                    );
                    hitCount = Physics.OverlapSphereNonAlloc(center, radius, _hitResults, targetLayers, trigger);
                }
                else if (col is BoxCollider box)
                {
                    var center = box.transform.TransformPoint(box.center);
                    var halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
                    var rotation = box.transform.rotation;
                    hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, _hitResults, rotation, targetLayers, trigger);
                }
                else
                {
                    // 其它类型碰撞体，简单用球形近似（或跳过）
                    // 这里用 Sphere 近似
                    var center = col.transform.TransformPoint(col.bounds.center);
                    var radius = col.bounds.extents.magnitude;
                    hitCount = Physics.OverlapSphereNonAlloc(center, radius, _hitResults, targetLayers, trigger);
                }

                // 处理命中结果
                // 获取当前碰撞体的中心点
                Vector3 colCenter = col.bounds.center;

                for (int i = 0; i < hitCount; i++)
                {
                    var unit = _hitResults[i].GetComponent<CharacterUnit>();
                    if (unit != null && unit != caster)
                    {
                        // 如果该目标尚未被命中过，则记录
                        if (!hitBuffer.Contains(unit))
                        {
                            hitBuffer.Add(unit);

                            // 方向：从碰撞体中心指向目标
                            Vector3 direction = (unit.transform.position - colCenter).normalized;

                            targetResults.Add(new TargetResult
                            {
                                target = unit,
                                direction = direction
                            });
                        }
                    }
                }
            }

            return targetResults;
        }

    }
}