using UnityEngine;
using UnityEngine.Timeline;
namespace Luo.Skill.Timeline
{

    [TrackClipType(typeof(ColliderControlClip))]
    [TrackBindingType(typeof(WeaponRoot))]
    public class ColliderControlTrack : TrackAsset { }
}


