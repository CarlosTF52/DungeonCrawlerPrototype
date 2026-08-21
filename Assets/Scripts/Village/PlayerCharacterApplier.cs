using UnityEngine;

public class PlayerCharacterApplier : MonoBehaviour
{
    [SerializeField] private EntityStatsProvider statsProvider;
    [SerializeField] private bool restoreVitalsWhenCharacterChanges = true;

    private CharacterRosterManager rosterManager;

    private void Awake()
    {
        if (statsProvider == null)
        {
            statsProvider = GetComponentInParent<EntityStatsProvider>();
        }
    }

    private void OnEnable()
    {
        rosterManager = CharacterRosterManager.Instance;
        rosterManager.ActiveCharacterChanged += ApplyActiveCharacter;
        ApplyActiveCharacter();
    }

    private void OnDisable()
    {
        if (rosterManager != null)
        {
            rosterManager.ActiveCharacterChanged -= ApplyActiveCharacter;
        }
    }

    public void ApplyActiveCharacter()
    {
        CharacterDefinition activeCharacter = rosterManager != null ? rosterManager.ActiveCharacter : null;

        if (activeCharacter == null || activeCharacter.Stats == null)
        {
            return;
        }

        if (statsProvider != null)
        {
            statsProvider.SetStats(activeCharacter.Stats);
        }

        Damageable damageable = GetComponentInParent<Damageable>();
        StaminaPool staminaPool = GetComponentInParent<StaminaPool>();
        MeleeAttack meleeAttack = GetComponentInParent<MeleeAttack>();
        PlayerWeaponHitbox[] weaponHitboxes = GetComponentsInChildren<PlayerWeaponHitbox>(true);

        if (damageable != null)
        {
            damageable.SetStats(activeCharacter.Stats, restoreVitalsWhenCharacterChanges);
        }

        if (staminaPool != null)
        {
            staminaPool.SetStats(activeCharacter.Stats, restoreVitalsWhenCharacterChanges);
        }

        if (meleeAttack != null)
        {
            meleeAttack.SetStats(activeCharacter.Stats);
        }

        for (int i = 0; i < weaponHitboxes.Length; i++)
        {
            weaponHitboxes[i].SetStats(activeCharacter.Stats);
        }
    }
}
