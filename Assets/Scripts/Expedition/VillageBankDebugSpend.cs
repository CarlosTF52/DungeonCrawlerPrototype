using UnityEngine;

public class VillageBankDebugSpend : MonoBehaviour
{
    [SerializeField] private int goldCost = 25;
    [SerializeField] private int relicCost;
    [SerializeField] private string successMessage = "Upgrade purchased.";
    [SerializeField] private string failureMessage = "Not enough resources.";

    public void TryPurchase()
    {
        if (VillageBank.Instance.TrySpend(goldCost, relicCost))
        {
            Debug.Log(successMessage, this);
            return;
        }

        Debug.LogWarning(failureMessage, this);
    }
}
