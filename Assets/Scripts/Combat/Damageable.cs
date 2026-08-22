using UnityEngine;
using UnityEngine.Events;
using System;

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
    public int LastDamageAmount { get; private set; }
    public int LastHealthLost { get; private set; }
    public int LastOverkillDamage { get; private set; }
    public bool IsInvincible => Time.time < invincibleUntilTime;

    public event Action Damaged;
    public event Action Died;

    private float invincibleUntilTime;
    private EntityStatsProvider statsProvider;

    private void Awake()
    {
        ResolveStats();
        CurrentHealth = MaxHealth;
    }

    private void OnEnable()
    {
        SubscribeToStatsProvider();
    }

    private void OnDisable()
    {
        if (statsProvider != null)
        {
            statsProvider.StatsChanged -= HandleProviderStatsChanged;
        }
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
    }

    public void SetStats(EntityStats newStats, bool restoreToFullHealth)
    {
        stats = newStats;

        if (restoreToFullHealth)
        {
            RestoreToFullHealth();
            return;
        }

        SetCurrentHealth(CurrentHealth);
    }

    public void RestoreToFullHealth()
    {
        SetCurrentHealth(MaxHealth);
    }

    public void SetCurrentHealth(int currentHealth)
    {
        CurrentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
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

        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        LastDamageSource = source;
        LastDamageAmount = amount;
        LastHealthLost = previousHealth - CurrentHealth;
        LastOverkillDamage = Mathf.Max(0, amount - previousHealth);
        invincibleUntilTime = Time.time + GetInvincibilityDuration();
        Damaged?.Invoke();
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
        Died?.Invoke();
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
            statsProvider.StatsChanged -= HandleProviderStatsChanged;
            statsProvider.StatsChanged += HandleProviderStatsChanged;
        }
    }

    private void HandleProviderStatsChanged(EntityStats newStats)
    {
        SetStats(newStats, false);
    }
}

[System.Serializable]
public class HealthChangedEvent : UnityEvent<int, int>
{
}
