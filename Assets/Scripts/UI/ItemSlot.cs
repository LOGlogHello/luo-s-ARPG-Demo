using Luo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;





public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;        // 显示武器图标
    public GameObject highlight;   // 高亮边框，默认关闭

    private WeaponRoot weaponData;

    public void SetData(WeaponRoot data)
    {
        
    }

    public void SetHighlight(bool active)
    {
        
    }

    // 点击时触发选中逻辑，可以通过事件系统通知管理器
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}