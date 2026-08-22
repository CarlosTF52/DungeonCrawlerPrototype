using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ResurrectionWardInteractable : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to inspect fallen characters";
    [SerializeField] private string emptyText = "No fallen characters";
    [SerializeField] private Vector2 hudSize = new Vector2(560f, 176f);
    [SerializeField] private float overlapRefreshInterval = 0.2f;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key interactionKey = Key.E;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyInteractionKey = KeyCode.E;
#endif

    private readonly List<CharacterDefinition> unavailableCharacters = new List<CharacterDefinition>();
    private bool isPlayerInRange;
    private Collider[] wardColliders;
    private int selectedUnavailableIndex;
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
        CacheWardColliders();
    }

    private void OnEnable()
    {
        CacheWardColliders();
        nextOverlapRefreshTime = 0f;
        RefreshPlayerOverlap();
        CharacterRosterManager.Instance.CharacterStatusChanged += HandleCharacterStatusChanged;
        RebuildUnavailableCharacters();
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
        hudSize.x = Mathf.Max(360f, hudSize.x);
        hudSize.y = Mathf.Max(132f, hudSize.y);
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
            CycleUnavailableCharacter();
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

        RebuildUnavailableCharacters();

        Rect panelRect = new Rect(20f, Screen.height - hudSize.y - 92f, hudSize.x, hudSize.y);
        GUI.Box(panelRect, "Resurrection Ward");

        if (unavailableCharacters.Count == 0)
        {
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 30f, panelRect.width - 24f, 24f), emptyText);
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 56f, panelRect.width - 24f, 24f), "Recoverable fallen characters will appear here.");
            return;
        }

        CharacterDefinition character = unavailableCharacters[Mathf.Clamp(selectedUnavailableIndex, 0, unavailableCharacters.Count - 1)];
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        int runsUntilReturn = rosterManager.GetRunsUntilReturn(character);

        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 28f, panelRect.width - 24f, 24f), $"{promptText}  ({selectedUnavailableIndex + 1}/{unavailableCharacters.Count})");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 54f, panelRect.width - 24f, 24f), $"{rosterManager.GetDisplayName(character)}  ({character.RoleName})  Fallen");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 80f, panelRect.width - 24f, 24f), $"Age: {rosterManager.GetAge(character)}   Unavailable for {runsUntilReturn} more run(s)");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 106f, panelRect.width - 24f, 24f), $"Injuries: {rosterManager.GetInjurySeverity(character)}   Stress: {rosterManager.GetStress(character)}   Job: {rosterManager.GetVillageJob(character)}");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 132f, panelRect.width - 24f, 36f), BuildCharacterFlavor(character));
    }

    private void CycleUnavailableCharacter()
    {
        RebuildUnavailableCharacters();

        if (unavailableCharacters.Count <= 1)
        {
            return;
        }

        selectedUnavailableIndex = (selectedUnavailableIndex + 1) % unavailableCharacters.Count;
    }

    private void HandleCharacterStatusChanged()
    {
        RebuildUnavailableCharacters();
    }

    private void RebuildUnavailableCharacters()
    {
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        unavailableCharacters.Clear();

        IReadOnlyList<CharacterDefinition> roster = rosterManager.Roster;

        for (int i = 0; i < roster.Count; i++)
        {
            CharacterDefinition character = roster[i];

            if (character != null && rosterManager.GetStatus(character) == CharacterStatus.Fallen)
            {
                unavailableCharacters.Add(character);
            }
        }

        if (selectedUnavailableIndex >= unavailableCharacters.Count)
        {
            selectedUnavailableIndex = Mathf.Max(0, unavailableCharacters.Count - 1);
        }
    }

    private string BuildCharacterFlavor(CharacterDefinition character)
    {
        if (character == null || string.IsNullOrWhiteSpace(character.Description))
        {
            return string.Empty;
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

    private void CacheWardColliders()
    {
        wardColliders = GetComponents<Collider>();
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
        if (wardColliders == null || wardColliders.Length == 0)
        {
            CacheWardColliders();
        }

        for (int i = 0; i < wardColliders.Length; i++)
        {
            Collider wardCollider = wardColliders[i];

            if (wardCollider == null || !wardCollider.enabled)
            {
                continue;
            }

            for (int j = 0; j < playerColliders.Length; j++)
            {
                Collider playerCollider = playerColliders[j];

                if (playerCollider != null && playerCollider.enabled && wardCollider.bounds.Intersects(playerCollider.bounds))
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