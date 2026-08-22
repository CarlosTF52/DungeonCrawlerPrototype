using UnityEngine;

public class PlayerCharacterApplier : MonoBehaviour
{
    [SerializeField] private EntityStatsProvider statsProvider;

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
        rosterManager.CharacterStatusChanged += ApplyActiveCharacter;
        ApplyActiveCharacter();
    }

    private void OnDisable()
    {
        if (rosterManager != null)
        {
            rosterManager.ActiveCharacterChanged -= ApplyActiveCharacter;
            rosterManager.CharacterStatusChanged -= ApplyActiveCharacter;
        }
    }

    public void ApplyActiveCharacter()
    {
        if (rosterManager == null)
        {
            return;
        }

        rosterManager.ApplyActiveCharacterToPlayer(gameObject);
    }
}
