using UnityEngine;
using UnityEngine.Playables;
namespace Luo.Skill.Timeline
{
    [System.Serializable]
    public class ColliderControlClip : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ColliderControlBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            return playable;
        }
    }

}

