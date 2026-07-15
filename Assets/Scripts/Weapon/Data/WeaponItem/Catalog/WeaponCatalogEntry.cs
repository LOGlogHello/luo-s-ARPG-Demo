// WeaponCatalogEntry.cs
using System;
using UnityEngine;

namespace Luo
{
    [Serializable]
    public class WeaponCatalogEntry
    {
        public WeaponType weaponType;          // 武器类型（枚举）
        public string weaponName;
        public int weaponID;                // 武器ID（如 "GreatSword1"）
        public string statPath;                // Resources 路径，不含扩展名
        public string combatPath;
        public string viewPath;
    }
}