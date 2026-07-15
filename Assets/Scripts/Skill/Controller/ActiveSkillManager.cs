using Luo.Character;
using Luo.Character.Controller;
using Luo.Effect;
using Luo.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Luo.Skill
{
    public class SkillManager : MonoBehaviour
    {
        // 通用技能管理逻辑（如冷却、列表等）
    }

    [RequireComponent(typeof(PlayableDirector))]
    public class ActiveSkillManager : MonoBehaviour
    {
        [Header("由不同类型的武器加载的技能配置")]
        [SerializeField]
        private List<ActiveSkillController> _skills;

        [Header("额外技能（非武器，如天赋）")]
        [SerializeField] private List<ActiveSkillController> _activeSkillList = new List<ActiveSkillController>();

        // 合并后的总技能列表（仅用于遍历检测）
        private List<ActiveSkillController> _allSkills = new List<ActiveSkillController>();

        [Header("打断行为选项")]
        [Tooltip("连击触发时是否立即打断当前技能")]
        public bool interruptOnCombo = false;

        [Tooltip("后摇重置触发时是否立即打断当前技能")]
        public bool interruptOnRecovery = true;

        private Animator _animator;
        private PlayableDirector _director;

        [Tooltip("当前主动技能管理器所在character的controller")]
        private BaseController _controller; 

        // 当前运行的技能状态
        private ActiveSkillController _currentSkill;

        /// <summary>
        /// 状态机的当前技能 中，适合当前状态机的 上下文。
        /// 其中有 技能触发前必须处于的状态、技能执行时进入的状态、技能结束后回到的状态。
        /// </summary>
        private ActiveSkillStateBinding _currentBinding;
        private bool _isExecuting;

        [Tooltip("是否已经过了效果窗口")]
        private bool _hasDealtEffect;

        [Tooltip("效果应用中，某些对象是否已经应用过效果")]
        private HashSet<CharacterUnit> _hitTargets = new HashSet<CharacterUnit>();
        private float _progress;
        private ActiveSkillController _pendingNextSkill; // 等待自然结束的连击技能
        private bool _isSkillFullyCompleted = false;

        // ----- 事件订阅 -----
        private void OnEnable()
        {
            EventManager.AddListener<DeathEvent>(OnDeathEvent);
            EventManager.AddListener<WeaponItemSelectedEvent>(OnWeaponItemSelected);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<DeathEvent>(OnDeathEvent);
            EventManager.RemoveListener<WeaponItemSelectedEvent>(OnWeaponItemSelected);
        }

        private void OnWeaponItemSelected(WeaponItemSelectedEvent evt)
        {
            // 更新技能列表
            UpdateSkillList(evt);

            // 如果当前正在执行技能，强制中断（因为武器切换后技能可能不适用）
            if (_isExecuting)
            {
                _director.Stop();
                _isExecuting = false;
                _isSkillFullyCompleted = false;
                _currentSkill = null;
                _hasDealtEffect = false;
                _progress = 0f;
                _pendingNextSkill = null;
                _hitTargets.Clear();
                Debug.Log("武器切换，当前技能已中断");
            }
        }

        private void OnDeathEvent(DeathEvent evt)
        {
            if (evt.Character != _controller.mCharacterUnit) return;
            if (_isExecuting)
            {
                _director.Stop();
                _isExecuting = false;
                _isSkillFullyCompleted = false;
                _currentSkill = null;
                _hasDealtEffect = false;
                _progress = 0f;
                _pendingNextSkill = null;
                Debug.Log($"{name} 死亡，技能已中断");
            }
        }

        // ----- 初始化 -----
        private void Awake() 
        { 
            
        
        }


        public void Start()
        {
            _controller=GetComponent<BaseController>();
            _animator = GetComponent<Animator>();
            _director = GetComponent<PlayableDirector>();
            if (_director == null) _director = gameObject.AddComponent<PlayableDirector>();

            _director.stopped += OnTimelineStopped;
            RebuildAllSkills();
        }

        private void Update()
        {
            if (!_isExecuting || _currentSkill == null) return;

            _progress = (float)(_director.time / _director.duration);
            _progress = Mathf.Clamp01(_progress);
            CheckTimeline(_progress);
        }



        // ----- 时间轴检测 -----
        private void CheckTimeline(float progress)
        {
            var timeline = _currentSkill.data.timeline;

            // 1. 效果判定
            if (!_hasDealtEffect && progress >= timeline.effectStart && progress <= timeline.effectEnd)
            {
                if(_currentSkill.data.targetSelector!=null)
                {
                    //动态获取target
                    var targetResults = _currentSkill.data.targetSelector.GetTargets(_controller.mCharacterUnit, transform.position, _currentSkill.data);
                    foreach (var result in targetResults)
                    {
                        if (_hitTargets.Contains(result.target)) continue;
                        _hitTargets.Add(result.target);

                        var context = new EffectContext
                        {
                            hitDirection = result.direction,
                            
                        };
                        foreach (var effect in _currentSkill.data.effects)
                            effect.Execute(_controller.mCharacterUnit, result.target, context);
                    }
                }
                else
                {
                    foreach (var effect in _currentSkill.data.effects)
                        effect.Execute(_controller.mCharacterUnit, null);
                }
                
            }
            else if (progress > timeline.effectEnd)
            {
                // 离开效果窗口，清空命中记录（下次释放技能时重新检测）
                _hitTargets.Clear();
                _hasDealtEffect = true;
            }

            // 2. 连击窗口
            if (_currentSkill != null && _currentSkill.data.canCombo &&
                progress >= timeline.comboStart && progress <= timeline.comboEnd)
            {
                if (CheckAllTransitions(_currentSkill.data.comboTransitions, isRecovery: false))
                    return;
            }

            // 3. 后摇可打断窗口
            if (_currentSkill != null && _currentSkill.data.canRecovery &&
                progress >= timeline.recovery2Start && progress <= timeline.recovery2End)
            {
                if (CheckAllTransitions(_currentSkill.data.recoveryTransitions, isRecovery: true))
                    return;
            }

            // 4. 技能自然结束 应该交给OnTimelineStopped(PlayableDirector director)来做，而非在这里。
            //在这里做，即使 timeline.recovery2End是0.99，也有可能因为progress 从0.94->0.97-> 结束 直接跳过了当前这里的代码的执行
            //Debug.Log($"progress {progress}， timeline.recovery2End {timeline.recovery2End}");
            //if (progress >= timeline.recovery2End)
            //{

            //}
        }

        // ----- 过渡检测 -----
        private bool CheckAllTransitions(List<SkillTransition> transitions, bool isRecovery)
        {
            if (transitions == null || transitions.Count == 0) return false;

            // 遍历每个过渡
            foreach (var transition in transitions)
            {
                var targetSkill = transition.targetSkill;
                if (targetSkill == null) continue;

                // 检查目标技能的 condition 是否满足（任意一个）
                bool conditionMet = false;
                if (targetSkill.conditions != null && targetSkill.conditions.Count > 0)
                {
                    foreach (var cond in targetSkill.conditions)
                    {
                        if (cond != null && cond.IsMet(_controller.mCharacterUnit, this))
                        {
                            conditionMet = true;
                            break;
                        }
                    }
                }
                else
                {
                    // 如果没有配置 condition，视为总是满足
                    conditionMet = true;
                }

                if (!conditionMet) continue;

                // 条件满足，执行过渡
                ExecuteTransition(targetSkill, isRecovery);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取单个技能 匹配的技能状态绑定。
        /// 先匹配 controllerType 和 triggerStateID，再检查技能自身的 conditions。
        /// </summary>
        public ActiveSkillStateBinding GetMatchingStateBinding(ActiveSkillController skill)
        {
            if (skill == null || skill.data == null) return null;
            if (skill.stateBindings == null) return null;

            int currentStateID = _controller.GetCurrentStateID();
            var controllerType = _controller.Type;

            foreach (var binding in skill.stateBindings)
            {
                if (binding.controllerType != controllerType || binding.triggerStateID != currentStateID)
                    continue;

                // 检查绑定自身的额外条件
                if (binding.conditions != null)
                {
                    bool allMet = true;
                    foreach (var cond in binding.conditions)
                    {
                        if (!cond.IsMet(_controller.mCharacterUnit, this))
                        {
                            allMet = false;
                            break;
                        }
                    }
                    if (!allMet) continue;
                }

                // 检查技能整体的进入条件
                bool dataConditionsMet = false;
                if (skill.data.conditions == null || skill.data.conditions.Count == 0)
                    dataConditionsMet = true;
                else
                {
                    foreach (var cond in skill.data.conditions)
                    {
                        if (cond.IsMet(_controller.mCharacterUnit, this))
                        {
                            dataConditionsMet = true;
                            break;
                        }
                    }
                }

                if (!dataConditionsMet) continue;

                return binding;
            }
            return null;
        }


        // 遍历所有技能，找到第一个匹配的
        public bool TryGetFirstMatchingSkill(out ActiveSkillController matchedSkill, out ActiveSkillStateBinding matchedBinding)
        {
            matchedSkill = null;
            matchedBinding = null;

            if (_allSkills == null) return false;

            foreach (var skill in _allSkills)
            {
                var binding = GetMatchingStateBinding(skill);
                if (binding != null)
                {
                    matchedSkill = skill;
                    matchedBinding = binding;
                    return true;
                }
            }
            return false;
        }

        // ----- 执行过渡 -----
        private void ExecuteTransition(ActiveSkillDataSO nextData, bool isRecovery)
        {
            if (nextData == null) return;

            var nextSkill = _allSkills.Find(s => s.data == nextData);
            if (nextSkill == null)
            {
                Debug.LogError($"未找到技能: {nextData.name}");
                return;
            }

            bool shouldInterrupt = isRecovery ? interruptOnRecovery : interruptOnCombo;
            //bool shouldInterrupt = true;

            if (shouldInterrupt)
            {
                // 立即打断
                _director.Stop();
                ExecuteSkill(nextSkill);
                Debug.Log($"{(isRecovery ? "后摇重置" : "连击")}立即打断：{nextData.name}");
            }
            else
            {
                // 等待当前技能自然结束
                _pendingNextSkill = nextSkill;
                Debug.Log($"{(isRecovery ? "后摇重置" : "连击")}已记录，等待当前技能结束：{nextData.name}");
            }
        }

        // ----- 执行技能（公共接口） -----
        public void ExecuteSkill(int index)
        {
            if (_skills == null || index < 0 || index >= _skills.Count) return;
            var skill = _skills[index];
            ExecuteSkill(skill);
        }

        /// <summary>
        /// 不改变状态机状态的 技能执行函数。适用与 连击技能、后摇重置技能。
        /// </summary>
        /// <param name="skill"></param>
        public void ExecuteSkill(ActiveSkillController skill)
        {
            _pendingNextSkill = null; // 清除任何待切换的记录
            _currentSkill = skill;
            _isExecuting = true;
            _isSkillFullyCompleted = false;
            _hasDealtEffect = false;
            _progress = 0f;
            _hitTargets.Clear(); //

            _director.playableAsset = _currentSkill.data.view.timeline;
            _director.Play();
        }

        public void ExecuteSkill(ActiveSkillController skill, ActiveSkillStateBinding binding = null)
        {
            _pendingNextSkill = null;
            _currentSkill = skill;
            _currentBinding = binding; // 保存当前绑定
            _isExecuting = true;
            _isSkillFullyCompleted = false;
            _hasDealtEffect = false;
            _progress = 0f;
            _hitTargets.Clear();

            _director.playableAsset = _currentSkill.data.view.timeline;
            _director.Play();
        }

        // ----- Timeline 结束回调 -----
        private void OnTimelineStopped(PlayableDirector director)
        {
            if (_isExecuting)
            {
                // 如果 Director 停止但技能还没有自然结束（例如手动 Stop），我们在这里清理状态
                // 但注意：如果有 _pendingNextSkill，也会在 CheckTimeline 的结束逻辑中处理
                // 这里只是兜底

                if (_pendingNextSkill != null)
                {
                    // 执行等待的连击技能
                    ExecuteSkill(_pendingNextSkill);
                    _pendingNextSkill = null;
                    Debug.Log("连击接续：等待结束后播放下一段");
                }
                else
                {
                    _isExecuting = false;
                    _isSkillFullyCompleted = true;
                    _currentSkill = null;
                    Debug.Log("技能链已完整结束");
                }
                //Debug.Log("技能结束（Timeline Stopped）");
            }
        }

        private void UpdateSkillList(WeaponItemSelectedEvent evt)
        {
            WeaponPlayerHas weaponPlayerHas = evt.selectedWeapon;
            if (weaponPlayerHas!=null && weaponPlayerHas.stats != null)
            {
                _skills = weaponPlayerHas.stats.skills;
                
            }
            else
            {
                _skills = null; // 或 new List<ActiveSkillController>();
            }
            RebuildAllSkills();
        }


        private void RebuildAllSkills()
        {
            _allSkills.Clear();
            if (_skills != null) _allSkills.AddRange(_skills);
            if (_activeSkillList != null) _allSkills.AddRange(_activeSkillList);
        }
        private void OnDestroy()
        {
            if (_director != null)
                _director.stopped -= OnTimelineStopped;
        }

        // ----- 公共属性 -----
        public bool mIsExecuting => _isExecuting;
        public ActiveSkillController mCurrentSkill => _currentSkill;
        public float mCurrentSkillProcess => _progress;
        public bool IsSkillFullyCompleted => _isSkillFullyCompleted;
        public List<ActiveSkillController> mSkills => _skills;
        public ActiveSkillStateBinding mCurrentBinding =>_currentBinding;
    }
}