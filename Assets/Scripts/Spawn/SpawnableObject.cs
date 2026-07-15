// SpawnableObject.cs
using Luo.Character;
using Luo.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Spawn
{
    
    public abstract class SpawnableObject : MonoBehaviour
    {
        public CharacterUnit Caster { get; set; }
        public float lifeTime;
        protected float _timer=0f;
        public abstract void OnSpawn();
        public abstract void OnDespawn();
    }
}


