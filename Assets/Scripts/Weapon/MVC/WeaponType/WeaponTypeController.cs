using Luo;
using Luo.Events;
using UnityEngine;

public class WeaponTypeController : MonoBehaviour
{
    [Header("View 引用")]
    public WeaponUIView weaponView;

    public WeaponType currentType= WeaponType.None;

    public void Start()
    {
        InitWeaponUI();
    }


    public void InitWeaponUI()
    {
        // 1. 生成和数据数量一致的按钮
        int total = WeaponTypeModel.Instance.GetWeaponCount();
        weaponView.CreateWeaponTypes(total);

        // 2. 循环给每个按钮设置图标、文字、点击事件
        for (int i = 0; i < total; i++)
        {
            var data = WeaponTypeModel.Instance.GetWeaponData(i);
            // 设置图标
            weaponView.SetBtnIcon(i, data.inventoryIcon);
            // 设置按钮文字
            weaponView.SetBtnText(i, data.weaponType.ToString());
            // 绑定点击回调，携带当前武器类型
            var type = data.weaponType;
            weaponView.SetBtnClickEvent(i, () => OnSelectWeapon(type));
        }
    }

    private void OnSelectWeapon(WeaponType type)
    {
        //当前武器类型 {type} 已经被选中，无需重复选择
        if (currentType == type)
            return;

        // 1. 构建事件数据（携带选中的武器类型）
        WeaponTypeChangedEvent evt = new WeaponTypeChangedEvent
        {
            newIndex = (int)type,
            newData = WeaponTypeModel.Instance.GetWeaponData((int)type)
        };

        currentType=type;

        // 2. 派发事件（通知所有监听者）
        EventManager.Dispatch(evt);
    }
}
