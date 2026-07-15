using Luo;
using Luo.Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WeaponUIView : BasePanel
{
    [Header("布局配置")]
    [Tooltip("按钮预制体模板")]
    public Button btnPrefab;
    [Tooltip("单个按钮正方形尺寸")]
    public Vector2 cellSize = new Vector2(80, 80);
    [Tooltip("按钮横向间距")]
    public int spaceX = 12;
    [Tooltip("面板左右内边距")]
    public int paddingLR = 8;

    private RectTransform _panelRt;
    private GridLayoutGroup _grid;
    // 存储所有动态生成的按钮
    private readonly List<Button> _weaponBtns = new List<Button>();

    protected override void Awake()
    {
        _panelRt = GetComponent<RectTransform>();
        // 自动挂载GridLayoutGroup
        _grid = GetComponent<GridLayoutGroup>();
        if (_grid == null) _grid = gameObject.AddComponent<GridLayoutGroup>();

        // 配置单行横向布局
        _grid.cellSize = cellSize;
        _grid.spacing = new Vector2(spaceX, 0);
        _grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        _grid.constraintCount = 1; // 固定一行，横向排列
        _grid.padding.left = paddingLR;
        _grid.padding.right = paddingLR;
        _grid.padding.top = _grid.padding.bottom = 0;
        _grid.childAlignment = TextAnchor.MiddleLeft; 

        ClearAllButtons();
    }
 

    /// <summary>
    /// 根据数量生成对应按钮，自动修改面板宽度
    /// </summary>
    /// <param name="count">生成按钮数量</param>
    public void CreateWeaponTypes(int count)
    {
        ClearAllButtons();
        if (count <= 0 || btnPrefab == null)
        {
            SetPanelTotalWidth(0);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Button newBtn = Instantiate(btnPrefab, transform);
            newBtn.name = $"WeaponBtn_{i}";
            _weaponBtns.Add(newBtn);
        }

        // 重新计算并更新面板宽度
        SetPanelTotalWidth(count);
    }

    #region 对外操作接口（Controller调用，控制单个按钮内容）
    /// <summary>
    /// 设置指定索引按钮的图标
    /// </summary>
    public void SetBtnIcon(int index, Sprite iconSprite)
    {
        if (!IsIndexValid(index)) return;

        //includeInactive: false 查找image时，不带_weaponBtns[index]自身的image组件
        Image iconImg = _weaponBtns[index].transform.Find("Icon")?.GetComponent<Image>();
        if (iconImg != null && iconSprite != null)
        {
            iconImg.sprite = iconSprite;
        }
    }

    /// <summary>
    /// 设置指定索引按钮文字
    /// </summary>
    public void SetBtnText(int index, string text)
    {
        if (!IsIndexValid(index)) return;
        TextMeshProUGUI txt = _weaponBtns[index].GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = text;
    }

    /// <summary>
    /// 给指定按钮绑定点击事件
    /// </summary>
    public void SetBtnClickEvent(int index, UnityEngine.Events.UnityAction callback)
    {
        if (!IsIndexValid(index) || callback == null) return;
        Button btn = _weaponBtns[index];
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(callback);
    }

    /// <summary>
    /// 获取按钮总数
    /// </summary>
    public int GetBtnCount() => _weaponBtns.Count;
    #endregion

    #region 内部工具方法
    // 索引合法性校验
    private bool IsIndexValid(int index)
    {
        return index >= 0 && index < _weaponBtns.Count;
    }

    // 根据按钮数量计算并设置Panel宽度
    private void SetPanelTotalWidth(int btnCount)
    {
        if (btnCount <= 0)
        {
            _panelRt.sizeDelta = new Vector2(0, _panelRt.sizeDelta.y);
            return;
        }

        // 宽度计算公式：左右内边距 + 所有按钮宽 + 间隙总和
        float totalPad = paddingLR * 2;
        float totalBtnW = cellSize.x * btnCount;
        float totalGap = spaceX * (btnCount - 1);
        float fullWidth = totalPad + totalBtnW + totalGap;

        // 仅修改宽度，高度保持原有
        _panelRt.sizeDelta = new Vector2(fullWidth, _panelRt.sizeDelta.y);
    }

    // 销毁所有动态生成按钮，清空列表
    private void ClearAllButtons()
    {
        foreach (var btn in _weaponBtns)
        {
            Destroy(btn.gameObject);
        }
        _weaponBtns.Clear();
    }
    #endregion
}