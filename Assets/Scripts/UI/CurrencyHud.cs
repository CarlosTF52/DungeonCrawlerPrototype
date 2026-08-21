using TMPro;
using UnityEngine;

public class CurrencyHud : MonoBehaviour
{
    private enum CurrencySource
    {
        CurrentExpedition,
        PlayerPouch,
        VillageBank
    }

    [SerializeField] private CurrencySource source = CurrencySource.PlayerPouch;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text relicsText;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private string goldPrefix = "Gold: ";
    [SerializeField] private string relicsPrefix = "Relics: ";
    [SerializeField] private bool hideWhenNoExpedition;

    private ExpeditionRunManager expeditionRunManager;
    private PlayerCurrencyPouch playerCurrencyPouch;
    private VillageBank villageBank;
    private int displayedGold = int.MinValue;
    private int displayedRelics = int.MinValue;
    private bool displayedVisibility = true;

    private void OnEnable()
    {
        expeditionRunManager = ExpeditionRunManager.Instance;
        playerCurrencyPouch = PlayerCurrencyPouch.Instance;
        villageBank = VillageBank.Instance;

        expeditionRunManager.StateChanged += Refresh;
        playerCurrencyPouch.BalanceChanged += Refresh;
        villageBank.BalanceChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (expeditionRunManager != null)
        {
            expeditionRunManager.StateChanged -= Refresh;
        }

        if (playerCurrencyPouch != null)
        {
            playerCurrencyPouch.BalanceChanged -= Refresh;
        }

        if (villageBank != null)
        {
            villageBank.BalanceChanged -= Refresh;
        }
    }

    private void LateUpdate()
    {
        Refresh();
    }

    public void Refresh()
    {
        GetCurrentValues(out int gold, out int relics, out bool shouldShow);

        if (gold == displayedGold && relics == displayedRelics && shouldShow == displayedVisibility)
        {
            return;
        }

        displayedGold = gold;
        displayedRelics = relics;
        displayedVisibility = shouldShow;

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

    private void GetCurrentValues(out int gold, out int relics, out bool shouldShow)
    {
        gold = 0;
        relics = 0;
        shouldShow = true;

        switch (source)
        {
            case CurrencySource.CurrentExpedition:
                if (expeditionRunManager != null)
                {
                    gold = expeditionRunManager.GoldCollected;
                    relics = expeditionRunManager.RelicsCollected;
                    shouldShow = !hideWhenNoExpedition || expeditionRunManager.IsInExpedition;
                }
                break;
            case CurrencySource.PlayerPouch:
                if (playerCurrencyPouch != null)
                {
                    gold = playerCurrencyPouch.Gold;
                    relics = playerCurrencyPouch.Relics;
                }
                break;
            case CurrencySource.VillageBank:
                if (villageBank != null)
                {
                    gold = villageBank.Gold;
                    relics = villageBank.Relics;
                }
                break;
        }
    }
}
