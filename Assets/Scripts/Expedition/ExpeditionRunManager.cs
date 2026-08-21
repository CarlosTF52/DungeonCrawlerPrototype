using System;
using System.Collections.Generic;
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
    [SerializeField] private bool autoExtractWhenPastExtractionRoom = true;

    [Header("Inspector Route")]
    [SerializeField] private bool useInspectorRoomRoute;
    [SerializeField] private List<ExpeditionRoomDefinition> inspectorRoomRoute = new List<ExpeditionRoomDefinition>();

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
    public int LastExtractedGold { get; private set; }
    public int LastExtractedRelics { get; private set; }
    public ExpeditionRoomGraph RoomGraph { get; private set; }
    public ExpeditionRoomNode CurrentRoom { get; private set; }
    public int CurrentObjectivePathIndex { get; private set; }

    public bool IsInExtractionRoom => CurrentRoom != null && CurrentRoom.HasExtraction;
    public bool CanExtract => IsInExpedition && IsInExtractionRoom && (allowEarlyExtraction || ObjectiveProgress >= ObjectivesRequiredToExtract);
    public bool HasGeneratedRoomGraph => RoomGraph != null && RoomGraph.Nodes.Count > 0;
    public string ObjectivePathLabel => HasGeneratedRoomGraph ? RoomGraph.BuildObjectivePathLabel(CurrentRoom != null ? CurrentRoom.Id : -1) : "No expedition map";
    public string ExtractionStatus
    {
        get
        {
            if (!IsInExpedition)
            {
                return "No active expedition";
            }

            if (!IsInExtractionRoom)
            {
                return "Find extraction";
            }

            return CanExtract ? "Ready" : "Objective incomplete";
        }
    }

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

    private void Reset()
    {
        if (inspectorRoomRoute.Count > 0)
        {
            return;
        }

        inspectorRoomRoute.Add(new ExpeditionRoomDefinition(ExpeditionRoomType.Entrance, 0, 0, 0, 0));
        inspectorRoomRoute.Add(new ExpeditionRoomDefinition(ExpeditionRoomType.Combat, 2, 2, 1, 0));
        inspectorRoomRoute.Add(new ExpeditionRoomDefinition(ExpeditionRoomType.Loot, 1, 1, 2, 1));
        inspectorRoomRoute.Add(new ExpeditionRoomDefinition(ExpeditionRoomType.Objective, 3, 2, 0, 1, true));
        inspectorRoomRoute.Add(new ExpeditionRoomDefinition(ExpeditionRoomType.Extraction, 1, 0, 1, 0, false, true));
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
        LastExtractedGold = 0;
        LastExtractedRelics = 0;
        RoomGraph = CreateRoomGraph();
        CurrentObjectivePathIndex = 0;
        CurrentRoom = RoomGraph.Entrance;

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

        if (!AdvanceToNextObjectivePathRoom())
        {
            if (autoExtractWhenPastExtractionRoom && CanExtract)
            {
                Extract();
            }

            return;
        }

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

        MoveCollectedLootToPouch();
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
        RoomGraph = null;
        CurrentRoom = null;
        CurrentObjectivePathIndex = 0;

        NotifyStateChanged();
        LoadConfiguredScene(hubSceneName, hubSpawnId);
    }

    private void MoveCollectedLootToPouch()
    {
        LastExtractedGold = GoldCollected;
        LastExtractedRelics = RelicsCollected;
        PlayerCurrencyPouch.Instance.Add(LastExtractedGold, LastExtractedRelics);
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

    private bool AdvanceToNextObjectivePathRoom()
    {
        if (!HasGeneratedRoomGraph)
        {
            RoomGraph = CreateRoomGraph();
            CurrentObjectivePathIndex = 0;
            CurrentRoom = RoomGraph.Entrance;
        }

        int nextPathIndex = CurrentObjectivePathIndex + 1;
        ExpeditionRoomNode nextRoom = RoomGraph.GetObjectivePathNodeAt(nextPathIndex);

        if (nextRoom == null)
        {
            return false;
        }

        CurrentObjectivePathIndex = nextPathIndex;
        CurrentRoom = nextRoom;
        Depth = CurrentObjectivePathIndex + 1;
        return true;
    }

    private ExpeditionRoomGraph CreateRoomGraph()
    {
        if (useInspectorRoomRoute)
        {
            ExpeditionRoomGraph inspectorGraph = ExpeditionRoomGraph.BuildFromRoute(inspectorRoomRoute);

            if (inspectorGraph.Nodes.Count > 0)
            {
                return inspectorGraph;
            }

            Debug.LogWarning("Inspector room route is enabled, but no valid rooms are configured. Falling back to generated route.", this);
        }

        return ExpeditionRoomGraph.Generate(RunNumber, ObjectivesRequiredToExtract);
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        ExpeditionRunManager sceneManager = FindObjectOfType<ExpeditionRunManager>();

        if (sceneManager != null)
        {
            instance = sceneManager;
            return;
        }

        GameObject managerObject = new GameObject("ExpeditionRunManager");
        managerObject.AddComponent<ExpeditionRunManager>();
    }
}
