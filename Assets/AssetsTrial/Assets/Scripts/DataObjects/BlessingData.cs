using UnityEngine;

[CreateAssetMenu(fileName = "BlessingData", menuName = "Game/Blessing Data")]
public class BlessingData : ScriptableObject
{
    public string id;
    public string displayName;
    public string description;
    public int maxLevel = 5;
}
