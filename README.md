# luo-s-ARPG-Demo
本作品为一款 Unity3D ARPG 互动游戏 Demo，围绕玩家控制、敌人 AI、战斗技能、武器系统、UI 界面、场景切换、动画状态机、碰撞检测、音效与资源管理等内容进行设计与实现。作品使用 Unity 新输入系统、Animator 状态机、CharacterController、Collider、NavMesh、Cinemachine、ScriptableObject 数据资产等技术，完成了主游戏场景、玩家与敌人交互、武器选择和简单战斗反馈等功能。

## 0. 开场
大家好，这期视频我想分享一下我做的这个 Unity ARPG Demo 背后是怎么实现的。前面那个演示视频大家已经看到了效果，这期我重点讲代码和系统设计。整个项目我把它拆成了几个相对独立的模块：输入系统、玩家状态机、角色移动、敌人 AI、战斗技能系统、武器三层架构、武器 UI 的 MVC、动画状态机、UI 框架，以及一个公共基础框架。我会一个一个讲它们各自解决了什么问题、是怎么组织的。

## 1. 输入系统
我先从输入讲起。项目用的是 Unity 的新输入系统，配置文件是 InputSystem_Actions.inputactions 。里面分了两个 ActionMap：一个叫 Player，一个叫 UI。

Player 这一组里定义了我需要的所有动作，比如 Move、Look、Attack、Heavy Attack、SpellCast、ReadyToAttack、EquipWeapon、Jump、Crouch、Sprint、Zoom、Next、Previous、Interact。每个动作都绑定了具体的按键，比如移动是 WASD，攻击是鼠标左键，重攻击是另一个按键。同时它还兼容了手柄、摇杆和触屏，这样新输入系统会帮我处理设备适配，我不用自己写 if 判断键盘还是手柄。

真正读输入的是 InputReader 这个类。它持有生成的 GameControl 引用，对外暴露一堆属性，比如 IsJumpPressed 、 IsHeavyAttackPressed 、 RawMoveValue 。其他脚本不直接碰 InputAction，而是向 InputReader 要数据。这样做的好处是，输入来源被统一收口了，以后换设备或者改按键，只动 InputReader 就行。

另外我还做了一个 InputBuffer ，用来做输入缓冲。它的思路是：玩家按下攻击时，如果角色当前还处在不能马上攻击的状态，这个输入不会直接丢掉，而是被记到一个短时间的缓冲窗口里，默认零点三秒。等角色进入可以攻击的时间点，再去消费这个缓冲输入。这样手感会更跟手，不会出现"我明明按了但没反应"的感觉。

## 2. 玩家状态机
玩家逻辑我用的是一个轻量的有限状态机。核心是 IPlayerState 接口，它定义了三个方法：Enter、Execute、Exit。每种状态都实现这个接口，比如 PlayerIdleState 、 PlayerMoveState 、 PlayerAttackState 、 PlayerSkillState 、 PlayerDeathState 。

PlayerController 就是状态机的持有者，它每帧 Update 里调用 currentPlayerState.Execute(this) 。状态切换通过 ChangeState 完成，它会先调旧状态的 Exit，再调新状态的 Enter。

我还做了一个 PlayerStateInstanceManager ，它继承自我的 BaseManager 单例基类，用来缓存状态实例。所有状态在启动时注册一次，之后用枚举 PlayerStateType 去取，避免每次切换都 new 一个对象。这样状态对象是共享的、无状态的，逻辑都依赖外部传入的 PlayerController。

状态内部的逻辑举个例子：IdleState 里会检测输入，如果玩家按下攻击，它不会自己直接播攻击动画，而是调用 player.TryTriggerSkill() ，把"能不能攻击、该播哪个技能"这件事交给技能系统去判断。MoveState 里会读输入方向，结合摄像机朝向，算出实际的移动方向，再调用 Locomotion.Move 。AttackState 则是技能系统接管后进入的状态，技能结束或被取消时，它会根据当前技能绑定的 exitStateID 切回 Idle 或 Move。

## 3. 角色移动 Locomotion
移动我单独抽了一个 Locomotion 组件，玩家和敌人共用。它挂在 CharacterController 上，强制依赖 CharacterController。

这里我做了一个设计选择：水平位移由动画 Root Motion 提供，垂直位移由物理代码控制。这样做的好处是，角色走路、跑步、攻击时的位移表现完全由动画驱动，看起来更自然，不会出现滑步；而跳跃和重力又由代码精确控制，不会因为动画位移导致飞天或下穿。

具体流程是：Animator 的 OnAnimatorMove 把动画位移传给 SetRootMotionDelta ，Locomotion 在 FixedUpdate 里把水平方向用 Root Motion 增量、垂直方向用速度加重力，合成最终位移，再交给 CharacterController.Move 处理碰撞。落地时我会给一个轻微的下拉力，让角色牢牢贴地，防止微弹跳。

Locomotion 还支持蹲下。蹲下时我会把 CharacterController 的高度从站姿高度过渡到蹲姿高度，同时调整中心点，保证脚底位置不变。死亡时会禁用 CharacterController，清空速度，让角色不再响应移动输入。

## 4. 敌人 AI 状态机
敌人这边我也是一套状态机，结构和玩家对称。 EnemyController 持有当前 IEnemyState ，状态枚举有 Idle、Patrol、Chase、Attack、Dead。状态实例同样由 EnemyStateInstanceManager 缓存。

真正做"思考"的是 EnemyAI 这个组件。它不直接控制动画，而是负责感知和决策。它有几个关键参数：检测范围、攻击范围、丢失范围、巡逻半径、巡逻等待时间、攻击冷却。每帧它会检测玩家是否进入这些范围，然后根据当前状态调用对应的处理函数，比如 OnIdle、OnPatrol、OnChase、OnAttack。

举个例子：敌人在 Idle 时，如果玩家进入检测范围，就切到 Chase；追到攻击范围内，就切到 Attack；如果玩家跑出丢失范围，就回到 Idle 或 Patrol。巡逻状态会在出生点附近随机生成一个巡逻目标点，走过去等一会儿再选下一点。攻击状态有冷却计时，避免敌人一直粘着玩家打。

移动方面，敌人状态里会调用 enemy.mLocomotion.Move(dir) ，由 Locomotion 负责实际位移。场景里我还配了 NavMeshSurface 和 NavMeshModifier，用来烘焙可行走区域。敌人受伤时会通过事件触发受击表现，生命值归零后切到 Dead 状态。

## 5. 战斗与技能系统
这一块是我花心思最多的部分。我没有把攻击逻辑写死在角色脚本里，而是做了一套数据驱动的技能系统。

核心是 ActiveSkillManager ，它挂载在角色上，管理这个角色有哪些技能、当前在执行哪个技能、技能进度到哪了。它内部维护了当前技能、当前状态绑定、是否正在执行、是否已经造成效果、命中目标集合、待执行的下一个技能等。

每个技能本身是一个 ActiveSkillDataSO ，也就是 ScriptableObject。它里面存了：技能名、冷却、是否激活、技能 Timeline、触发条件列表、效果配置列表、目标选择器、连击跳转、恢复跳转。这意味着一个技能"能不能放、打到谁、造成什么效果、放完能连到哪个技能"，全部是配置出来的，不用改代码。

技能能不能释放，由一组 SkillCondition 判断。我实现了好几种： InputActionCondition 判断输入动作状态， InputBufferCondition 判断输入缓冲， AnimatorCondition 判断动画参数， TimingCondition 判断技能时间窗口， TargetHealthCondition 判断目标血量，还有 TrueCondition 恒真。条件是抽象类，以后还能加。

打到谁，由 TargetSelector 决定。 MeleeWeaponSelector 用武器碰撞体做范围检测，按 LayerMask 筛选目标； SelfSelector 选自己，用于加血或增益。选择结果包含目标和方向，方便做受击方向表现。

造成什么效果，由 SkillEffectSO 体系负责。 DamageEffectSO 扣血， HealEffectSO 加血， DoTEffectSO 持续伤害， HoTEffectSO 持续治疗， OTAttackBoostSO 攻击力增益。持续类效果会通过 RuntimeBuff 挂到 CharacterUnit 上，带 Tick、到期回调，支持唯一性判断。

技能的表现我用的是 Unity Timeline。每个技能配了一个 SkillViewSO ，里面有 TimelineAsset 和命中特效预制体。 ActiveSkillManager 用 PlayableDirector 播放技能 Timeline。我还自定义了一条 ColliderControlTrack ，它是 Timeline 的一条轨道，配合 ColliderControlClip 和 ColliderControlBehaviour ，可以在 Timeline 的特定时间段自动开启或关闭武器碰撞体。这样就实现了"只在攻击挥到的瞬间才判定命中"，而不是整个动画一直能打到人。

技能之间还能连击。 SkillTransition 描述了从当前技能跳到目标技能的条件，比如在连击窗口内再次按攻击，就跳到下一段。 ActiveSkillStateBinding 把技能和状态机绑起来，告诉状态机：这个技能触发时要切到哪个状态，退出时切回哪个状态。这样技能系统和角色状态机就解耦了，玩家和敌人都能复用同一套技能数据。

## 6. 武器三层架构
武器系统我做了三层拆分。一把武器不是一个单一的类，而是由三个 ScriptableObject 组合而成： WeaponBaseStats 管基础属性，比如武器类型、ID、名字、基础伤害、基础耐久、每级成长值、附带技能列表； WeaponCombatData 管战斗相关数据，引用 stats； WeaponViewData 管显示，比如 3D 模型预制体、挂点骨骼、位置偏移、旋转偏移、背包图标。

玩家运行时持有的武器用 WeaponPlayerHas 表示，它引用这三个模板，再加上自己的动态数据：等级、当前耐久、额外伤害加成、额外耐久加成。这样模板数据是共享的、只读的，动态数据是每个实例自己的，不会互相影响。

武器的目录用 WeaponCatalog 管理，它是一张表，每一项是 WeaponCatalogEntry ，记录了武器类型、名字、ID 和三个模板在 Resources 下的路径。加载武器时，根据 ID 查表，再按路径去 Resources 加载三个模板，拼成 WeaponPlayerHas 。

WeaponRoot 是场景里挂武器的组件。它根据当前武器数据实例化模型，用 ParentConstraint 绑到指定骨骼上，并收集模型上的所有 Collider。默认这些 Collider 是关闭的，只有技能 Timeline 在攻击帧才会通过 ColliderControlTrack 打开，防止走路时武器乱蹭撞到人。

## 7. 武器 UI 的 MVC
武器 UI 这块我用了 MVC 思想，并且做了两层：一层是武器类型选择，一层是当前类型下的武器列表。

武器类型这一层： WeaponTypeModel 存武器类型列表和当前索引， WeaponTypeUIView 负责用 GridLayoutGroup 动态生成类型按钮， WeaponTypeController 监听按钮点击，切换类型并派发事件。

武器列表这一层： WeaponItemModel 用一个字典按武器类型分组存玩家持有的武器， WeaponItemUIView 负责生成武器格子、设置图标、名字、等级、点击高亮， WeaponItemController 监听武器类型变化和武器列表变化事件，收到事件后刷新对应格子。

两层之间的通信靠事件。我写了一个 EventManager ，用泛型字典按事件类型存委托，支持订阅和派发。切换武器类型时派发 WeaponTypeChangedEvent ，武器列表变化时派发 WeaponListChangedEvent 。Model 派发，Controller 监听，View 只管显示。这样数据、控制、显示三层互不耦合，加新武器类型或改 UI 表现都不会牵一发动全身。

## 8. 动画状态机
动画我用了两层控制。底层是 Unity 的 Animator Controller，玩家用 X Bot Animator Controller，敌人用 Mutant Animator Controller，里面配了各种动作状态和过渡条件。

上层是脚本控制。 AnimatorParams 是一个静态类，把所有动画参数名用 Animator.StringToHash 转成整型 ID，比如 MoveX、MoveY、Attack、IsCrouching、IdleJump、RunJump、Death、IsHit、HitDirection、IsReadyToAttack、IsEnemyMoving 等等。代码里统一用这些哈希去设参数，避免字符串拼错和性能开销。

我还在 Animator 里挂了几个 StateMachineBehaviour ，比如 Combat 、 IdleState 、 ReadyAttack 。它们在动画进入或离开某个状态时执行逻辑，比如在攻击动画的特定帧通过事件或时间窗口通知技能系统"现在可以判定命中了"或"现在可以接下一段连击了"。这样动画和技能系统的时间点就同步了，不需要在 Update 里硬算时间。

## 9. UI 与登录
UI 分两块。登录场景里， LoginController 挂在 Canvas 上，监听登录按钮点击，读账号和密码输入框，非空就 SceneManager.LoadScene 进主场景，为空就打一条警告。登录界面用了 TMP 输入框和按钮，EventSystem 用的是 InputSystem 的 UI 输入模块，所以手柄也能操作 UI。

主场景里， HPBar 是玩家血条，绑定一个 CharacterUnit，每帧更新 Slider 和文字； EnemyHPBar_Screen 是敌人屏幕空间血条，会把敌人世界位置投影到屏幕坐标，加一个偏移，离屏还可以隐藏。武器 UI 前面讲过了。这些 UI 都继承自我写的 BasePanel ， UIManager 按 bot、mid、top、system 四层管理面板的显示层级，复用 Canvas 和 EventSystem。

## 10. 存档与数据持久化
WeaponSaveManager 负责把玩家持有的武器存成 JSON。存档路径是 persistentDataPath/weapon_save.json 。保存时从 WeaponItemModel 拿到所有武器，转成 WeaponSaveData 这个 DTO，只存 ID、名字、类型、等级、当前耐久、额外加成这些动态字段，模板数据不存。加载时读 JSON，按 ID 查 WeaponCatalog，再从 Resources 加载三个模板，重新拼成运行时实例。我还提供了 LoadDefaultWeapons ，用于首次启动或重置时从 Catalog 加载默认武器。

## 11. 公共基础框架
最后说一下底座。 ProjectBase 这个文件夹放的是通用框架。 BaseManager 是非 MonoBehaviour 单例基类， SingletonMono 和 SingletonAutoMono 是两种 MonoBehaviour 单例。 EventCenter 和 EventManager 是事件中心，一个用字符串 key，一个用泛型类型 key。 MonoMgr 把 Update 事件化，让非 MonoBehaviour 类也能拿到每帧回调。 PoolMgr 是对象池， ResMgr 是资源加载， MusicMgr 管背景音乐和音效， ScenesMgr 封场景切换， InputMgr 管旧输入开关。还有一套 AStar 网格寻路，虽然这个 Demo 里主要用的是 NavMesh，但 A* 实现也留着了。这些框架让上层模块不用每次都自己写单例、事件、对象池，直接拿就行。

## 12. 结尾
整体下来，这个 Demo 的核心思路就是数据驱动加状态机。技能、武器、效果都是 ScriptableObject 配出来的，角色行为是状态机跑出来的，UI 是 MVC 拆出来的。这样虽然代码量不少，但每个模块的边界很清楚，加新技能、新武器、新敌人状态都只需要配资产和写少量代码。我觉得这种结构对一个 ARPG 来说是比较舒服的扩展方式，后续做 Boss、做装备词条、做技能树都能接着这套走。感谢大家看到这里，我们下期见。
