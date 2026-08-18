using System;
using UnityEngine;

public class VillageBank : MonoBehaviour
{
    private static VillageBank instance;

    [Header("Starting Resources")]
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

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Gold = Mathf.Max(0, startingGold);
        Relics = Mathf.Max(0, startingRelics);
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
            BalanceChanged?.Invoke();
        }
    }

    public bool CanAfford(int goldCost, int relicCost)
    {
        return Gold >= Mathf.Max(0, goldCost) && Relics >= Mathf.Max(0, relicCost);
    }

    public bool TrySpend(int goldCost, int relicCost)
    {
        goldCost = Mathf.Max(0, goldCost);
        relicCost = Mathf.Max(0, relicCost);

        if (!CanAfford(goldCost, relicCost))
        {
            return false;
        }

        Gold -= goldCost;
        Relics -= relicCost;
        BalanceChanged?.Invoke();
        return true;
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        GameObject bankObject = new GameObject("VillageBank");
        bankObject.AddComponent<VillageBank>();
    }
}
