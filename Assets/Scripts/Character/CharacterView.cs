using Luo.Character.Controller;
using UnityEngine;

namespace Luo.Character
{
    public class CharacterView : MonoBehaviour
    {
        //[Tooltip("管理当前CharacterView的角色控制器")]
        //private BaseController _controller;

        [Header("是否禁用子模型上的animator")]
        public bool isDisableChildAnimator = true;
        private Animator _animator;

        public GameObject modelObject;
        [SerializeField] private bool autoFindModel = true;

        private void Awake()
        {
            
            // 2. 查找子物体模型
            Transform modelTransform = null;
            if (modelObject != null) modelTransform = modelObject.transform;
            else if (autoFindModel)
            {
                foreach (Transform child in transform)
                    if (child.TryGetComponent<Animator>(out _)) { modelTransform = child; break; }
            }

            if (modelTransform == null) { Debug.LogError("未找到模型"); return; }

            Animator modelAnimator = modelTransform.GetComponent<Animator>();

           

            if (isDisableChildAnimator)
            {
                // 1. 确保根节点有 Animator
                Animator rootAnimator = GetComponent<Animator>();

                if (rootAnimator == null) rootAnimator = gameObject.AddComponent<Animator>();
                _animator = rootAnimator;


                // 3. 迁移控制器和 Avatar
                rootAnimator.runtimeAnimatorController = modelAnimator.runtimeAnimatorController;
                rootAnimator.avatar = modelAnimator.avatar;
                rootAnimator.applyRootMotion = modelAnimator.applyRootMotion;

                // 4. 禁用子模型的 Animator（关键操作）
                modelAnimator.enabled = false;
 
            }
            else
            {
                _animator=modelAnimator;
            }
                

            // 5. 归零模型坐标
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = Quaternion.identity;

            // 注意：这里不处理任何移动逻辑！


        }


        public Animator mAnimator=> _animator;
    }

}


