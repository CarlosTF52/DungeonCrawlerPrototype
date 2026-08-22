using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private EntityStats stats;
    [SerializeField] private CombatTeam attackerTeam = CombatTeam.Player;
    [SerializeField] private int damage = 2;
    [SerializeField] private float range = 1.5f;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float cooldown = 0.45f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private bool useInput = true;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
    [SerializeField] private bool playTargetKnockback = true;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key attackKey = Key.Space;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyAttackKey = KeyCode.Space;
#endif

    private float nextAttackTime;
    private readonly Collider[] hits = new Collider[16];
    private readonly HashSet<Damageable> damagedTargets = new HashSet<Damageable>();
    private EntityStatsProvider statsProvider;

    private void Reset()
    {
        attackOrigin = transform;
    }

    private void Awake()
    {
        ResolveStats();
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
        range = Mathf.Max(0f, range);
        radius = Mathf.Max(0.01f, radius);
        cooldown = Mathf.Max(0f, cooldown);
    }

    private void Update()
    {
        if (useInput && WasAttackPressed())
        {
            TryAttack();
        }
    }

#if ENABLE_INPUT_SYSTEM
    public void OnAttack(InputValue value)
    {
        if (useInput && value.isPressed)
        {
            TryAttack();
        }
    }
#endif

    public void SetStats(EntityStats newStats)
    {
        stats = newStats;
    }

    public bool TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return false;
        }

        nextAttackTime = Time.time + cooldown;
        PerformAttack();
        return true;
    }

    private void PerformAttack()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Vector3 center = origin.position + origin.forward * range;
        int hitCount = Physics.OverlapSphereNonAlloc(center, radius, hits, targetLayers, triggerInteraction);
        damagedTargets.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Damageable damageable = hits[i].GetComponentInParent<Damageable>();

            if (damageable != null && damagedTargets.Add(damageable))
            {
                if (damageable.TryTakeDamage(GetDamage(), attackerTeam, gameObject))
                {
                    TryPlayTargetKnockback(damageable);
                }
            }
        }
    }

    private int GetDamage()
    {
        int baseDamage = stats != null ? stats.AttackPower : damage;

        if (attackerTeam == CombatTeam.Player)
        {
            baseDamage += VillageUpgradeManager.Instance.WeaponDamageBonus;
        }

        return Mathf.Max(1, baseDamage);
    }

    private bool WasAttackPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current[attackKey].wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(legacyAttackKey))
        {
            return true;
        }
#endif

        return false;
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

    private void OnDrawGizmosSelected()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin.position + origin.forward * range, radius);
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
