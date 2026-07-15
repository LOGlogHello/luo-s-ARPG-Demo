using UnityEngine;
using UnityEngine.Timeline;

namespace Luo.Skill
{
    [CreateAssetMenu(fileName = "NewSkillView", menuName = "Skill/View")]
    public class SkillViewSO : ScriptableObject
    {
        public TimelineAsset timeline;
        public GameObject hitEffectPrefab;
        // 注意：这里不包含时间数据，只包含视觉资产！
    }
}


