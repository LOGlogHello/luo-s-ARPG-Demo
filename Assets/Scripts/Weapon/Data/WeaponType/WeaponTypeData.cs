using UnityEngine;


namespace Luo
{
    [CreateAssetMenu(fileName = "WeaponType", menuName = "Scriptable Objects/WeaponType")]
    public class WeaponTypeData : ScriptableObject
    {
        public WeaponType weaponType;
        public Sprite inventoryIcon;

    }
}


