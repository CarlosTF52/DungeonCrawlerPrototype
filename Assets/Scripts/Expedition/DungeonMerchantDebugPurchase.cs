using UnityEngine;
using UnityEngine.Events;

public class DungeonMerchantDebugPurchase : MonoBehaviour
{
    [SerializeField] private int goldCost = 10;
    [SerializeField] private int relicCost;
    [SerializeField] private string successMessage = "Merchant purchase complete.";
    [SerializeField] private string failureMessage = "Not enough carried currency.";

    [Header("Events")]
    [SerializeField] private UnityEvent purchased;

    private void OnValidate()
    {
        goldCost = Mathf.Max(0, goldCost);
        relicCost = Mathf.Max(0, relicCost);
    }

    public void TryPurchase()
    {
        if (PlayerCurrencyPouch.Instance.TrySpend(goldCost, relicCost))
        {
            purchased?.Invoke();
            Debug.Log(successMessage, this);
            return;
        }

        Debug.LogWarning(failureMessage, this);
    }
}
