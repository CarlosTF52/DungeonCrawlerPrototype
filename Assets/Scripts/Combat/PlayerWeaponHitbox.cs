using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponHitbox : MonoBehaviour
{
    [SerializeField] private EntityStats stats;
    [SerializeField] private CombatTeam attackerTeam = CombatTeam.Player;
    [SerializeField] private int damage = 2;
    [SerializeField] private Collider hitboxCollider;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
    [SerializeField] private bool playTargetKnockback = true;

    private readonly HashSet<Damageable> damagedTargets = new HashSet<Damageable>();
    private bool isActive;
    private EntityStatsProvider statsProvider;

    private void Awake()
    {
        ResolveStats();

        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider>();
        }

        SetColliderEnabled(false);
    }

    private void OnEnable()
    {
        SubscribeToStatsProvider();
    }

    private void Reset()
    {
        hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
        }
    }

    private void OnDisable()
    {
        if (statsProvider != null)
        {
            statsProvider.StatsChanged -= SetStats;
        }

        isActive = false;
        SetColliderEnabled(false);
    }

    private void OnValidate()
    {
        damage = Mathf.Max(1, damage);

        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider>();
        }

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
        }
    }

    public void SetStats(EntityStats newStats)
    {
        stats = newStats;
    }

    public void BeginHitWindow()
    {
        damagedTargets.Clear();
        isActive = true;
        SetColliderEnabled(true);
    }

    public void EndHitWindow()
    {
        isActive = false;
        SetColliderEnabled(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (!isActive || !CanHitCollider(other))
        {
            return;
        }

        Damageable damageable = other.GetComponentInParent<Damageable>();

        if (damageable != null && damagedTargets.Add(damageable))
        {
            if (damageable.TryTakeDamage(GetDamage(), attackerTeam, gameObject))
            {
                TryPlayTargetKnockback(damageable);
            }
        }
    }

    private int GetDamage()
    {
        return stats != null ? stats.AttackPower : damage;
    }

    private bool CanHitCollider(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return false;
        }

        if (triggerInteraction == QueryTriggerInteraction.Ignore && other.isTrigger)
        {
            return false;
        }

        if (triggerInteraction == QueryTriggerInteraction.Collide && !other.isTrigger)
        {
            return true;
        }

        return triggerInteraction != QueryTriggerInteraction.Ignore;
    }

    private void SetColliderEnabled(bool enabled)
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = enabled;
        }
    }

    private void TryPlayTargetKnockback(Damageable target)
    {
        if (!playTargetKnockback || target == null)
        {
            return;
        }

        DamageKnockback knockback = target.GetComponentInParent<DamageKnockback>();

        if (knockback != null)
        {
            knockback.PlayKnockback();
        }
    }

    private void ResolveStats()
    {
        if (stats != null)
        {
            return;
        }

        statsProvider = GetComponentInParent<EntityStatsProvider>();

        if (statsProvider != null)
        {
            stats = statsProvider.Stats;
        }
    }

    private void SubscribeToStatsProvider()
    {
        statsProvider = GetComponentInParent<EntityStatsProvider>();

        if (statsProvider != null)
        {
            statsProvider.StatsChanged -= SetStats;
            statsProvider.StatsChanged += SetStats;
        }
    }
}
