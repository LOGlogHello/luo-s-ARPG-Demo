// SpawnEffectSO.cs
using Luo.Character;
using Luo.Spawn;
using UnityEngine;
using UnityEngine.UIElements;

namespace Luo.Effect
{
    [CreateAssetMenu(fileName = "SpawnEffect", menuName = "Skill/Effect/Spawn")]
    public class SpawnEffectSO : SkillEffectSO
    {
        [Header("生成物配置")]
        [Tooltip("要生成的预制体（必须挂载 SpawnableObject）")]
        public GameObject prefab;

        [Tooltip("生成位置偏移（相对于施法者）")]
        public Vector3 spawnOffset = Vector3.zero;

        [Tooltip("生成旋转偏移（相对于施法者）")]
        public Vector3 spawnRotation = Vector3.zero;

        public float Scale = 1f;

        [Tooltip("是否跟随施法者移动（法阵通常不跟随，投射物通常不跟随）")]
        public bool followCaster = false;

        [Header("生成后在几秒后自动销毁（0 表示永不自动销毁，由生成物自身决定）")]
        public float autoDestroyDelay = 0f;

        public override void Execute(CharacterUnit caster, CharacterUnit target, EffectContext context)
        {
            if (prefab == null)
            {
                Debug.LogError("SpawnEffect: 预制体为空！");
                return;
            }

            // 计算生成位置和旋转
            Vector3 spawnPos = caster.transform.position + caster.transform.TransformDirection(spawnOffset);
            Quaternion spawnRot = caster.transform.rotation * Quaternion.Euler(spawnRotation);
            // 实例化
            GameObject go = Object.Instantiate(prefab, spawnPos, spawnRot);
            go.transform.localScale = new Vector3(Scale, Scale, Scale);
            // 如果跟随施法者，设置为子物体
            if (followCaster)
            {
                go.transform.SetParent(caster.transform, true); // 保持世界位置不变
            }

            var spawnable = go.GetComponent<SpawnableObject>();
            if (spawnable != null)
            {
                spawnable.Caster = caster;
                spawnable.lifeTime = autoDestroyDelay;
                spawnable.OnSpawn();
            }

        }
    }
}