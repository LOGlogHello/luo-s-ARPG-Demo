using System.Collections.Generic;
using Luo.Events;
using UnityEngine;

namespace Luo
{
    public class WeaponItemModel : MonoBehaviour
    {
        public static WeaponItemModel Instance { get; private set; }

        // ===== 核心数据：按武器类型分组的玩家武器库 =====
        private Dictionary<WeaponType, List<WeaponPlayerHas>> weaponTypeDict = new Dictionary<WeaponType, List<WeaponPlayerHas>>();

        // 当前选中的武器类型
        private WeaponType currentType = WeaponType.None;

        // ---- 初始化 ----
        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            // 初始化测试数据（实际项目中从存档或配置加载）
            
        }

        private void OnEnable()
        {
            // 监听类型切换事件，更新当前类型
            EventManager.AddListener<WeaponTypeChangedEvent>(OnWeaponTypeSelected);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<WeaponTypeChangedEvent>(OnWeaponTypeSelected);
        }

        // ---- 事件响应 ----
        private void OnWeaponTypeSelected(WeaponTypeChangedEvent evt)
        {
            if(currentType== evt.newData.weaponType)
                return; // 如果类型没有变化，则不处理
            currentType = evt.newData.weaponType;
            // 类型切换后，自动派发列表刷新事件
            DispatchWeaponListChangedEvent();
        }

        // ---- 对外接口 ----
        /// <summary> 获取某一类型的所有武器 </summary>
        public List<WeaponPlayerHas> GetWeaponsByType(WeaponType type)
        {
            if (weaponTypeDict.TryGetValue(type, out var list))
                return list;
            return new List<WeaponPlayerHas>(); // 返回空列表，避免 null
        }

        /// <summary> 获取当前选中类型的武器列表 </summary>
        public List<WeaponPlayerHas> GetCurrentWeapons()
        {
            return GetWeaponsByType(currentType);
        }

        /// <summary> 增加一把武器 </summary>
        public void AddWeapon(WeaponType type, WeaponPlayerHas weapon)
        {
            if (!weaponTypeDict.ContainsKey(type))
                weaponTypeDict[type] = new List<WeaponPlayerHas>();

            weaponTypeDict[type].Add(weapon);
            DispatchWeaponListChangedEvent(); // 通知 UI 刷新
        }

        /// <summary> 移除一把武器（按索引） </summary>
        public void RemoveWeapon(WeaponType type, int index)
        {
            if (weaponTypeDict.TryGetValue(type, out var list))
            {
                if (index >= 0 && index < list.Count)
                {
                    list.RemoveAt(index);
                    DispatchWeaponListChangedEvent();
                }
            }
        }

        /// <summary> 移除一把武器（按引用） </summary>
        public void RemoveWeapon(WeaponType type, WeaponPlayerHas weapon)
        {
            if (weaponTypeDict.TryGetValue(type, out var list))
            {
                if (list.Remove(weapon))
                    DispatchWeaponListChangedEvent();
            }
        }

        // ---- 新增：获取所有武器的扁平列表（用于存档） ----
        public List<WeaponPlayerHas> GetAllPlayerWeapons()
        {
            List<WeaponPlayerHas> all = new List<WeaponPlayerHas>();
            foreach (var kvp in weaponTypeDict)
            {
                all.AddRange(kvp.Value);
            }
            return all;
        }

        // ---- 新增：清空所有武器（用于加载存档前） ----
        public void ClearAllWeapons()
        {
            weaponTypeDict.Clear();
            // 可以保留空类型列表，以免 null 引用
            foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
            {
                if (!weaponTypeDict.ContainsKey(type))
                    weaponTypeDict[type] = new List<WeaponPlayerHas>();
            }
        }

        // ---- 新增：获取当前选中的类型 ----
        public WeaponType GetCurrentType()
        {
            return currentType;
        }

        // ---- 私有方法 ----
        private void DispatchWeaponListChangedEvent()
        {
            // 派发事件，通知所有监听者（如 WeaponItemController）
            EventManager.Dispatch(new WeaponListChangedEvent
            {
                type = currentType,
                weapons = GetCurrentWeapons()
            });
        }
        
    }
}