using System;
using UnityEngine;

public class PlayerCurrencyPouch : MonoBehaviour
{
    private static PlayerCurrencyPouch instance;

    [Header("Starting Carried Resources")]
    [SerializeField] private int startingGold;
    [SerializeField] private int startingRelics;

    public static PlayerCurrencyPouch Instance
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

    public void Add(int gold, int relics)
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
        BalanceChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        if (!HasAnyCurrency)
        {
            return;
        }

        Gold = 0;
        Relics = 0;
        BalanceChanged?.Invoke();
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        PlayerCurrencyPouch scenePouch = FindObjectOfType<PlayerCurrencyPouch>();

        if (scenePouch != null)
        {
            instance = scenePouch;
            return;
        }

        GameObject pouchObject = new GameObject("PlayerCurrencyPouch");
        pouchObject.AddComponent<PlayerCurrencyPouch>();
    }
}

