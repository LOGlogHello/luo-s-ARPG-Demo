// WeaponCatalog.cs
using System.Collections.Generic;
using UnityEngine;

namespace Luo
{
    [CreateAssetMenu(menuName = "GameData/WeaponCatalog")]
    public class WeaponCatalog : ScriptableObject
    {
        public List<WeaponCatalogEntry> entries = new List<WeaponCatalogEntry>();


        // 通过 ID 查找条目（O(n) 查找，武器数量不大时可接受）
        public WeaponCatalogEntry GetEntry(WeaponType type, int id)
        {
            return entries.Find(e =>(e.weaponType == type&&e.weaponID == id));
        }
    }
}