using System;
using UnityEngine;

public class ExpeditionRunManager : MonoBehaviour
{
    private static ExpeditionRunManager instance;

    [Header("Scene Routing")]
    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private string dungeonSceneName = "Dungeon";
    [SerializeField] private string hubSpawnId = "ExpeditionReturn";
    [SerializeField] private string dungeonSpawnId = "Default";

    [Header("Run Rules")]
    [SerializeField] private int objectivesRequiredToExtract = 3;
    [SerializeField] private bool allowEarlyExtraction = true;
    [SerializeField] private bool beginExpeditionOnStartForTesting;

    public static ExpeditionRunManager Instance
    {
        get
        {
            EnsureInstanceExists();
            return instance;
        }
    }

    public event Action StateChanged;

    public bool IsInExpedition { get; private set; }
    public int RunNumber { get; private set; }
    public int Depth { get; private set; }
    public int GoldCollected { get; private set; }
    public int RelicsCollected { get; private set; }
    public int ObjectiveProgress { get; private set; }
    public int ObjectivesRequiredToExtract => Mathf.Max(1, objectivesRequiredToExtract);
    public ExpeditionOutcome LastOutcome { get; private set; }
    public int LastBankedGold { get; private set; }
    public int LastBankedRelics { get; private set; }

    public bool CanExtract => IsInExpedition && (allowEarlyExtraction || ObjectiveProgress >= ObjectivesRequiredToExtract);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (beginExpeditionOnStartForTesting && !IsInExpedition)
        {
            BeginExpedition(false);
        }
    }

    public void BeginExpedition()
    {
        BeginExpedition(true);
    }

    public void BeginExpedition(bool loadDungeonScene)
    {
        RunNumber++;
        IsInExpedition = true;
        Depth = 1;
        GoldCollected = 0;
        RelicsCollected = 0;
        ObjectiveProgress = 0;
        LastOutcome = ExpeditionOutcome.None;
        LastBankedGold = 0;
        LastBankedRelics = 0;

        NotifyStateChanged();

        if (loadDungeonScene)
        {
            LoadConfiguredScene(dungeonSceneName, dungeonSpawnId);
        }
    }

    public void DescendToNextDepth()
    {
        if (!IsInExpedition)
        {
            BeginExpedition();
            return;
        }

        Depth++;
        NotifyStateChanged();
        LoadConfiguredScene(dungeonSceneName, dungeonSpawnId);
    }

    public void Extract()
    {
        if (!IsInExpedition)
        {
            return;
        }

        if (!CanExtract)
        {
            Debug.LogWarning("Extraction blocked: expedition objective is not complete.", this);
            return;
        }

        BankCollectedLoot();
        EndExpedition(ExpeditionOutcome.Extracted);
    }

    public void FailExpedition()
    {
        if (!IsInExpedition)
        {
            return;
        }

        GoldCollected = 0;
        RelicsCollected = 0;
        EndExpedition(ExpeditionOutcome.Failed);
    }

    public void AbandonExpedition()
    {
        if (!IsInExpedition)
        {
            return;
        }

        EndExpedition(ExpeditionOutcome.Abandoned);
    }

    public bool AddGold(int amount)
    {
        if (!IsInExpedition)
        {
            Debug.LogWarning("Ignored gold pickup because no expedition is active.", this);
            return false;
        }

        if (amount <= 0)
        {
            return false;
        }

        GoldCollected += amount;
        NotifyStateChanged();
        return true;
    }

    public bool AddRelics(int amount)
    {
        if (!IsInExpedition)
        {
            Debug.LogWarning("Ignored relic pickup because no expedition is active.", this);
            return false;
        }

        if (amount <= 0)
        {
            return false;
        }

        RelicsCollected += amount;
        NotifyStateChanged();
        return true;
    }

    public bool AddObjectiveProgress(int amount)
    {
        if (!IsInExpedition)
        {
            Debug.LogWarning("Ignored objective progress because no expedition is active.", this);
            return false;
        }

        if (amount <= 0)
        {
            return false;
        }

        ObjectiveProgress = Mathf.Min(ObjectiveProgress + amount, ObjectivesRequiredToExtract);
        NotifyStateChanged();
        return true;
    }

    private void EndExpedition(ExpeditionOutcome outcome)
    {
        IsInExpedition = false;
        LastOutcome = outcome;

        NotifyStateChanged();
        LoadConfiguredScene(hubSceneName, hubSpawnId);
    }

    private void BankCollectedLoot()
    {
        LastBankedGold = GoldCollected;
        LastBankedRelics = RelicsCollected;
        VillageBank.Instance.Deposit(LastBankedGold, LastBankedRelics);
    }

    private void LoadConfiguredScene(string sceneName, string spawnId)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Expedition scene route is missing a scene name.", this);
            return;
        }

        SceneTransitionManager.LoadScene(sceneName, spawnId);
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("ExpeditionRunManager");
        managerObject.AddComponent<ExpeditionRunManager>();
    }
}
