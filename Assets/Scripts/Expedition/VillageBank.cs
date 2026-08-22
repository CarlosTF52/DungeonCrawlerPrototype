using System;
using UnityEngine;

public class VillageBank : MonoBehaviour
{
    private static VillageBank instance;
    private static bool hasLoadedStartingResources;
    private static int savedGold;
    private static int savedRelics;

    [Header("Starting Stored Resources")]
    [SerializeField] private int startingGold;
    [SerializeField] private int startingRelics;

    public static VillageBank Instance
    {
        get
        {
            EnsureInstanceExists();
            return instance;
        }
    }

    public event Action BalanceChanged;

    public int Gold { get; private set; }
    public int Relics { get; private set; }
    public bool HasAnyCurrency => Gold > 0 || Relics > 0;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        instance = null;
        hasLoadedStartingResources = false;
        savedGold = 0;
        savedRelics = 0;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (hasLoadedStartingResources)
        {
            Gold = savedGold;
            Relics = savedRelics;
        }
        else
        {
            Gold = Mathf.Max(0, startingGold);
            Relics = Mathf.Max(0, startingRelics);
            SaveCurrentBalance();
            hasLoadedStartingResources = true;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnValidate()
    {
        startingGold = Mathf.Max(0, startingGold);
        startingRelics = Mathf.Max(0, startingRelics);
    }

    public void Deposit(int gold, int relics)
    {
        bool changed = false;

        if (gold > 0)
        {
            Gold += gold;
            changed = true;
        }

        if (relics > 0)
        {
            Relics += relics;
            changed = true;
        }

        if (changed)
        {
            SaveCurrentBalance();
            BalanceChanged?.Invoke();
        }
    }

    public bool TryDepositFromPouch(int gold, int relics)
    {
        gold = Mathf.Max(0, gold);
        relics = Mathf.Max(0, relics);

        if (gold <= 0 && relics <= 0)
        {
            return false;
        }

        PlayerCurrencyPouch pouch = PlayerCurrencyPouch.Instance;

        if (!pouch.TrySpend(gold, relics))
        {
            return false;
        }

        Deposit(gold, relics);
        return true;
    }

    public bool TryDepositAllFromPouch()
    {
        PlayerCurrencyPouch pouch = PlayerCurrencyPouch.Instance;
        int gold = pouch.Gold;
        int relics = pouch.Relics;

        if (gold <= 0 && relics <= 0)
        {
            return false;
        }

        if (!pouch.TrySpend(gold, relics))
        {
            return false;
        }

        Deposit(gold, relics);
        return true;
    }

    public bool TryWithdrawToPouch(int gold, int relics)
    {
        gold = Mathf.Max(0, gold);
        relics = Mathf.Max(0, relics);

        if (gold <= 0 && relics <= 0)
        {
            return false;
        }

        if (!TrySpend(gold, relics))
        {
            return false;
        }

        PlayerCurrencyPouch.Instance.Add(gold, relics);
        return true;
    }

    public bool TryWithdrawAllToPouch()
    {
        if (!HasAnyCurrency)
        {
            return false;
        }

        int gold = Gold;
        int relics = Relics;

        if (!TrySpend(gold, relics))
        {
            return false;
        }

        PlayerCurrencyPouch.Instance.Add(gold, relics);
        return true;
    }

    public bool CanAfford(int goldCost, int relicCost)
    {
        return Gold >= Mathf.Max(0, goldCost) && Relics >= Mathf.Max(0, relicCost);
    }

    public bool TrySpend(int goldCost, int relicCost)
    {
        goldCost = Mathf.Max(0, goldCost);
        relicCost = Mathf.Max(0, relicCost);

        if (goldCost <= 0 && relicCost <= 0)
        {
            return false;
        }

        if (!CanAfford(goldCost, relicCost))
        {
            return false;
        }

        Gold -= goldCost;
        Relics -= relicCost;
        SaveCurrentBalance();
        BalanceChanged?.Invoke();
        return true;
    }

    private void SaveCurrentBalance()
    {
        savedGold = Gold;
        savedRelics = Relics;
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        VillageBank sceneBank = FindObjectOfType<VillageBank>();

        if (sceneBank != null)
        {
            instance = sceneBank;
            return;
        }

        GameObject bankObject = new GameObject("VillageBank");
        bankObject.AddComponent<VillageBank>();
    }
}


