using UnityEngine;

public class PlayerCharacterDefeatHandler : MonoBehaviour
{
    [SerializeField] private Damageable playerHealth;
    [SerializeField] private bool failExpeditionOnDefeat = true;
    [SerializeField] private bool disableStressOnDamage;
    [SerializeField] private int stressPerHealthLost = 1;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<Damageable>();
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged += HandlePlayerDamaged;
            playerHealth.Died += HandlePlayerDied;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged -= HandlePlayerDamaged;
            playerHealth.Died -= HandlePlayerDied;
        }
    }

    private void OnValidate()
    {
        stressPerHealthLost = Mathf.Max(1, stressPerHealthLost);
    }

    private void HandlePlayerDamaged()
    {
        if (playerHealth == null)
        {
            return;
        }

        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        rosterManager.RecordActiveCharacterHealthFromScenePlayer();

        if (disableStressOnDamage)
        {
            return;
        }

        int stressAmount = Mathf.Max(0, playerHealth.LastHealthLost) * Mathf.Max(1, stressPerHealthLost);

        if (stressAmount > 0)
        {
            rosterManager.AddStressToActiveCharacter(stressAmount);
        }
    }

    private void HandlePlayerDied()
    {
        CharacterRosterManager.Instance.MarkActiveCharacterDefeated(playerHealth.LastOverkillDamage);

        if (failExpeditionOnDefeat)
        {
            ExpeditionRunManager.Instance.FailExpedition();
        }
    }
}
