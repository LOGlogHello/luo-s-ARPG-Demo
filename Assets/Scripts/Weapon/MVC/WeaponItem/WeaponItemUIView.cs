using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Luo
{
    /// <summary>
    /// 武器 Item 列表的 UI 视图（右侧面板）
    /// </summary>
    public class WeaponItemUIView : BasePanel
    {
        [Header("布局配置")]
        public Button itemPrefab;               // 单个武器 Item 的预制体（含 Icon, Name, Level 等）
        public Vector2 cellSize = new Vector2(100, 120);
        public Vector2 spacing = new Vector2(10, 10);
        public RectOffset padding;              // 内边距，左右用于计算宽度

        private RectTransform _panelRt;
        private GridLayoutGroup _grid;
        private readonly List<Button> _itemBtns = new List<Button>();

        protected override void Awake()
        {
            base.Awake();

            _panelRt = GetComponent<RectTransform>();
            if (padding == null)
                padding = new RectOffset(10, 10, 10, 10);

            _grid = GetComponent<GridLayoutGroup>();
            if (_grid == null) _grid = gameObject.AddComponent<GridLayoutGroup>();

            // 配置单行横向布局（与 WeaponTypeUIView 一致）
            _grid.cellSize = cellSize;
            _grid.spacing = spacing;
            _grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            _grid.constraintCount = 1; // 固定一行，横向排列
            _grid.padding = padding;
            _grid.childAlignment = TextAnchor.MiddleLeft;

            ClearAllItems();
        }

        /// <summary>
        /// 根据数量生成对应数量的 Item 按钮
        /// </summary>
        public void CreateWeaponItems(int count)
        {
            ClearAllItems();
            if (count <= 0 || itemPrefab == null)
            {
                SetPanelTotalWidth(0);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Button newBtn = Instantiate(itemPrefab, transform);
                newBtn.name = $"WeaponItem_{i}";
                _itemBtns.Add(newBtn);
            }

            SetPanelTotalWidth(count);
        }

        #region 对外设置接口（Controller 调用）
        public void SetItemIcon(int index, Sprite icon)
        {
            if (!IsIndexValid(index)) return;
            Image img = _itemBtns[index].transform.Find("Icon")?.GetComponent<Image>();
            if (img != null) img.sprite = icon;
        }

        public void SetItemName(int index, string name)
        {
            if (!IsIndexValid(index)) return;
            TextMeshProUGUI txt = _itemBtns[index].transform.Find("WeaponName")?.GetComponent<TextMeshProUGUI>();
            if (txt != null) txt.text = name;
        }

        public void SetItemLevel(int index, int level)
        {
            if (!IsIndexValid(index)) return;
            TextMeshProUGUI levelTxt = _itemBtns[index].transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            if (levelTxt != null) levelTxt.text = $"Lv.{level}";
        }

        public void SetItemClickEvent(int index, UnityEngine.Events.UnityAction callback)
        {
            if (!IsIndexValid(index) || callback == null) return;
            Button btn = _itemBtns[index];
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(callback);
        }

        public void HighlightItem(int index, bool highlight)
        {
            if (!IsIndexValid(index)) return;
            Image bg = _itemBtns[index].GetComponent<Image>();
            if (bg != null)
                bg.color = highlight ? Color.yellow : Color.white;
        }

        public void ClearAllItems()
        {
            foreach (var btn in _itemBtns)
                Destroy(btn.gameObject);
            _itemBtns.Clear();
            SetPanelTotalWidth(0); // 重置宽度
        }

        public int GetItemCount() => _itemBtns.Count;
        #endregion

        #region 内部工具方法
        private bool IsIndexValid(int index) => index >= 0 && index < _itemBtns.Count;

        // 根据按钮数量计算并设置Panel宽度
        private void SetPanelTotalWidth(int btnCount)
        {
            if (btnCount <= 0)
            {
                _panelRt.sizeDelta = new Vector2(0, _panelRt.sizeDelta.y);
                return;
            }

            // 宽度计算公式：左右内边距 + 所有按钮宽 + 间隙总和
            float totalPad = padding.left + padding.right;
            float totalBtnW = cellSize.x * btnCount;
            float totalGap = spacing.x * (btnCount - 1);
            float fullWidth = totalPad + totalBtnW + totalGap;

            _panelRt.sizeDelta = new Vector2(fullWidth, _panelRt.sizeDelta.y);
        }
        #endregion
    }
}