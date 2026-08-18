using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField] private EntityStats stats;
    [SerializeField] private CombatTeam team = CombatTeam.Neutral;
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float invincibilityDuration;
    [SerializeField] private bool destroyOnDeath;

    [Header("Events")]
    [SerializeField] private HealthChangedEvent healthChanged;
    [SerializeField] private UnityEvent damaged;
    [SerializeField] private UnityEvent died;

    public CombatTeam Team => team;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => stats != null ? stats.MaxHealth : maxHealth;
    public bool IsAlive => CurrentHealth > 0;
    public GameObject LastDamageSource { get; private set; }
    public bool IsInvincible => Time.time < invincibleUntilTime;

    private float invincibleUntilTime;

    private void Awake()
    {
        ResolveStats();
        CurrentHealth = MaxHealth;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
    }

    public void RestoreToFullHealth()
    {
        CurrentHealth = MaxHealth;
        healthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public bool CanBeDamagedBy(CombatTeam attackerTeam)
    {
        return IsAlive && !IsInvincible && (team == CombatTeam.Neutral || attackerTeam == CombatTeam.Neutral || team != attackerTeam);
    }

    public bool TryTakeDamage(int amount, CombatTeam attackerTeam, GameObject source = null)
    {
        if (amount <= 0 || !CanBeDamagedBy(attackerTeam))
        {
            return false;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        LastDamageSource = source;
        invincibleUntilTime = Time.time + GetInvincibilityDuration();
        damaged?.Invoke();
        healthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth == 0)
        {
            Die(source);
        }

        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        healthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void Die(GameObject source)
    {
        died?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    private float GetInvincibilityDuration()
    {
        return stats != null ? stats.InvincibilityDuration : invincibilityDuration;
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

[System.Serializable]
public class HealthChangedEvent : UnityEvent<int, int>
{
}
