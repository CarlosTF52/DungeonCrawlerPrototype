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

    private readonly HashSet<Damageable> damagedTargets = new HashSet<Damageable>();
    private bool isActive;

    private void Awake()
    {
        ResolveStats();

        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider>();
        }

        SetColliderEnabled(false);
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
            damageable.TryTakeDamage(GetDamage(), attackerTeam, gameObject);
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

    private void ResolveStats()
    {
        if (stats != null)
        {
            return;
        }

        EntityStatsProvider provider = GetComponentInParent<EntityStatsProvider>();

        if (provider != null)
        {
            stats = provider.Stats;
        }
    }
}
