using UnityEngine;

public enum GearSlot
{
    Weapon,
    Armor,
    Accessory1,
    Accessory2
}

[CreateAssetMenu(fileName = "GearItemData", menuName = "Game/Gear Item Data")]
public class GearItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public GearSlot slot;
    public float attackBonus;
    public float hpBonus;
    public float moveSpeedBonus;
    public string description;
}
