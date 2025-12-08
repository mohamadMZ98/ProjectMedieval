using System.Collections.Generic;
using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    [SerializeField] private ArmyUnit armyUnitPrefab;
    [SerializeField] private List<UnitData> startingUnits = new List<UnitData>();
    [SerializeField] private int maxUnits = 10;

    private readonly List<ArmyUnit> activeUnits = new List<ArmyUnit>();

    public IReadOnlyList<ArmyUnit> ActiveUnits => activeUnits;

public void InitializeArmy(PlayerController hero)
{
    ClearArmy();

    if (armyUnitPrefab == null || hero == null)
    {
        Debug.LogWarning("ArmyManager: Prefab or hero is missing.");
        return;
    }

    int count = Mathf.Min(startingUnits.Count, maxUnits);
    if (count <= 0) return;

    float radius = 1.5f;

    for (int i = 0; i < count; i++)
    {
        // Compute a unique offset on a circle around the hero
        float angle = (Mathf.PI * 2f / count) * i;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        Vector3 spawnPos = hero.transform.position + new Vector3(offset.x, offset.y, 0f);

        ArmyUnit unit = Object.Instantiate(armyUnitPrefab, spawnPos, Quaternion.identity, transform);
        // Pass UnitData + formation offset
        unit.Initialize(startingUnits[i], offset);
        activeUnits.Add(unit);
    }
}

    public void ClearArmy()
    {
        for (int i = 0; i < activeUnits.Count; i++)
        {
            ArmyUnit unit = activeUnits[i];
            if (unit != null)
            {
                Object.Destroy(unit.gameObject);
            }
        }

        activeUnits.Clear();
    }
}
