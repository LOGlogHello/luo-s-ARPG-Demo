using Luo.Character;
using Luo.Character.Controller;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luo.Skill
{


    [Serializable]
    public class SkillController
    {
        //public SkillDataSO data;

    }

    [Serializable]
    public class ActiveSkillController
    {
        // ===== 配置数据（由策划在 Inspector 中拖拽） =====
        public ActiveSkillDataSO data;

        [Header("满足任何一个，即会执行")]
        public List<ActiveSkillStateBinding> stateBindings = new List<ActiveSkillStateBinding>();
    }
}