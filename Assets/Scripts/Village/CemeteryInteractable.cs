using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CemeteryInteractable : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to cycle graves";
    [SerializeField] private string emptyText = "No permanent deaths recorded";
    [SerializeField] private Vector2 hudSize = new Vector2(560f, 196f);
    [SerializeField] private float overlapRefreshInterval = 0.2f;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key interactionKey = Key.E;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyInteractionKey = KeyCode.E;
#endif

    private readonly List<CharacterDefinition> deadCharacters = new List<CharacterDefinition>();
    private bool isPlayerInRange;
    private Collider[] cemeteryColliders;
    private int selectedDeadIndex;
    private float nextOverlapRefreshTime;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        CacheCemeteryColliders();
    }

    private void OnEnable()
    {
        CacheCemeteryColliders();
        nextOverlapRefreshTime = 0f;
        RefreshPlayerOverlap();
        CharacterRosterManager.Instance.CharacterStatusChanged += HandleCharacterStatusChanged;
        RebuildDeadCharacters();
    }

    private void OnDisable()
    {
        SetPlayerInRange(false);

        CharacterRosterManager rosterManager = FindObjectOfType<CharacterRosterManager>();

        if (rosterManager != null)
        {
            rosterManager.CharacterStatusChanged -= HandleCharacterStatusChanged;
        }
    }

    private void OnValidate()
    {
        hudSize.x = Mathf.Max(380f, hudSize.x);
        hudSize.y = Mathf.Max(156f, hudSize.y);
        overlapRefreshInterval = Mathf.Max(0.05f, overlapRefreshInterval);
    }

    private void Update()
    {
        if (Time.time >= nextOverlapRefreshTime)
        {
            RefreshPlayerOverlap();
            nextOverlapRefreshTime = Time.time + overlapRefreshInterval;
        }

        if (isPlayerInRange && WasInteractionPressed())
        {
            CycleDeadCharacter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            SetPlayerInRange(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            SetPlayerInRange(false);
        }
    }

    private void OnGUI()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        RebuildDeadCharacters();

        Rect panelRect = new Rect(20f, Screen.height - hudSize.y - 92f, hudSize.x, hudSize.y);
        GUI.Box(panelRect, "Cemetery");

        if (deadCharacters.Count == 0)
        {
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 30f, panelRect.width - 24f, 24f), emptyText);
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 56f, panelRect.width - 24f, 42f), "Characters who die permanently will be remembered here.");
            return;
        }

        CharacterDefinition character = deadCharacters[Mathf.Clamp(selectedDeadIndex, 0, deadCharacters.Count - 1)];
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;

        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 28f, panelRect.width - 24f, 24f), $"{promptText}  ({selectedDeadIndex + 1}/{deadCharacters.Count})");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 54f, panelRect.width - 24f, 24f), $"{rosterManager.GetDisplayName(character)}  ({character.RoleName})");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 80f, panelRect.width - 24f, 24f), $"Age at death: {rosterManager.GetAge(character)}   Status: Permanently dead");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 106f, panelRect.width - 24f, 24f), "Runs until return: Never   No resurrection currently available");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 132f, panelRect.width - 24f, 24f), $"Injuries: {rosterManager.GetInjurySeverity(character)}   Stress: {rosterManager.GetStress(character)}   Job: {rosterManager.GetVillageJob(character)}");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 158f, panelRect.width - 24f, 30f), BuildMemorialText(character));
    }

    private void CycleDeadCharacter()
    {
        RebuildDeadCharacters();

        if (deadCharacters.Count <= 1)
        {
            return;
        }

        selectedDeadIndex = (selectedDeadIndex + 1) % deadCharacters.Count;
    }

    private void HandleCharacterStatusChanged()
    {
        RebuildDeadCharacters();
    }

    private void RebuildDeadCharacters()
    {
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        deadCharacters.Clear();

        IReadOnlyList<CharacterDefinition> roster = rosterManager.Roster;

        for (int i = 0; i < roster.Count; i++)
        {
            CharacterDefinition character = roster[i];

            if (character != null && rosterManager.GetStatus(character) == CharacterStatus.Dead)
            {
                deadCharacters.Add(character);
            }
        }

        if (selectedDeadIndex >= deadCharacters.Count)
        {
            selectedDeadIndex = Mathf.Max(0, deadCharacters.Count - 1);
        }
    }

    private string BuildMemorialText(CharacterDefinition character)
    {
        if (character == null || string.IsNullOrWhiteSpace(character.Description))
        {
            return "Their name remains in the village records.";
        }

        return character.Description;
    }

    private void SetPlayerInRange(bool value)
    {
        if (isPlayerInRange == value)
        {
            return;
        }

        isPlayerInRange = value;
    }

    private void CacheCemeteryColliders()
    {
        cemeteryColliders = GetComponents<Collider>();
    }

    private void RefreshPlayerOverlap()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            SetPlayerInRange(false);
            return;
        }

        Collider[] playerColliders = player.GetComponentsInChildren<Collider>();

        if (playerColliders == null || playerColliders.Length == 0)
        {
            SetPlayerInRange(false);
            return;
        }

        SetPlayerInRange(IsOverlappingPlayer(playerColliders));
    }

    private bool IsOverlappingPlayer(Collider[] playerColliders)
    {
        if (cemeteryColliders == null || cemeteryColliders.Length == 0)
        {
            CacheCemeteryColliders();
        }

        for (int i = 0; i < cemeteryColliders.Length; i++)
        {
            Collider cemeteryCollider = cemeteryColliders[i];

            if (cemeteryCollider == null || !cemeteryCollider.enabled)
            {
                continue;
            }

            for (int j = 0; j < playerColliders.Length; j++)
            {
                Collider playerCollider = playerColliders[j];

                if (playerCollider != null && playerCollider.enabled && cemeteryCollider.bounds.Intersects(playerCollider.bounds))
                {
                    return true;
                }
            }
        }

        return false;
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
