using System.Collections.Generic;
using UnityEngine;

namespace Luo.Events
{
    /// <summary>武器Type变化事件（切换武器Type时触发） </summary>
    public struct WeaponTypeChangedEvent : IEvent
    {
        public int newIndex;
        public WeaponTypeData newData; // 新数据（方便View直接取用，避免再次查询Model）
    }

    /// <summary> 武器列表变化事件（增删武器时触发） </summary>
    public struct WeaponListChangedEvent : IEvent
    {
        public WeaponType type;
        public List<WeaponPlayerHas> weapons;
    }

    /// <summary> 具体武器被选中事件（切换武器 Item 时触发） </summary>
    public struct WeaponItemSelectedEvent : IEvent
    {
        public WeaponPlayerHas selectedWeapon;
        public int index;
    }

    // 3. 如果未来有“武器卸下”、“武器强化”等，都可以在这里扩展
    // public struct WeaponUnequippedEvent : IEvent { ... }
}