using UnityEngine;

namespace Luo
{
    [CreateAssetMenu(menuName = "GameData/View/WeaponView")]
    public class WeaponViewData : ScriptableObject
    {
        //武器模型存放路径
        public GameObject modelPrefab;      // 3D模型预制体（挂载在子物体）
                                            //public Material[] weaponMaterials;  // 材质球数组
                                            //public AudioClip swingSound;        // 挥舞音效
                                            //public GameObject hitVFXPrefab;     // 命中特效

        // 骨骼绑定数据
        public HumanBodyBones targetBone;   // 绑定到哪块骨骼（右手、左手、背部）
        public Vector3 localPositionOffset; // 相对于骨骼的局部偏移（如：大剑稍微靠后，匕首靠前）
        public Vector3 localRotationOffset; // 相对骨骼的局部旋转（如：弓箭需要旋转90度握持）

        public Sprite inventoryIcon;        // 背包图标
                                            // 注意：这里没有任何 float damage 或 float durability！
    }
}