using System.Collections.Generic;
using Luo.Events;
using UnityEngine;

namespace Luo
{
    public class WeaponItemController : MonoBehaviour
    {
        [Header("View 引用")]
        public WeaponItemUIView view;

        // 缓存当前展示的武器列表（用于点击时获取数据）
        private List<WeaponPlayerHas> currentWeaponList = new List<WeaponPlayerHas>();

        private void OnEnable()
        {
            EventManager.AddListener<WeaponListChangedEvent>(OnWeaponListChanged);
            //EventManager.AddListener<WeaponTypeChangedEvent>(OnWeaponTypeSelected);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<WeaponListChangedEvent>(OnWeaponListChanged);
            //EventManager.RemoveListener<WeaponTypeChangedEvent>(OnWeaponTypeSelected);
        }

        private void Start()
        {
            // 初始显示当前类型的武器列表（若 Model 已初始化）
            RefreshUI(WeaponItemModel.Instance.GetCurrentWeapons());
        }

        // ---- 事件响应 ----
        private void OnWeaponListChanged(WeaponListChangedEvent evt)
        {
            RefreshUI(evt.weapons);
        }

        //private void OnWeaponTypeSelected(WeaponTypeChangedEvent evt)
        //{
        //    // 类型切换时，View 会自动通过 WeaponListChangedEvent 刷新
        //    // 这里可做额外清理，比如取消高亮
        //}

        // ---- 刷新 UI ----
        private void RefreshUI(List<WeaponPlayerHas> weapons)
        {
            currentWeaponList = weapons ?? new List<WeaponPlayerHas>();

            // 1. 生成对应数量的按钮
            view.CreateWeaponItems(currentWeaponList.Count);

            // 2. 填充数据 & 绑定点击事件
            for (int i = 0; i < currentWeaponList.Count; i++)
            {
                var data = currentWeaponList[i];
                view.SetItemIcon(i, data.view.inventoryIcon);
                view.SetItemName(i, data.stats.weaponName);
                view.SetItemLevel(i, data.level);

                int index = i; // 闭包捕获
                view.SetItemClickEvent(i, () => OnWeaponItemClicked(index));
            }

            // 3. 默认高亮第一个（如果有）
            if (currentWeaponList.Count > 0)
                view.HighlightItem(0, true);
        }

        // ---- 点击 Item ----
        private void OnWeaponItemClicked(int index)
        {
            if (index < 0 || index >= currentWeaponList.Count) return;

            // 高亮当前点击的 Item
            for (int i = 0; i < currentWeaponList.Count; i++)
                view.HighlightItem(i, i == index);

            // 获取选中的武器数据
            WeaponPlayerHas selected = currentWeaponList[index];

            // 派发事件，通知其他系统（如角色换武器、详情面板更新）
            EventManager.Dispatch(new WeaponItemSelectedEvent
            {
                selectedWeapon = selected,
                index = index
            });

            Debug.Log($"选中武器：{selected.stats.weaponName}，等级：{selected.level}");
        }
    }
}