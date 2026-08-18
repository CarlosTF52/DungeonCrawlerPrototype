using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
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
    public int MaxHealth => maxHealth;
    public bool IsAlive => CurrentHealth > 0;
    public GameObject LastDamageSource { get; private set; }
    public bool IsInvincible => Time.time < invincibleUntilTime;

    private float invincibleUntilTime;

    private void Awake()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
    }

    public void RestoreToFullHealth()
    {
        CurrentHealth = maxHealth;
        healthChanged?.Invoke(CurrentHealth, maxHealth);
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
        invincibleUntilTime = Time.time + invincibilityDuration;
        damaged?.Invoke();
        healthChanged?.Invoke(CurrentHealth, maxHealth);

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

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        healthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die(GameObject source)
    {
        died?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}

[System.Serializable]
public class HealthChangedEvent : UnityEvent<int, int>
{
}
