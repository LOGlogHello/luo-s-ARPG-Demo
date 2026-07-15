using Luo;
using Luo.Events; // 引入事件命名空间
using System.Collections.Generic;
using UnityEngine;

public class WeaponTypeModel : MonoBehaviour
{
    public static WeaponTypeModel Instance { get; private set; }

    [SerializeField] private List<WeaponTypeData> weaponTypeList = new List<WeaponTypeData>();

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ---- 数据访问接口 ----
    public int GetCurrentIndex() => currentIndex;
    public int GetWeaponCount() => weaponTypeList.Count;
    public WeaponTypeData GetWeaponData(int index) => weaponTypeList[index];
    public WeaponTypeData GetCurrentWeaponData() => GetWeaponData(currentIndex);

    // ---- 数据修改接口（唯一变化点：发布事件） ----
    public void SetCurrentIndex(int newIndex)
    {
        int clampedIndex = Mathf.Clamp(newIndex, 0, weaponTypeList.Count - 1);

        // 只有索引真正改变时才触发事件（防止死循环）
        if (currentIndex != clampedIndex)
        {
            currentIndex = clampedIndex;

            // ===== 核心：发布事件，通知所有监听者 =====
            EventManager.Dispatch(new WeaponTypeChangedEvent
            {
                newIndex = currentIndex,
                newData = GetCurrentWeaponData()
            });
        }
    }
}