using TMPro;
using UnityEngine;

public class CurrencyHud : MonoBehaviour
{
    private enum CurrencySource
    {
        CurrentExpedition,
        VillageBank
    }

    [SerializeField] private CurrencySource source = CurrencySource.CurrentExpedition;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text relicsText;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private string goldPrefix = "Gold: ";
    [SerializeField] private string relicsPrefix = "Relics: ";
    [SerializeField] private bool hideWhenNoExpedition;

    private ExpeditionRunManager expeditionRunManager;
    private VillageBank villageBank;

    private void OnEnable()
    {
        expeditionRunManager = ExpeditionRunManager.Instance;
        villageBank = VillageBank.Instance;

        expeditionRunManager.StateChanged += Refresh;
        villageBank.BalanceChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (expeditionRunManager != null)
        {
            expeditionRunManager.StateChanged -= Refresh;
        }

        if (villageBank != null)
        {
            villageBank.BalanceChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        int gold = 0;
        int relics = 0;
        bool shouldShow = true;

        if (source == CurrencySource.CurrentExpedition)
        {
            if (expeditionRunManager != null)
            {
                gold = expeditionRunManager.GoldCollected;
                relics = expeditionRunManager.RelicsCollected;
                shouldShow = !hideWhenNoExpedition || expeditionRunManager.IsInExpedition;
            }
        }
        else if (villageBank != null)
        {
            gold = villageBank.Gold;
            relics = villageBank.Relics;
        }

        if (goldText != null)
        {
            goldText.text = $"{goldPrefix}{gold}";
        }

        if (relicsText != null)
        {
            relicsText.text = $"{relicsPrefix}{relics}";
        }

        if (contentRoot != null)
        {
            contentRoot.SetActive(shouldShow);
        }
    }
}
