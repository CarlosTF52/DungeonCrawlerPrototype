using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TavernCharacterSelector : MonoBehaviour
{
    public enum SelectionAction
    {
        NextCharacter,
        PreviousCharacter,
        SelectConfiguredIndex
    }

    [SerializeField] private SelectionAction action = SelectionAction.NextCharacter;
    [SerializeField] private int characterIndex;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to choose character";
    [SerializeField] private bool showStatsInPrompt = true;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key interactionKey = Key.E;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyInteractionKey = KeyCode.E;
#endif

    private bool isPlayerInRange;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        characterIndex = Mathf.Max(0, characterIndex);
    }

    private void Update()
    {
        if (isPlayerInRange && WasInteractionPressed())
        {
            SelectCharacter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }

    private void OnGUI()
    {
        if (!isPlayerInRange || string.IsNullOrWhiteSpace(promptText))
        {
            return;
        }

        CharacterDefinition activeCharacter = CharacterRosterManager.Instance.ActiveCharacter;
        string activeSummary = BuildActiveCharacterSummary(activeCharacter);
        GUI.Label(new Rect(20f, Screen.height - 190f, 520f, 72f), $"{promptText}\n{activeSummary}");
    }

    public void SelectCharacter()
    {
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;

        switch (action)
        {
            case SelectionAction.NextCharacter:
                rosterManager.SelectNextCharacter();
                break;
            case SelectionAction.PreviousCharacter:
                rosterManager.SelectPreviousCharacter();
                break;
            case SelectionAction.SelectConfiguredIndex:
                rosterManager.SelectCharacter(characterIndex);
                break;
        }
    }

    private string BuildActiveCharacterSummary(CharacterDefinition activeCharacter)
    {
        if (activeCharacter == null)
        {
            return "Active: No character";
        }

        if (!showStatsInPrompt || activeCharacter.Stats == null)
        {
            return $"Active: {activeCharacter.DisplayName}";
        }

        EntityStats stats = activeCharacter.Stats;
        return $"Active: {activeCharacter.DisplayName} ({activeCharacter.RoleName})  HP {stats.MaxHealth}  STA {stats.MaxStamina:0.#}  ATK {stats.AttackPower}";
    }

    private bool WasInteractionPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current[interactionKey].wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(legacyInteractionKey))
        {
            return true;
        }
#endif

        return false;
    }
}
