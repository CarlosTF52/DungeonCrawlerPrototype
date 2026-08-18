using UnityEngine;

[CreateAssetMenu(fileName = "EntityStats", menuName = "Dungeon Crawler/Combat/Entity Stats")]
public class EntityStats : ScriptableObject
{
    [Header("Vitals")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float maxStamina = 10f;
    [SerializeField] private float staminaRegenPerSecond = 2f;
    [SerializeField] private float sprintStaminaCostPerSecond = 4f;
    [SerializeField] private float attackStaminaCost = 3f;
    [SerializeField] private float invincibilityDuration;

    [Header("Offense")]
    [SerializeField] private int attackPower = 2;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float contactHitCooldown = 1f;

    public int MaxHealth => maxHealth;
    public float MaxStamina => maxStamina;
    public float StaminaRegenPerSecond => staminaRegenPerSecond;
    public float SprintStaminaCostPerSecond => sprintStaminaCostPerSecond;
    public float AttackStaminaCost => attackStaminaCost;
    public float InvincibilityDuration => invincibilityDuration;
    public int AttackPower => attackPower;
    public int ContactDamage => contactDamage;
    public float ContactHitCooldown => contactHitCooldown;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        maxStamina = Mathf.Max(0f, maxStamina);
        staminaRegenPerSecond = Mathf.Max(0f, staminaRegenPerSecond);
        sprintStaminaCostPerSecond = Mathf.Max(0f, sprintStaminaCostPerSecond);
        attackStaminaCost = Mathf.Max(0f, attackStaminaCost);
        invincibilityDuration = Mathf.Max(0f, invincibilityDuration);
        attackPower = Mathf.Max(1, attackPower);
        contactDamage = Mathf.Max(1, contactDamage);
        contactHitCooldown = Mathf.Max(0f, contactHitCooldown);
    }
}
