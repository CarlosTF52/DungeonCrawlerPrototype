using System;
using System.Collections.Generic;
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

    private static readonly HashSet<TavernCharacterSelector> activeSelectorsInRange = new HashSet<TavernCharacterSelector>();

    [SerializeField] private SelectionAction action = SelectionAction.NextCharacter;
    [SerializeField] private int characterIndex;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to choose character";
    [SerializeField] private bool showStatsInPrompt = true;
    [SerializeField] private float overlapRefreshInterval = 0.2f;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key interactionKey = Key.E;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyInteractionKey = KeyCode.E;
#endif

    private bool isPlayerInRange;
    private Collider[] selectorColliders;
    private float nextOverlapRefreshTime;

    public static event Action PlayerRangeChanged;
    public static bool IsPlayerInAnySelectorRange
    {
        get
        {
            PruneInactiveSelectors();
            return activeSelectorsInRange.Count > 0;
        }
    }

    public static bool IsPlayerInAnySelectorRangeFor(string playerTag)
    {
        if (IsPlayerInAnySelectorRange)
        {
            return true;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            return false;
        }

        Collider[] playerColliders = player.GetComponentsInChildren<Collider>();

        if (playerColliders == null || playerColliders.Length == 0)
        {
            return false;
        }

        TavernCharacterSelector[] selectors = FindObjectsOfType<TavernCharacterSelector>();

        for (int i = 0; i < selectors.Length; i++)
        {
            TavernCharacterSelector selector = selectors[i];

            if (selector != null && selector.isActiveAndEnabled && selector.IsOverlappingPlayer(playerColliders))
            {
                selector.SetPlayerInRange(true);
                return true;
            }
        }

        return false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        activeSelectorsInRange.Clear();
        PlayerRangeChanged = null;
    }

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
        CacheSelectorColliders();
    }

    private void OnEnable()
    {
        CacheSelectorColliders();
        nextOverlapRefreshTime = 0f;
        RefreshPlayerOverlap();
    }

    private void OnDisable()
    {
        SetPlayerInRange(false);
    }

    private void OnValidate()
    {
        characterIndex = Mathf.Max(0, characterIndex);
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
            SelectCharacter();
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
        if (!isPlayerInRange || string.IsNullOrWhiteSpace(promptText))
        {
            return;
        }

        CharacterDefinition activeCharacter = CharacterRosterManager.Instance.ActiveCharacter;
        string activeSummary = BuildActiveCharacterSummary(activeCharacter);
        GUI.Label(new Rect(20f, Screen.height - 210f, 620f, 96f), $"{promptText}\n{activeSummary}");
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

    private void SetPlayerInRange(bool value)
    {
        if (isPlayerInRange == value)
        {
            return;
        }

        isPlayerInRange = value;

        bool changed = value
            ? activeSelectorsInRange.Add(this)
            : activeSelectorsInRange.Remove(this);

        if (changed)
        {
            PlayerRangeChanged?.Invoke();
        }
    }

    private static void PruneInactiveSelectors()
    {
        activeSelectorsInRange.RemoveWhere(selector => selector == null || !selector.isActiveAndEnabled);
    }

    private void CacheSelectorColliders()
    {
        selectorColliders = GetComponents<Collider>();
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
        if (selectorColliders == null || selectorColliders.Length == 0)
        {
            CacheSelectorColliders();
        }

        for (int i = 0; i < selectorColliders.Length; i++)
        {
            Collider selectorCollider = selectorColliders[i];

            if (selectorCollider == null || !selectorCollider.enabled)
            {
                continue;
            }

            for (int j = 0; j < playerColliders.Length; j++)
            {
                Collider playerCollider = playerColliders[j];

                if (playerCollider != null && playerCollider.enabled && selectorCollider.bounds.Intersects(playerCollider.bounds))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private string BuildActiveCharacterSummary(CharacterDefinition activeCharacter)
    {
        if (activeCharacter == null)
        {
            return "Active: No character";
        }

        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        EntityStats stats = rosterManager.GetEffectiveStats(activeCharacter);

        if (!showStatsInPrompt || stats == null)
        {
            return $"Active: {rosterManager.GetDisplayName(activeCharacter)}";
        }

        int weaponDamageBonus = VillageUpgradeManager.Instance.GetEffectTotal(activeCharacter.CharacterId, UpgradeEffectType.WeaponDamage);
        int effectiveAttack = Mathf.Max(1, stats.AttackPower + weaponDamageBonus);
        string attackSummary = weaponDamageBonus > 0 ? $"{effectiveAttack} ({stats.AttackPower}+{weaponDamageBonus})" : stats.AttackPower.ToString();
        string unavailableSummary = rosterManager.GetStatus(activeCharacter) == CharacterStatus.Fallen ? $"  Returns in {rosterManager.GetRunsUntilReturn(activeCharacter)} run(s)" : string.Empty;
        return $"Active: {rosterManager.GetDisplayName(activeCharacter)} ({activeCharacter.RoleName})  Age {rosterManager.GetAge(activeCharacter)}  {rosterManager.GetStatus(activeCharacter)}{unavailableSummary}\nHP {rosterManager.GetStoredHealth(activeCharacter)}/{stats.MaxHealth}  STA {stats.MaxStamina:0.#}  REG {stats.StaminaRegenPerSecond:0.##}/s  ATK {attackSummary}  Job {rosterManager.GetVillageJob(activeCharacter)}";
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
