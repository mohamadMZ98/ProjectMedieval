using UnityEngine;

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "UnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    public string id;
    public string displayName;
    public float maxHP = 50f;
    public float attackDamage = 8f;
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackInterval = 1.0f;
    public Rarity rarity = Rarity.Common;
}
