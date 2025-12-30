using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string id;
    public string displayName;
    public float maxHP = 10f;
    public float attackDamage = 5f;
    public float moveSpeed = 2f;
    public float attackInterval = 1f;
    public float xpReward = 10f;
}
