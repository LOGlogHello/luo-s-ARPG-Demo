using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Luo.Events;

namespace Luo
{
    /// <summary>
    /// 挂在角色身上的武器挂载点，根据事件切换显示的武器
    /// </summary>
    [RequireComponent(typeof(ParentConstraint))]
    public class WeaponRoot : MonoBehaviour
    {
        [Header("当前持有的武器数据（运行时由事件驱动）")]
        public WeaponPlayerHas weaponData;

        private ParentConstraint parentConstraint;
        private GameObject modelInstance;
        private List<Collider> _colliders = new List<Collider>(); //modelInstance 下可能不止一个碰撞体

        [Header("是否启用武器Type、Item切换 事件监听")]
        public bool isListenWeaponChange = true;
        // 可选：在Inspector中添加开关
        public bool showGizmos = true;

        private void OnEnable()
        {
            if(isListenWeaponChange)
            {
                EventManager.AddListener<WeaponTypeChangedEvent>(OnWeaponTypeSelected);
                EventManager.AddListener<WeaponItemSelectedEvent>(OnWeaponItemSelected);
            }
            
        }

        private void OnDisable()
        {
            if (isListenWeaponChange)
            {
                EventManager.RemoveListener<WeaponTypeChangedEvent>(OnWeaponTypeSelected);
                EventManager.RemoveListener<WeaponItemSelectedEvent>(OnWeaponItemSelected);
            }
            
        }

        private void Start()
        {
            // 若初始有 weaponData（例如编辑器拖拽），则直接加载；否则等待事件驱动
            if (weaponData != null)
            {
                SetParentConstraint();
                LoadModel();
            }
        }

        // ---- 事件响应 ----
        private void OnWeaponTypeSelected(WeaponTypeChangedEvent evt)
        {
            // 当类型改变时，尝试获取该类型下的第一把武器并自动选中
            var weapons = WeaponItemModel.Instance.GetWeaponsByType(evt.newData.weaponType);
            if (weapons != null && weapons.Count > 0)
            {
                // 派发选中事件，触发 OnWeaponItemSelected 更新显示
                EventManager.Dispatch(new WeaponItemSelectedEvent
                {
                    selectedWeapon = weapons[0],
                    index = 0
                });
            }
            else
            {
                // 该类型没有武器，清空显示
                ClearWeapon();
            }
        }

        private void OnWeaponItemSelected(WeaponItemSelectedEvent evt)
        {
            if (evt.selectedWeapon == null)
            {
                ClearWeapon();
                return;
            }

            // 更新数据并刷新显示
            weaponData = evt.selectedWeapon;
            SetParentConstraint();
            LoadModel();
        }

        // ---- 清空当前武器 ----
        private void ClearWeapon()
        {
            weaponData = null;
            if (modelInstance != null)
            {
                Destroy(modelInstance);
                modelInstance = null;
            }
            if (parentConstraint != null)
            {
                parentConstraint.constraintActive = false;
            }
        }

        // ---- 约束设置（从 View 层读取骨骼绑定信息） ----
        private void SetParentConstraint()
        {
            // 如果无数据，停用约束
            if (weaponData == null || weaponData.view == null)
            {
                if (parentConstraint != null)
                    parentConstraint.constraintActive = false;
                return;
            }

            parentConstraint = GetComponent<ParentConstraint>();
            Animator animator = GetComponentInParent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"WeaponRoot 找不到父级 Animator！");
                return;
            }

            Transform targetBone = animator.GetBoneTransform(weaponData.view.targetBone);
            if (targetBone == null)
            {
                Debug.LogError($"找不到骨骼：{weaponData.view.targetBone}");
                return;
            }

            // 重置约束源
            parentConstraint.SetSources(new List<ConstraintSource>());
            parentConstraint.AddSource(new ConstraintSource
            {
                sourceTransform = targetBone,
                weight = 1f
            });
            parentConstraint.SetTranslationOffset(0, weaponData.view.localPositionOffset);
            parentConstraint.SetRotationOffset(0, weaponData.view.localRotationOffset);
            parentConstraint.constraintActive = true;
            parentConstraint.locked = true;
        }

        // ---- 加载模型（从 View 层获取预制体并实例化） ----
        private void LoadModel()
        {
            // 先销毁旧模型
            if (modelInstance != null)
            {
                Destroy(modelInstance);
                modelInstance = null;
            }

            if (weaponData == null || weaponData.view == null || weaponData.view.modelPrefab == null)
                return;

            GameObject newModel = Instantiate(weaponData.view.modelPrefab, transform);
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;
            newModel.transform.localScale = Vector3.one;
            modelInstance = newModel;

            LoadColliders();
        }


        private void LoadColliders()
        {
            // 清空旧列表
            foreach (var col in _colliders) Destroy(col);
            _colliders.Clear();

            if (modelInstance == null) return;

            // 查找模型下所有 Collider（包括子物体）
            var cols = modelInstance.GetComponentsInChildren<Collider>(true);
            foreach (var col in cols)
            {
                col.enabled = false; // 默认禁用
                _colliders.Add(col);
            }

            // 可选：根据 WeaponCombatData 中的参数设置碰撞体属性（如是否为 Trigger）
            if (weaponData?.combat != null)
            {
                foreach (var col in _colliders)
                {
                    col.isTrigger = true; 
                                                                 // 其他参数...
                }
            }
        }

        public void SetColliderActive(bool active)
        {
            foreach (var col in _colliders)
            {
                col.enabled = active;
            }
        }

        public List<Collider> GetActiveColliders()
        {
            // 只返回当前处于激活状态的碰撞体（由 Timeline 控制）
            List<Collider> activeColliders = new List<Collider>();
            foreach (var col in _colliders)
            {
                if (col != null && col.enabled)
                {
                    activeColliders.Add(col);
                }
            }
            return activeColliders;
        }



        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            if (modelInstance == null) return;

            // 获取所有Collider（包括子物体）
            var colliders = modelInstance.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if (!col.enabled) continue; // 只绘制激活的
                                            // 根据类型绘制
                Gizmos.color = Color.green; // 或者红色？
                DrawColliderGizmo(col);
            }
        }

        private void DrawColliderGizmo(Collider col)
        {
            // 颜色可以用半透明绿色
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            // 或者根据激活与否改变颜色
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.transform.TransformPoint(box.center), box.size);
                // 可选：绘制实体半透明
                // Gizmos.DrawCube(box.transform.TransformPoint(box.center), box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.transform.TransformPoint(sphere.center), sphere.radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                // 绘制胶囊体比较复杂，可以使用Gizmos.DrawWireMesh? 或自己绘制
                // 简单起见，可以画两个半圆球和圆柱？但这里用近似方法：
                // 使用Unity内置方法：UnityEditor.Handles.DrawWireCapsule? 但需要Editor命名空间。
                // 在Gizmos中没有直接绘制胶囊体的方法，我们可以用球体和圆柱组合，或者用线框画一个胶囊形状。
                // 这里采用简单做法：画一个线框球体近似？或者使用WireSphere并缩放？
                // 更精确：计算胶囊体的端点，画两个球体和一个圆柱的线框。
                // 由于时间，这里用球体近似代替，或者调用Handles.DrawWireCapsule（需要Editor）。
#if UNITY_EDITOR
                // 在Editor中可以使用Handles
                var center = capsule.transform.TransformPoint(capsule.center);
                var radius = capsule.radius;
                var height = capsule.height;
                var direction = capsule.direction;
                // 计算端点
                Vector3 dirVec = Vector3.zero;
                if (direction == 0) dirVec = capsule.transform.right;
                else if (direction == 1) dirVec = capsule.transform.up;
                else if (direction == 2) dirVec = capsule.transform.forward;
                var halfHeight = Mathf.Max(0, height * 0.5f - radius);
                var p1 = center - dirVec * halfHeight;
                var p2 = center + dirVec * halfHeight;
                // 使用Handles绘制，但Handles需要SceneView等，可以在OnDrawGizmos中使用Handles吗？可以，但需要using UnityEditor;
                // 注意：Handles在OnDrawGizmos中可用
                UnityEditor.Handles.color = Gizmos.color;
                UnityEditor.Handles.DrawWireDisc(p1, dirVec, radius);
                UnityEditor.Handles.DrawWireDisc(p2, dirVec, radius);
                // 画圆柱？Handles没有直接画圆柱线框，可以用DrawWireArc? 更简单：画两个球体和一个圆柱的轮廓太复杂，这里只画两个球体加中间连线
                Gizmos.DrawWireSphere(p1, radius);
                Gizmos.DrawWireSphere(p2, radius);
                // 画四条边连接两个球体
                // 简单起见，画四个点？
#else
            // 运行时Gizmos不工作，所以这里不会执行
#endif
            }
            // 其他类型暂时忽略
        }

        private void OnDestroy()
        {
            if (modelInstance != null)
            {
                Destroy(modelInstance);
                modelInstance = null;
            }
        }
    }
}