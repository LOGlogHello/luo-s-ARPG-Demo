// ===== ScriptableObject 数据层 =====
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Combat/Character Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("基础数值")]
    public float maxHealth = 100f;
    public float maxMana = 50f;
    public float attack = 20f;
    public float defense = 10f;
    public float moveSpeed = 5f;
    public float healMultipler=1f;

    [Header("状态初始值")]
    public bool isDead = false;
    public bool isStunned = false;
    public bool isInvincible = false;
}