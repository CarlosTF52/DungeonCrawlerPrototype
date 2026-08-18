using UnityEngine;

public class EntityStatsProvider : MonoBehaviour
{
    [SerializeField] private EntityStats stats;

    public EntityStats Stats => stats;
}
