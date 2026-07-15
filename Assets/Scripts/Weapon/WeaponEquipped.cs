using UnityEngine;

public class WeaponEquipped : StateMachineBehaviour
{
    // 在切换动画退出的瞬间调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {                            
        animator.SetBool("IsAbleToAttack", true);                                   
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsAbleToAttack", false);
    }
}                                                                       