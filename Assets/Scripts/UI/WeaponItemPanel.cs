using Luo;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class WeaponItemPanel : BasePanel
{
    [Header("References")]
    public GameObject itemSlotPrefab;   // 拖入 ItemSlot 预制体
    private GridLayoutGroup gridLayout;  // 自动引用，也可手动拖

    [Header("Test Data")]
    public List<WeaponRoot> currentWeaponList = new List<WeaponRoot>(); // 模拟数据，实际从管理器获取

    protected override void Awake()
    {
        // 调用父类 Awake，确保控件字典初始化
        base.Awake();
        // 如果没有手动拖 GridLayoutGroup，自动获取
        if (gridLayout == null)
            gridLayout = GetComponent<GridLayoutGroup>();
    }

    // 外部调用此函数刷新武器栏，传入当前种类武器列表
    public void RefreshWeaponSlots(List<WeaponRoot> weaponList)
    {
        currentWeaponList = weaponList;

        // 1. 删除现有的所有子物体（简单做法，大厂用对象池）
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 2. 根据武器数量动态生成格子
        foreach (WeaponRoot data in weaponList)
        {
            GameObject slot = Instantiate(itemSlotPrefab, transform);
            // 假设 ItemSlot 上有一个组件用于设置数据
            ItemSlot slotScript = slot.GetComponent<ItemSlot>();
            if (slotScript != null)
            {
                slotScript.SetData(data);
            }
        }

        // 3. 由于 GridLayoutGroup 会自动排列，无需手动计算坐标
        // 但如果你需要调整父物体大小以适应内容，可以这样做：
        // 动态计算总宽高（可选）
        // int columns = gridLayout.constraintCount; // 如果设置了 Constraint
        // int count = weaponList.Count;
        // 根据行列调整 GetComponent<RectTransform>().sizeDelta
    }
}