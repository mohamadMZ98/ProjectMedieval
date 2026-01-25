using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Data", fileName = "EnemyData_")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string id = "Enemy";

    [Header("Visuals")]
    public Sprite sprite;

    [Header("Stats")]
    public float maxHP = 10f;
    public float attackDamage = 5f;
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;
    public float attackInterval = 1.0f;

    [Header("Rewards")]
    public float xpReward = 5f;
}
