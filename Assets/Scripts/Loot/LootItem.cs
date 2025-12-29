using UnityEngine;

public class LootItem : MonoBehaviour
{
    [SerializeField] private int goldAmount = 0;
    [SerializeField] private GearItemData gearItem;

    public void OnPickedUp(PlayerController hero)
    {
        if (goldAmount > 0 && RunManager.Instance != null)
        {
            RunManager.Instance.AddGold(goldAmount);
        }

        if (gearItem != null)
        {
            Debug.Log($"Picked up gear item: {gearItem.displayName}");
            // TODO: add item to hero inventory or meta progression.
        }

        Destroy(gameObject);
    }
}
