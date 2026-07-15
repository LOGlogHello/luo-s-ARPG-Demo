using UnityEngine;

// 窗口类型枚举（方便调试和状态机判断）
public enum SkillWindowType
{
    None,
    StartUp,      // 前摇
    Damage,       // 伤害判定
    Combo,        // 连击输入窗口
    Recovery1,    // 强制后摇（不可打断）
    Recovery2     // 可重置后摇（可跳过后摇或触发下一段）
}

/// <summary>
/// 技能时间轴数据（组合进 ActiveSkillDataSO 中）
/// 所有时间均为归一化时间（0~1）
/// </summary>
[System.Serializable]
public struct SkillTimeline
{
    [Header("前摇窗口")]
    [Range(0f, 1f)] public float startUpStart;
    [Range(0f, 1f)] public float startUpEnd;

    [Header("伤害/效果判定窗口")]
    [Range(0f, 1f)] public float effectStart;
    [Range(0f, 1f)] public float effectEnd;

    [Header("连击输入窗口")]
    [Range(0f, 1f)] public float comboStart;
    [Range(0f, 1f)] public float comboEnd;

    [Header("强制后摇窗口（不可打断/移动）")]
    [Range(0f, 1f)] public float recovery1Start;
    [Range(0f, 1f)] public float recovery1End;

    [Header("可跳过后摇窗口（按攻击键重置连击）")]
    [Range(0f, 1f)] public float recovery2Start;
    [Range(0f, 1f)] public float recovery2End;

    // ===== 便捷查询方法 =====

    /// <summary>
    /// 判断当前时间是否在指定的窗口内
    /// </summary>
    public bool IsInWindow(float normalizedTime, SkillWindowType type)
    {
        return type switch
        {
            SkillWindowType.StartUp => normalizedTime >= startUpStart && normalizedTime <= startUpEnd,
            SkillWindowType.Damage => normalizedTime >= effectStart && normalizedTime <= effectEnd,
            SkillWindowType.Combo => normalizedTime >= comboStart && normalizedTime <= comboEnd,
            SkillWindowType.Recovery1 => normalizedTime >= recovery1Start && normalizedTime <= recovery1End,
            SkillWindowType.Recovery2 => normalizedTime >= recovery2Start && normalizedTime <= recovery2End,
            _ => false
        };
    }

    /// <summary>
    /// 获取当前时间点所处的窗口类型（优先返回第一个匹配的）
    /// 注意：因为窗口可能重叠，这里按“优先级”返回，你可以在调用处调整顺序
    /// </summary>
    public SkillWindowType GetCurrentWindow(float normalizedTime)
    {
        if (IsInWindow(normalizedTime, SkillWindowType.Damage)) return SkillWindowType.Damage;
        if (IsInWindow(normalizedTime, SkillWindowType.Combo)) return SkillWindowType.Combo;
        if (IsInWindow(normalizedTime, SkillWindowType.StartUp)) return SkillWindowType.StartUp;
        if (IsInWindow(normalizedTime, SkillWindowType.Recovery1)) return SkillWindowType.Recovery1;
        if (IsInWindow(normalizedTime, SkillWindowType.Recovery2)) return SkillWindowType.Recovery2;
        return SkillWindowType.None;
    }

    /// <summary>
    /// 检查技能是否已经结束（所有后摇结束）
    /// </summary>
    public bool IsSkillFinished(float normalizedTime)
    {
        return normalizedTime >= recovery1End && normalizedTime >= recovery2End;
    }
}