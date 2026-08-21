using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRosterManager : MonoBehaviour
{
    private static CharacterRosterManager instance;

    [SerializeField] private List<CharacterDefinition> startingRoster = new List<CharacterDefinition>();
    [SerializeField] private int activeCharacterIndex;
    [SerializeField] private bool loadRosterFromResourcesIfEmpty = true;
    [SerializeField] private string resourcesRosterPath = "Characters";
    [SerializeField] private bool applyActiveCharacterToPlayer = true;
    [SerializeField] private string playerTag = "Player";

    public static CharacterRosterManager Instance
    {
        get
        {
            EnsureInstanceExists();
            return instance;
        }
    }

    public event Action ActiveCharacterChanged;

    public IReadOnlyList<CharacterDefinition> Roster => startingRoster;
    public int ActiveCharacterIndex => startingRoster.Count > 0 ? Mathf.Clamp(activeCharacterIndex, 0, startingRoster.Count - 1) : -1;
    public CharacterDefinition ActiveCharacter => ActiveCharacterIndex >= 0 ? startingRoster[ActiveCharacterIndex] : null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        LoadRosterFromResourcesIfNeeded();
        activeCharacterIndex = ActiveCharacterIndex;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyActiveCharacterToScenePlayer();
    }

    private void OnValidate()
    {
        if (startingRoster.Count == 0)
        {
            activeCharacterIndex = 0;
            return;
        }

        activeCharacterIndex = Mathf.Clamp(activeCharacterIndex, 0, startingRoster.Count - 1);
    }

    public bool SelectCharacter(int index)
    {
        LoadRosterFromResourcesIfNeeded();

        if (startingRoster.Count == 0 || index < 0 || index >= startingRoster.Count)
        {
            return false;
        }

        if (activeCharacterIndex == index)
        {
            ApplyActiveCharacterToScenePlayer();
            return true;
        }

        activeCharacterIndex = index;
        ApplyActiveCharacterToScenePlayer();
        ActiveCharacterChanged?.Invoke();
        return true;
    }

    public void SelectNextCharacter()
    {
        LoadRosterFromResourcesIfNeeded();

        if (startingRoster.Count == 0)
        {
            return;
        }

        SelectCharacter((ActiveCharacterIndex + 1) % startingRoster.Count);
    }

    public void SelectPreviousCharacter()
    {
        LoadRosterFromResourcesIfNeeded();

        if (startingRoster.Count == 0)
        {
            return;
        }

        int nextIndex = ActiveCharacterIndex - 1;

        if (nextIndex < 0)
        {
            nextIndex = startingRoster.Count - 1;
        }

        SelectCharacter(nextIndex);
    }

    public void ApplyActiveCharacterToScenePlayer()
    {
        if (!applyActiveCharacterToPlayer || ActiveCharacter == null || ActiveCharacter.Stats == null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            return;
        }

        ApplyActiveCharacterToPlayer(player);
    }

    public void ApplyActiveCharacterToPlayer(GameObject player)
    {
        CharacterDefinition activeCharacter = ActiveCharacter;

        if (player == null || activeCharacter == null || activeCharacter.Stats == null)
        {
            return;
        }

        EntityStatsProvider statsProvider = player.GetComponentInParent<EntityStatsProvider>();
        Damageable damageable = player.GetComponentInParent<Damageable>();
        StaminaPool staminaPool = player.GetComponentInParent<StaminaPool>();
        MeleeAttack meleeAttack = player.GetComponentInParent<MeleeAttack>();
        PlayerWeaponHitbox[] weaponHitboxes = player.GetComponentsInChildren<PlayerWeaponHitbox>(true);

        if (statsProvider != null)
        {
            statsProvider.SetStats(activeCharacter.Stats);
        }

        if (damageable != null)
        {
            damageable.SetStats(activeCharacter.Stats, true);
        }

        if (staminaPool != null)
        {
            staminaPool.SetStats(activeCharacter.Stats, true);
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

    private void LoadRosterFromResourcesIfNeeded()
    {
        if (!loadRosterFromResourcesIfEmpty || startingRoster.Count > 0)
        {
            return;
        }

        CharacterDefinition[] loadedCharacters = Resources.LoadAll<CharacterDefinition>(resourcesRosterPath);

        if (loadedCharacters == null || loadedCharacters.Length == 0)
        {
            return;
        }

        startingRoster.AddRange(loadedCharacters);
        activeCharacterIndex = ActiveCharacterIndex;
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        CharacterRosterManager sceneManager = FindObjectOfType<CharacterRosterManager>();

        if (sceneManager != null)
        {
            instance = sceneManager;
            return;
        }

        GameObject managerObject = new GameObject("CharacterRosterManager");
        managerObject.AddComponent<CharacterRosterManager>();
    }
}
