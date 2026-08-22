using System.Collections.Generic;
using System;
using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    [SerializeField] private EntityStats stats;
    [SerializeField] private CombatTeam attackerTeam = CombatTeam.Enemy;
    [SerializeField] private int damage = 1;
    [SerializeField] private float hitCooldown = 1f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private bool ignoreTriggerColliders = true;
    [SerializeField] private GameObject damageSource;
    [SerializeField] private bool playTargetKnockback = true;
    [SerializeField] private float attackerPauseDuration = 0.35f;

    private readonly Dictionary<Damageable, float> nextHitTimes = new Dictionary<Damageable, float>();
    private SimpleEnemyChase enemyChase;
    private EntityStatsProvider statsProvider;

    public event Action<Damageable> DamageDealt;

    private void Awake()
    {
        ResolveStats();
        enemyChase = GetComponentInParent<SimpleEnemyChase>();

        if (damageSource == null)
        {
            damageSource = enemyChase != null ? enemyChase.gameObject : gameObject;
        }
    }

    private void OnEnable()
    {
        SubscribeToStatsProvider();
    }

    private void OnDisable()
    {
        if (statsProvider != null)
        {
            statsProvider.StatsChanged -= SetStats;
        }
    }

    private void OnValidate()
    {
        damage = Mathf.Max(1, damage);
        hitCooldown = Mathf.Max(0f, hitCooldown);
        attackerPauseDuration = Mathf.Max(0f, attackerPauseDuration);
    }

    public void SetStats(EntityStats newStats)
    {
        stats = newStats;
    }

    public void SetPlayTargetKnockback(bool shouldPlayKnockback)
    {
        playTargetKnockback = shouldPlayKnockback;
    }

    private void OnTriggerStay(Collider other)
    {
        if (ignoreTriggerColliders && other.isTrigger)
        {
            return;
        }

        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        Damageable target = other.GetComponentInParent<Damageable>();

        if (target == null || !CanHit(target))
        {
            return;
        }

        if (!target.TryTakeDamage(GetDamage(), attackerTeam, damageSource))
        {
            return;
        }

        nextHitTimes[target] = Time.time + GetHitCooldown();
        DamageDealt?.Invoke(target);

        if (playTargetKnockback)
        {
            DamageKnockback knockback = target.GetComponentInParent<DamageKnockback>();

            if (knockback != null)
            {
                knockback.PlayKnockback();
            }
        }

        if (enemyChase != null)
        {
            enemyChase.Stagger(attackerPauseDuration);
        }
    }

    private bool CanHit(Damageable target)
    {
        if (!target.CanBeDamagedBy(attackerTeam))
        {
            return false;
        }

        return !nextHitTimes.TryGetValue(target, out float nextHitTime) || Time.time >= nextHitTime;
    }

    private int GetDamage()
    {
        return stats != null ? stats.ContactDamage : damage;
    }

    private float GetHitCooldown()
    {
        return stats != null ? stats.ContactHitCooldown : hitCooldown;
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
