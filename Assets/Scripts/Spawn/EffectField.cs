using Luo.Character;
using Luo.Effect;
using Luo.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Spawn
{
    [Tooltip("")]
    //这个 法阵 自己有几个字段，如tickInterval，用来让法阵自己 对 处于法阵中的敌人施加  effects中的瞬时效果。
    //  如进入火焰区域，火焰区域每隔tickInterval 让处在其中的人掉血
    //之于effects中的OT效果，可以设计为 玩家离开法阵区域 或 法阵消失时，给角色加上一个 OT效果（buff)。
    //  如离开火焰区域或火焰区域消失，会给在其中的角色添加一个 灼烧OTbuff。
    //  问题是 若角色反复进出 法阵区域 或 角色离开法阵区域时，法阵正好消失，岂不是会给角色添加数个重复buff?
    //  所以OTbuff 添加前，应该先检查角色是否已经有该DOTbuff。这一步应该交给 CharacterUnit自身吗？比如角色同一时间只能有一个灼烧种类的OT buff。
    //  也就说，要给oTEffectSO基类添加一个Type字段，用以维持这种Type的OTbuff 在同一角色身上的一致性？
    //  玩家反复进出法阵，玩家身上的灼烧OT 效果应该刷新，也就是说：新的灼烧buff取代了旧的灼烧buff.

    // 另外，若是这种法阵：进入其中的玩家攻击力增加，离开后攻击力回到之前的。这种该怎么做呢？
    public class EffectField : SpawnableObject
    {
        [Header("范围参数")]
        public float radius = 3f;

        [Header("Tick参数")]
        public float tickInterval = 1f;

        [Header("效果配置")]
        public List<EffectConfig> EnterEffects = new List<EffectConfig>();
        public List<EffectConfig> TickEffects = new List<EffectConfig>();
        public List<EffectConfig> ExitEffects = new List<EffectConfig>();

        // 记录当前在法阵内的单位，避免重复触发
        private HashSet<CharacterUnit> _insideUnits = new HashSet<CharacterUnit>();

        public override void OnSpawn()
        {
            _insideUnits.Clear();
        }

        private void Update()
        {
            // 1. 获取当前范围内所有单位
            var hits = Physics.OverlapSphere(transform.position, radius);
            var currentUnits = new HashSet<CharacterUnit>();
            foreach (var hit in hits)
            {
                var unit = hit.GetComponent<CharacterUnit>();
                if (unit != null && unit != Caster)
                {
                    currentUnits.Add(unit);
                }
            }

            // 2. 处理进入（在 currentUnits 但不在 _insideUnits）
            foreach (var unit in currentUnits)
            {
                if (!_insideUnits.Contains(unit))
                {
                    _insideUnits.Add(unit);
                    foreach (var effect in EnterEffects)
                        effect.Execute(Caster, unit);
                }
            }

            // 3. 处理离开（在 _insideUnits 但不在 currentUnits）
            var leftUnits = new List<CharacterUnit>();
            foreach (var unit in _insideUnits)
            {
                if (!currentUnits.Contains(unit))
                {
                    leftUnits.Add(unit);
                }
            }
            foreach (var unit in leftUnits)
            {
                _insideUnits.Remove(unit);
                foreach (var effect in ExitEffects)
                    effect.Execute(Caster, unit);
            }

            // 4. 处理 Tick 效果（对当前在范围内的所有单位）
            _timer += Time.deltaTime;
            if (_timer >= tickInterval)
            {
                _timer = 0f;
                foreach (var unit in _insideUnits)
                {
                    foreach (var effect in TickEffects)
                        effect.Execute(Caster, unit);
                }
            }

            if (lifeTime > 0 && _timer > lifeTime)
            {
                OnDespawn();
            }
        }

        public override void OnDespawn()
        {
            // 法阵消失时，对所有仍在范围内的单位触发离开效果
            foreach (var unit in _insideUnits)
            {
                foreach (var effect in ExitEffects)
                    effect.Execute(Caster, unit);
            }
            _insideUnits.Clear();
            Destroy(gameObject);
        }
    }
}
