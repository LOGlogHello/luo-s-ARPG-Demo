using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 当选中 Timeline Asset 时，自动将其赋值给场景中 Tag 为 "Player" 的物体的 PlayableDirector。
/// </summary>
[InitializeOnLoad]
public static class TimelineAutoAssign
{
    static TimelineAutoAssign()
    {
        // 订阅选择变化事件
        Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged()
    {
        // 只处理单个选中对象
        var selected = Selection.activeObject;
        if (selected == null) return;

        // 检查选中的是否是 TimelineAsset（注意：不能直接使用 is TimelineAsset，因为类型在不同版本可能不同）
        if (!(selected is TimelineAsset timeline)) return;

        // 尝试在场景中找到 Tag 为 "Player" 的物体
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"[TimelineAutoAssign] 场景中未找到 Tag 为 'Player' 的物体");
            return;
        }

        // 获取或添加 PlayableDirector
        PlayableDirector director = player.GetComponent<PlayableDirector>();
        if (director == null)
        {
            director = player.AddComponent<PlayableDirector>();
            Debug.Log($"[TimelineAutoAssign] 已为 Player 添加 PlayableDirector 组件");
        }

        // 如果当前的 Timeline 已经是同一个，跳过
        if (director.playableAsset == timeline) return;

        // 赋值并记录
        Undo.RecordObject(director, "Assign Timeline to Player");
        director.playableAsset = timeline;

        // 可选：自动将 Director 的 Update Method 设置为 Game Time（按需求可改）
        // director.timeUpdateMode = DirectorUpdateMode.GameTime;

        Debug.Log($"[TimelineAutoAssign] 已将 Timeline '{timeline.name}' 自动赋给 Player 的 PlayableDirector");
    }
}