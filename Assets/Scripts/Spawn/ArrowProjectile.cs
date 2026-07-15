

using Luo.Character;
using Luo.Effect;
using Luo.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Spawn
{
    [RequireComponent (typeof(Rigidbody))]
    public class ArrowProjectile : SpawnableObject
    {
        private Rigidbody _rb;
        public List<EffectConfig> effects = new List<EffectConfig>();

        public override void OnSpawn()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (lifeTime>0&&_timer > lifeTime)
            {
                OnDespawn();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            var unit = collision.gameObject.GetComponent<CharacterUnit>();
            if (unit != null)
            {

                var context = new EffectContext
                {
                    hitDirection = (unit.transform.position - transform.position).normalized
                };

                foreach (var effectConfig in effects)
                {
                    effectConfig.Execute(Caster, unit, context);
                }
            }
            OnDespawn();
        }

        public override void OnDespawn()
        {
            Destroy(gameObject);
        }
    }
}
