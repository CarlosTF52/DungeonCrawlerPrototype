using UnityEngine;
using UnityEngine.Events;

public class StaminaPool : MonoBehaviour
{
    [SerializeField] private EntityStats stats;
    [SerializeField] private float maxStamina = 10f;
    [SerializeField] private float staminaRegenPerSecond = 2f;
    [SerializeField] private float sprintStaminaCostPerSecond = 4f;
    [SerializeField] private float attackStaminaCost = 3f;
    [SerializeField] private bool regenerateAutomatically = true;

    [Header("Events")]
    [SerializeField] private StaminaChangedEvent staminaChanged;

    public float CurrentStamina { get; private set; }
    public float MaxStamina => stats != null ? stats.MaxStamina : maxStamina;
    public float StaminaRegenPerSecond => stats != null ? stats.StaminaRegenPerSecond : staminaRegenPerSecond;
    public float SprintStaminaCostPerSecond => stats != null ? stats.SprintStaminaCostPerSecond : sprintStaminaCostPerSecond;
    public float AttackStaminaCost => stats != null ? stats.AttackStaminaCost : attackStaminaCost;
    public bool HasStamina => CurrentStamina > 0f;

    private void Awake()
    {
        ResolveStats();
        CurrentStamina = MaxStamina;
    }

    private void OnValidate()
    {
        maxStamina = Mathf.Max(0f, maxStamina);
        staminaRegenPerSecond = Mathf.Max(0f, staminaRegenPerSecond);
        sprintStaminaCostPerSecond = Mathf.Max(0f, sprintStaminaCostPerSecond);
        attackStaminaCost = Mathf.Max(0f, attackStaminaCost);
    }

    private void Update()
    {
        if (regenerateAutomatically)
        {
            Restore(StaminaRegenPerSecond * Time.deltaTime);
        }
    }

    public bool TrySpend(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        if (CurrentStamina < amount)
        {
            return false;
        }

        CurrentStamina -= amount;
        staminaChanged?.Invoke(CurrentStamina, MaxStamina);
        return true;
    }

    public bool TrySpendAttackCost()
    {
        return TrySpend(AttackStaminaCost);
    }

    public bool TrySpendSprintCost(float deltaTime)
    {
        return TrySpend(SprintStaminaCostPerSecond * deltaTime);
    }

    public void Restore(float amount)
    {
        if (amount <= 0f || CurrentStamina >= MaxStamina)
        {
            return;
        }

        float previousStamina = CurrentStamina;
        CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + amount);

        if (!Mathf.Approximately(CurrentStamina, previousStamina))
        {
            staminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }
    }

    public void RestoreToFull()
    {
        CurrentStamina = MaxStamina;
        staminaChanged?.Invoke(CurrentStamina, MaxStamina);
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
public class StaminaChangedEvent : UnityEvent<float, float>
{
}
