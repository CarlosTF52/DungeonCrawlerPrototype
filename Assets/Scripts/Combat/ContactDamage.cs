using System.Collections.Generic;
using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    [SerializeField] private CombatTeam attackerTeam = CombatTeam.Enemy;
    [SerializeField] private int damage = 1;
    [SerializeField] private float hitCooldown = 1f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private bool ignoreTriggerColliders = true;

    private readonly Dictionary<Damageable, float> nextHitTimes = new Dictionary<Damageable, float>();

    private void OnValidate()
    {
        damage = Mathf.Max(1, damage);
        hitCooldown = Mathf.Max(0f, hitCooldown);
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

        nextHitTimes[target] = Time.time + hitCooldown;
        target.TryTakeDamage(damage, attackerTeam, gameObject);
    }

    private bool CanHit(Damageable target)
    {
        if (!target.CanBeDamagedBy(attackerTeam))
        {
            return false;
        }

        return !nextHitTimes.TryGetValue(target, out float nextHitTime) || Time.time >= nextHitTime;
    }
}
