using System;
using UnityEngine;

public class EntityStatsProvider : MonoBehaviour
{
    [SerializeField] private EntityStats stats;

    public event Action<EntityStats> StatsChanged;

    public EntityStats Stats => stats;

    public void SetStats(EntityStats newStats)
    {
        if (stats == newStats)
        {
            return;
        }

        stats = newStats;
        StatsChanged?.Invoke(stats);
    }
}
