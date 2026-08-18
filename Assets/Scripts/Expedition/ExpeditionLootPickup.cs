using UnityEngine;

public class ExpeditionLootPickup : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int goldValue = 10;
    [SerializeField] private int relicValue;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private bool logPickupEvents = true;

    private bool hasBeenCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            if (logPickupEvents)
            {
                Debug.Log($"Loot pickup ignored collider tagged '{other.tag}'. Expected '{playerTag}'.", this);
            }

            return;
        }

        ExpeditionRunManager manager = ExpeditionRunManager.Instance;
        bool collectedGold = manager.AddGold(goldValue);
        bool collectedRelics = manager.AddRelics(relicValue);

        if (!collectedGold && !collectedRelics)
        {
            return;
        }

        hasBeenCollected = true;

        if (logPickupEvents)
        {
            Debug.Log($"Collected loot: {goldValue} gold, {relicValue} relics.", this);
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}
