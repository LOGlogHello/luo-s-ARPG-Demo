using UnityEngine;

/// <summary>
/// Animator中Combat_NormaAttack相关的参数的哈希值
/// </summary>
public static class AnimatorParams
{
    public static readonly int AnimSpeed = Animator.StringToHash("AnimSpeed");
    
    public static readonly int IsAbleToChangeToIdle = Animator.StringToHash("IsAbleToChangeToIdle");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int InputInComboWindow = Animator.StringToHash("Combat_NormaAttack_InputInComboWindow");
    public static readonly int InputInRecoveryWindow2 = Animator.StringToHash("Combat_NormaAttack_InputInRecoveryWindow2");
    public static readonly int IsAbleToExit = Animator.StringToHash("Combat_NormaAttack_IsAbleToExit");
    public static readonly int IsAbleToAttack = Animator.StringToHash("IsAbleToAttack");
    public static readonly int MoveX = Animator.StringToHash("MoveX");
    public static readonly int MoveY = Animator.StringToHash("MoveY");
    public static readonly int IsCrouching = Animator.StringToHash("isCrouching");
    public static readonly int IdleJump = Animator.StringToHash("IdleJump");
    public static readonly int RunJump = Animator.StringToHash("RunJump");
    public static readonly int Death = Animator.StringToHash("Death");
    public static readonly int IsHit = Animator.StringToHash("IsHit");
    public static readonly int HitDirection = Animator.StringToHash("HitDirection");
    public static readonly int IsReadyToAttack = Animator.StringToHash("IsReadyToAttack");
    public static readonly int IsEnemyMoving = Animator.StringToHash("IsEnemyMoving");


}