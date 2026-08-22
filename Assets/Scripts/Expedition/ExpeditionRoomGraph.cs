using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ExpeditionRoomType
{
    Entrance,
    Combat,
    Loot,
    Objective,
    Extraction
}

[Serializable]
public class ExpeditionRoomDefinition
{
    [SerializeField] private ExpeditionRoomType roomType = ExpeditionRoomType.Combat;
    [SerializeField, Range(0, 5)] private int dangerRating = 1;
    [SerializeField] private int enemyCount = 1;
    [SerializeField] private int pickupCount;
    [SerializeField] private int trapCount;
    [SerializeField] private bool hasObjective;
    [SerializeField] private bool hasExtraction;

    public ExpeditionRoomDefinition()
    {
    }

    public ExpeditionRoomDefinition(
        ExpeditionRoomType roomType,
        int dangerRating,
        int enemyCount,
        int pickupCount,
        int trapCount,
        bool hasObjective = false,
        bool hasExtraction = false)
    {
        this.roomType = roomType;
        this.dangerRating = dangerRating;
        this.enemyCount = enemyCount;
        this.pickupCount = pickupCount;
        this.trapCount = trapCount;
        this.hasObjective = hasObjective;
        this.hasExtraction = hasExtraction;
    }

    public ExpeditionRoomType RoomType => roomType;
    public int DangerRating => Mathf.Clamp(dangerRating, 0, 5);
    public int EnemyCount => Mathf.Max(0, enemyCount);
    public int PickupCount => Mathf.Max(0, pickupCount);
    public int TrapCount => Mathf.Max(0, trapCount);
    public bool HasObjective => hasObjective || roomType == ExpeditionRoomType.Objective;
    public bool HasExtraction => hasExtraction || roomType == ExpeditionRoomType.Extraction;
}

[Serializable]
public class ExpeditionRoomNode
{
    [SerializeField] private int id;
    [SerializeField] private ExpeditionRoomType roomType;
    [SerializeField] private int dangerRating;
    [SerializeField] private int enemyCount;
    [SerializeField] private int pickupCount;
    [SerializeField] private int trapCount;
    [SerializeField] private bool hasObjective;
    [SerializeField] private bool hasExtraction;
    [SerializeField] private List<int> connectedNodeIds = new List<int>();

    public ExpeditionRoomNode(
        int id,
        ExpeditionRoomType roomType,
        int dangerRating,
        int enemyCount,
        int pickupCount,
        int trapCount,
        bool hasObjective,
        bool hasExtraction)
    {
        this.id = id;
        this.roomType = roomType;
        this.dangerRating = dangerRating;
        this.enemyCount = Mathf.Max(0, enemyCount);
        this.pickupCount = Mathf.Max(0, pickupCount);
        this.trapCount = Mathf.Max(0, trapCount);
        this.hasObjective = hasObjective;
        this.hasExtraction = hasExtraction;
    }

    public ExpeditionRoomNode(int id, ExpeditionRoomDefinition definition)
        : this(
            id,
            definition.RoomType,
            definition.DangerRating,
            definition.EnemyCount,
            definition.PickupCount,
            definition.TrapCount,
            definition.HasObjective,
            definition.HasExtraction)
    {
    }

    public int Id => id;
    public ExpeditionRoomType RoomType => roomType;
    public int DangerRating => dangerRating;
    public int EnemyCount => enemyCount;
    public int PickupCount => pickupCount;
    public int TrapCount => trapCount;
    public bool HasObjective => hasObjective;
    public bool HasExtraction => hasExtraction;
    public IReadOnlyList<int> ConnectedNodeIds => connectedNodeIds;
    public string ContentsLabel => $"Enemies {enemyCount}  Pickups {pickupCount}  Traps {trapCount}";

    public void ConnectTo(int nodeId)
    {
        if (!connectedNodeIds.Contains(nodeId))
        {
            connectedNodeIds.Add(nodeId);
        }
    }
}

[Serializable]
public class ExpeditionRoomGraph
{
    [SerializeField] private List<ExpeditionRoomNode> nodes = new List<ExpeditionRoomNode>();
    [SerializeField] private List<int> objectivePathNodeIds = new List<int>();

    public IReadOnlyList<ExpeditionRoomNode> Nodes => nodes;
    public IReadOnlyList<int> ObjectivePathNodeIds => objectivePathNodeIds;

    public ExpeditionRoomNode Entrance => nodes.Count > 0 ? nodes[0] : null;
    public ExpeditionRoomNode Extraction => nodes.Count > 0 ? nodes[nodes.Count - 1] : null;

    public static ExpeditionRoomGraph Generate(int runNumber, int objectivesRequired)
    {
        ExpeditionRoomGraph graph = new ExpeditionRoomGraph();
        System.Random random = new System.Random(Environment.TickCount ^ (runNumber * 397));
        int objectiveCount = Mathf.Max(1, objectivesRequired);

        graph.AddPathNode(ExpeditionRoomType.Entrance, 0, random, 0);

        int roomCountBeforeExtraction = Mathf.Clamp(objectiveCount + 3, 5, 8);
        bool hasLoot = false;
        bool hasCombat = false;

        for (int i = 1; i < roomCountBeforeExtraction; i++)
        {
            ExpeditionRoomType roomType;

            if (i <= objectiveCount)
            {
                roomType = ExpeditionRoomType.Objective;
            }
            else if (!hasLoot)
            {
                roomType = ExpeditionRoomType.Loot;
            }
            else if (!hasCombat)
            {
                roomType = ExpeditionRoomType.Combat;
            }
            else
            {
                roomType = random.NextDouble() < 0.65d ? ExpeditionRoomType.Combat : ExpeditionRoomType.Loot;
            }

            hasLoot |= roomType == ExpeditionRoomType.Loot;
            hasCombat |= roomType == ExpeditionRoomType.Combat;
            graph.AddPathNode(roomType, CalculateDanger(roomType, i, random), random, i);
        }

        graph.AddPathNode(ExpeditionRoomType.Extraction, 1, random, roomCountBeforeExtraction);
        graph.AddOptionalBranches(random);

        return graph;
    }

    public static ExpeditionRoomGraph CreateEntranceOnly(int runNumber)
    {
        ExpeditionRoomGraph graph = new ExpeditionRoomGraph();
        System.Random random = new System.Random(Environment.TickCount ^ (runNumber * 397));

        graph.AddPathNode(ExpeditionRoomType.Entrance, 0, random, 0);
        return graph;
    }

    public static ExpeditionRoomGraph BuildFromRoute(IReadOnlyList<ExpeditionRoomDefinition> roomRoute)
    {
        ExpeditionRoomGraph graph = new ExpeditionRoomGraph();

        if (roomRoute == null || roomRoute.Count == 0)
        {
            return graph;
        }

        for (int i = 0; i < roomRoute.Count; i++)
        {
            if (roomRoute[i] == null)
            {
                continue;
            }

            graph.AddPathNode(roomRoute[i]);
        }

        return graph;
    }

    public ExpeditionRoomNode GetNode(int nodeId)
    {
        return nodes.Find(node => node.Id == nodeId);
    }

    public ExpeditionRoomNode GetObjectivePathNodeAt(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex >= objectivePathNodeIds.Count)
        {
            return null;
        }

        return GetNode(objectivePathNodeIds[pathIndex]);
    }

    public int GetObjectivePathIndex(int nodeId)
    {
        return objectivePathNodeIds.IndexOf(nodeId);
    }

    public ExpeditionRoomNode AppendGeneratedPathRoom(ExpeditionRoomType roomType, int dangerRating, int randomSeed, int depth)
    {
        System.Random random = new System.Random(randomSeed);
        return AddPathNode(roomType, dangerRating, random, depth);
    }

    public string BuildObjectivePathLabel(int currentNodeId)
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < objectivePathNodeIds.Count; i++)
        {
            ExpeditionRoomNode node = GetNode(objectivePathNodeIds[i]);

            if (node == null)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(" > ");
            }

            if (node.Id == currentNodeId)
            {
                builder.Append("[");
            }

            builder.Append(node.RoomType);

            if (node.Id == currentNodeId)
            {
                builder.Append("]");
            }
        }

        return builder.ToString();
    }

    private ExpeditionRoomNode AddPathNode(ExpeditionRoomType roomType, int dangerRating, System.Random random, int depth)
    {
        ExpeditionRoomNode node = AddNode(roomType, dangerRating, random, depth);
        objectivePathNodeIds.Add(node.Id);

        if (objectivePathNodeIds.Count > 1)
        {
            int previousNodeId = objectivePathNodeIds[objectivePathNodeIds.Count - 2];
            Connect(previousNodeId, node.Id);
        }

        return node;
    }

    private ExpeditionRoomNode AddPathNode(ExpeditionRoomDefinition definition)
    {
        ExpeditionRoomNode node = new ExpeditionRoomNode(nodes.Count, definition);
        nodes.Add(node);
        objectivePathNodeIds.Add(node.Id);

        if (objectivePathNodeIds.Count > 1)
        {
            int previousNodeId = objectivePathNodeIds[objectivePathNodeIds.Count - 2];
            Connect(previousNodeId, node.Id);
        }

        return node;
    }

    private ExpeditionRoomNode AddNode(ExpeditionRoomType roomType, int dangerRating, System.Random random, int depth)
    {
        int clampedDanger = Mathf.Clamp(dangerRating, 0, 5);
        int enemyCount = RollEnemyCount(roomType, clampedDanger, random);
        int pickupCount = RollPickupCount(roomType, random);
        int trapCount = RollTrapCount(roomType, clampedDanger, random);
        bool hasObjective = roomType == ExpeditionRoomType.Objective;
        bool hasExtraction = roomType == ExpeditionRoomType.Extraction;

        if (roomType == ExpeditionRoomType.Entrance)
        {
            enemyCount = 0;
            trapCount = 0;
            pickupCount = random.NextDouble() < 0.35d ? 1 : 0;
        }

        if (roomType == ExpeditionRoomType.Extraction)
        {
            pickupCount = Mathf.Max(pickupCount, 1);
            trapCount = Mathf.Min(trapCount, 1);
        }

        ExpeditionRoomNode node = new ExpeditionRoomNode(
            nodes.Count,
            roomType,
            clampedDanger,
            enemyCount,
            pickupCount,
            trapCount,
            hasObjective,
            hasExtraction);

        nodes.Add(node);
        return node;
    }

    private void Connect(int firstNodeId, int secondNodeId)
    {
        ExpeditionRoomNode firstNode = GetNode(firstNodeId);
        ExpeditionRoomNode secondNode = GetNode(secondNodeId);

        if (firstNode == null || secondNode == null)
        {
            return;
        }

        firstNode.ConnectTo(secondNodeId);
        secondNode.ConnectTo(firstNodeId);
    }

    private void AddOptionalBranches(System.Random random)
    {
        int branchCount = random.Next(1, 3);

        for (int i = 0; i < branchCount; i++)
        {
            int parentPathIndex = random.Next(1, Mathf.Max(2, objectivePathNodeIds.Count - 1));
            ExpeditionRoomType roomType = random.NextDouble() < 0.5d ? ExpeditionRoomType.Combat : ExpeditionRoomType.Loot;
            ExpeditionRoomNode branchNode = AddNode(roomType, CalculateDanger(roomType, parentPathIndex + 1, random), random, parentPathIndex + 1);

            Connect(objectivePathNodeIds[parentPathIndex], branchNode.Id);
        }
    }

    private static int CalculateDanger(ExpeditionRoomType roomType, int depth, System.Random random)
    {
        int baseDanger = Mathf.Clamp(1 + (depth / 2), 1, 4);

        switch (roomType)
        {
            case ExpeditionRoomType.Entrance:
                return 0;
            case ExpeditionRoomType.Loot:
                return Mathf.Max(1, baseDanger - 1);
            case ExpeditionRoomType.Objective:
                return Mathf.Clamp(baseDanger + 1, 2, 5);
            case ExpeditionRoomType.Extraction:
                return 1;
            default:
                return Mathf.Clamp(baseDanger + random.Next(0, 2), 1, 5);
        }
    }

    private static int RollEnemyCount(ExpeditionRoomType roomType, int dangerRating, System.Random random)
    {
        switch (roomType)
        {
            case ExpeditionRoomType.Loot:
                return random.NextDouble() < 0.45d ? 1 : 0;
            case ExpeditionRoomType.Objective:
                return Mathf.Max(1, dangerRating + random.Next(0, 2));
            case ExpeditionRoomType.Extraction:
                return random.NextDouble() < 0.5d ? 1 : 0;
            default:
                return Mathf.Max(1, dangerRating + random.Next(0, 3));
        }
    }

    private static int RollPickupCount(ExpeditionRoomType roomType, System.Random random)
    {
        switch (roomType)
        {
            case ExpeditionRoomType.Combat:
                return random.NextDouble() < 0.4d ? 1 : 0;
            case ExpeditionRoomType.Objective:
                return random.NextDouble() < 0.5d ? 1 : 0;
            case ExpeditionRoomType.Extraction:
                return random.Next(1, 3);
            default:
                return random.Next(1, 4);
        }
    }

    private static int RollTrapCount(ExpeditionRoomType roomType, int dangerRating, System.Random random)
    {
        if (roomType == ExpeditionRoomType.Entrance)
        {
            return 0;
        }

        double trapChance = roomType == ExpeditionRoomType.Loot ? 0.55d : 0.25d + (dangerRating * 0.08d);

        if (random.NextDouble() > trapChance)
        {
            return 0;
        }

        return dangerRating >= 4 && random.NextDouble() < 0.35d ? 2 : 1;
    }
}
