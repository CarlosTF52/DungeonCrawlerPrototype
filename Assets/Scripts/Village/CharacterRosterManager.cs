using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRosterManager : MonoBehaviour
{
    private static CharacterRosterManager instance;
    private static readonly Dictionary<string, int> savedHealthByCharacterId = new Dictionary<string, int>();
    private static readonly Dictionary<string, CharacterRuntimeState> runtimeStateByCharacterId = new Dictionary<string, CharacterRuntimeState>();

    [SerializeField] private List<CharacterDefinition> startingRoster = new List<CharacterDefinition>();
    [SerializeField] private int activeCharacterIndex;
    [SerializeField] private bool loadRosterFromResourcesIfEmpty = true;
    [SerializeField] private string resourcesRosterPath = "Characters";
    [SerializeField] private bool applyActiveCharacterToPlayer = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Generated Characters")]
    [SerializeField] private bool generateRuntimeNames = true;
    [SerializeField] private bool randomizeStartingAge = true;
    [SerializeField] private int randomStartingAgeMin = 18;
    [SerializeField] private int randomStartingAgeMax = 65;
    [SerializeField] private string[] generatedFirstNames = { "Mara", "Osric", "Edda", "Bran", "Iris", "Rowan", "Selka", "Tobin" };
    [SerializeField] private string[] generatedLastNames = { "Ash", "Graves", "Vale", "Hollow", "Wick", "Rook", "Mire", "Black" };
    [SerializeField] private int randomHealthOffsetMin = -1;
    [SerializeField] private int randomHealthOffsetMax = 2;
    [SerializeField] private float randomStaminaOffsetMin = -1f;
    [SerializeField] private float randomStaminaOffsetMax = 2f;
    [SerializeField] private float randomStaminaRegenOffsetMin = -0.25f;
    [SerializeField] private float randomStaminaRegenOffsetMax = 0.35f;
    [SerializeField] private int randomAttackOffsetMin;
    [SerializeField] private int randomAttackOffsetMax = 1;

    [Header("Cursed Aging")]
    [SerializeField] private int permanentDeathAge = 100;
    [SerializeField] private int baseYearsAgedOnDefeat = 10;
    [SerializeField] private int oldAgeStartsAt = 50;
    [SerializeField] private int oldAgePenaltyEveryYears = 10;
    [SerializeField] private int healthPenaltyPerOldAgeStep = 1;
    [SerializeField] private float staminaPenaltyPerOldAgeStep = 1f;
    [SerializeField] private float staminaRegenPenaltyPerOldAgeStep = 0.25f;
    [SerializeField] private int youngAgeEndsAt = 29;
    [SerializeField] private int youngHealthBonus = 1;
    [SerializeField] private float youngStaminaBonus = 1f;
    [SerializeField] private float youngStaminaRegenBonus = 0.2f;

    [Header("Stress")]
    [SerializeField] private int baseMaxStress = 10;
    [SerializeField] private int maxStressPerToleranceBonus;

    [Header("Tavern Recovery")]
    [SerializeField] private int restingHealthRecoveredAfterExtractedRun = 2;

    public static CharacterRosterManager Instance
    {
        get
        {
            EnsureInstanceExists();
            return instance;
        }
    }

    public event Action ActiveCharacterChanged;
    public event Action CharacterStatusChanged;

    public IReadOnlyList<CharacterDefinition> Roster => startingRoster;
    public int ActiveCharacterIndex => startingRoster.Count > 0 ? Mathf.Clamp(activeCharacterIndex, 0, startingRoster.Count - 1) : -1;
    public CharacterDefinition ActiveCharacter => ActiveCharacterIndex >= 0 ? startingRoster[ActiveCharacterIndex] : null;
    public int BaseRestingHealthRecoveredAfterExtractedRun => Mathf.Max(0, restingHealthRecoveredAfterExtractedRun);
    public int TavernRecoveryUpgradeBonus => VillageUpgradeManager.Instance.TavernRestingHealthRecoveryBonus;
    public int RestingHealthRecoveredAfterExtractedRun => BaseRestingHealthRecoveredAfterExtractedRun + TavernRecoveryUpgradeBonus;
    public int BaseMaxStress => Mathf.Max(1, baseMaxStress);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        instance = null;
        savedHealthByCharacterId.Clear();
        runtimeStateByCharacterId.Clear();
    }

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
        EnsureRosterRuntimeStateInitialized();
        EnsureRosterHealthInitialized();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyActiveCharacterToScenePlayer();
    }

    private void OnValidate()
    {
        restingHealthRecoveredAfterExtractedRun = Mathf.Max(0, restingHealthRecoveredAfterExtractedRun);
        permanentDeathAge = Mathf.Max(1, permanentDeathAge);
        randomStartingAgeMin = Mathf.Clamp(randomStartingAgeMin, 1, Mathf.Max(1, permanentDeathAge - 1));
        randomStartingAgeMax = Mathf.Clamp(Mathf.Max(randomStartingAgeMin, randomStartingAgeMax), randomStartingAgeMin, Mathf.Max(randomStartingAgeMin, permanentDeathAge - 1));
        randomHealthOffsetMax = Mathf.Max(randomHealthOffsetMin, randomHealthOffsetMax);
        randomStaminaOffsetMax = Mathf.Max(randomStaminaOffsetMin, randomStaminaOffsetMax);
        randomStaminaRegenOffsetMax = Mathf.Max(randomStaminaRegenOffsetMin, randomStaminaRegenOffsetMax);
        randomAttackOffsetMax = Mathf.Max(randomAttackOffsetMin, randomAttackOffsetMax);
        baseYearsAgedOnDefeat = Mathf.Max(0, baseYearsAgedOnDefeat);
        oldAgeStartsAt = Mathf.Max(1, oldAgeStartsAt);
        oldAgePenaltyEveryYears = Mathf.Max(1, oldAgePenaltyEveryYears);
        healthPenaltyPerOldAgeStep = Mathf.Max(0, healthPenaltyPerOldAgeStep);
        staminaPenaltyPerOldAgeStep = Mathf.Max(0f, staminaPenaltyPerOldAgeStep);
        staminaRegenPenaltyPerOldAgeStep = Mathf.Max(0f, staminaRegenPenaltyPerOldAgeStep);
        youngAgeEndsAt = Mathf.Max(1, youngAgeEndsAt);
        youngHealthBonus = Mathf.Max(0, youngHealthBonus);
        youngStaminaBonus = Mathf.Max(0f, youngStaminaBonus);
        youngStaminaRegenBonus = Mathf.Max(0f, youngStaminaRegenBonus);
        baseMaxStress = Mathf.Max(1, baseMaxStress);
        maxStressPerToleranceBonus = Mathf.Max(0, maxStressPerToleranceBonus);

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

        if (startingRoster.Count == 0 || index < 0 || index >= startingRoster.Count || !CanSelectCharacter(startingRoster[index]))
        {
            return false;
        }

        RecordActiveCharacterHealthFromScenePlayer();

        if (activeCharacterIndex == index)
        {
            ApplyActiveCharacterToScenePlayer();
            CharacterStatusChanged?.Invoke();
            return true;
        }

        activeCharacterIndex = index;
        ApplyActiveCharacterToScenePlayer();
        ActiveCharacterChanged?.Invoke();
        CharacterStatusChanged?.Invoke();
        return true;
    }

    public void SelectNextCharacter()
    {
        LoadRosterFromResourcesIfNeeded();

        if (startingRoster.Count == 0)
        {
            return;
        }

        SelectCharacterInDirection(1);
    }

    public void SelectPreviousCharacter()
    {
        LoadRosterFromResourcesIfNeeded();

        if (startingRoster.Count == 0)
        {
            return;
        }

        SelectCharacterInDirection(-1);
    }

    public bool CanSelectCharacter(CharacterDefinition character)
    {
        return character != null && GetStatus(character) == CharacterStatus.Available;
    }

    public string GetDisplayName(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null && !string.IsNullOrWhiteSpace(state.DisplayName) ? state.DisplayName : character != null ? character.DisplayName : string.Empty;
    }

    public int GetAge(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null ? state.Age : character != null ? character.StartingAge : 0;
    }

    public CharacterStatus GetStatus(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null ? state.Status : CharacterStatus.Available;
    }

    public VillageJob GetVillageJob(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null ? state.VillageJob : character != null ? character.StartingVillageJob : VillageJob.None;
    }

    public int GetRunsUntilReturn(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null ? state.RunsUntilReturn : 0;
    }

    public int GetStress(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null ? state.Stress : 0;
    }

    public int GetMaxStress(CharacterDefinition character)
    {
        return BaseMaxStress + (GetStressToleranceBonus(character) * Mathf.Max(0, maxStressPerToleranceBonus));
    }

    public float GetStressPercent(CharacterDefinition character)
    {
        int maxStress = GetMaxStress(character);

        if (maxStress <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01((float)GetStress(character) / maxStress);
    }

    public float GetSanityPercent(CharacterDefinition character)
    {
        return 1f - GetStressPercent(character);
    }

    public void SetStress(CharacterDefinition character, int stress)
    {
        CharacterRuntimeState state = GetRuntimeState(character);

        if (character == null || state == null)
        {
            return;
        }

        int clampedStress = Mathf.Clamp(stress, 0, GetMaxStress(character));

        if (state.Stress == clampedStress)
        {
            return;
        }

        state.Stress = clampedStress;
        CharacterStatusChanged?.Invoke();
    }

    public void AddStress(CharacterDefinition character, int amount)
    {
        if (amount == 0)
        {
            return;
        }

        SetStress(character, GetStress(character) + amount);
    }

    public void AddStressToActiveCharacter(int amount)
    {
        AddStress(ActiveCharacter, amount);
    }

    public int GetInjurySeverity(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null ? state.InjurySeverity : character != null ? character.StartingInjurySeverity : 0;
    }

    public int GetStressToleranceBonus(CharacterDefinition character)
    {
        int age = GetAge(character);

        if (age < oldAgeStartsAt)
        {
            return 0;
        }

        return 1 + ((age - oldAgeStartsAt) / oldAgePenaltyEveryYears);
    }

    public EntityStats GetEffectiveStats(CharacterDefinition character)
    {
        CharacterRuntimeState state = GetRuntimeState(character);
        return state != null ? state.EffectiveStats : character != null ? character.Stats : null;
    }

    public void ApplyActiveCharacterToScenePlayer()
    {
        if (!applyActiveCharacterToPlayer || ActiveCharacter == null || GetEffectiveStats(ActiveCharacter) == null)
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

        EntityStats effectiveStats = GetEffectiveStats(activeCharacter);

        if (player == null || activeCharacter == null || effectiveStats == null)
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
            statsProvider.SetStats(effectiveStats);
        }

        if (damageable != null)
        {
            EnsurePlayerDefeatHandler(damageable);
            damageable.SetStats(effectiveStats, false);
            damageable.SetCurrentHealth(GetStoredHealth(activeCharacter));
        }

        if (staminaPool != null)
        {
            staminaPool.SetStats(effectiveStats, true);
        }

        if (meleeAttack != null)
        {
            meleeAttack.SetStats(effectiveStats);
        }

        for (int i = 0; i < weaponHitboxes.Length; i++)
        {
            weaponHitboxes[i].SetStats(effectiveStats);
        }
    }

    public int GetStoredHealth(CharacterDefinition character)
    {
        EntityStats effectiveStats = GetEffectiveStats(character);

        if (character == null || effectiveStats == null)
        {
            return 0;
        }

        string characterId = character.CharacterId;

        if (!savedHealthByCharacterId.TryGetValue(characterId, out int currentHealth))
        {
            currentHealth = effectiveStats.MaxHealth;
            savedHealthByCharacterId[characterId] = currentHealth;
        }

        return Mathf.Clamp(currentHealth, 0, effectiveStats.MaxHealth);
    }

    public void SetStoredHealth(CharacterDefinition character, int currentHealth)
    {
        EntityStats effectiveStats = GetEffectiveStats(character);

        if (character == null || effectiveStats == null)
        {
            return;
        }

        savedHealthByCharacterId[character.CharacterId] = Mathf.Clamp(currentHealth, 0, effectiveStats.MaxHealth);
        CharacterStatusChanged?.Invoke();
    }

    public void RecordActiveCharacterHealthFromScenePlayer()
    {
        CharacterDefinition activeCharacter = ActiveCharacter;

        if (activeCharacter == null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            return;
        }

        Damageable damageable = player.GetComponentInParent<Damageable>();

        if (damageable != null)
        {
            SetStoredHealth(activeCharacter, damageable.CurrentHealth);
        }
    }

    public void RecoverRestingCharactersAfterCompletedRun(string runnerCharacterId)
    {
        int recoveryAmount = RestingHealthRecoveredAfterExtractedRun;
        bool changed = AdvanceFallenCharactersAfterCompletedRun(runnerCharacterId);

        if (recoveryAmount <= 0)
        {
            if (changed)
            {
                CharacterStatusChanged?.Invoke();
            }

            return;
        }

        for (int i = 0; i < startingRoster.Count; i++)
        {
            CharacterDefinition character = startingRoster[i];
            EntityStats effectiveStats = GetEffectiveStats(character);

            if (character == null || effectiveStats == null || character.CharacterId == runnerCharacterId || GetStatus(character) != CharacterStatus.Available)
            {
                continue;
            }

            int currentHealth = GetStoredHealth(character);

            if (currentHealth <= 0 || currentHealth >= effectiveStats.MaxHealth)
            {
                continue;
            }

            savedHealthByCharacterId[character.CharacterId] = Mathf.Min(effectiveStats.MaxHealth, currentHealth + recoveryAmount);
            changed = true;
        }

        if (changed)
        {
            CharacterStatusChanged?.Invoke();
        }
    }

    public void MarkActiveCharacterDefeated(int overkillDamage)
    {
        CharacterDefinition activeCharacter = ActiveCharacter;
        CharacterRuntimeState state = GetRuntimeState(activeCharacter);

        if (activeCharacter == null || state == null || state.Status == CharacterStatus.Dead)
        {
            return;
        }

        int yearsAged = Mathf.Max(0, baseYearsAgedOnDefeat) + Mathf.Max(0, overkillDamage);
        state.Age += yearsAged;
        state.InjurySeverity++;
        savedHealthByCharacterId[activeCharacter.CharacterId] = 0;
        PlayerCurrencyPouch.Instance.Clear();

        if (state.Age >= permanentDeathAge)
        {
            state.Status = CharacterStatus.Dead;
            state.RunsUntilReturn = 0;
        }
        else
        {
            state.Status = CharacterStatus.Fallen;
            state.RunsUntilReturn = Mathf.Max(1, 1 + (yearsAged / 10));
        }

        RebuildEffectiveStats(activeCharacter, state);
        CharacterStatusChanged?.Invoke();
    }

    public bool SelectFirstAvailableCharacterIfActiveUnavailable()
    {
        if (CanSelectCharacter(ActiveCharacter))
        {
            return false;
        }

        for (int i = 0; i < startingRoster.Count; i++)
        {
            if (!CanSelectCharacter(startingRoster[i]))
            {
                continue;
            }

            activeCharacterIndex = i;
            ApplyActiveCharacterToScenePlayer();
            ActiveCharacterChanged?.Invoke();
            CharacterStatusChanged?.Invoke();
            return true;
        }

        return false;
    }

    public string BuildHealthSummary(CharacterDefinition character)
    {
        EntityStats effectiveStats = GetEffectiveStats(character);

        if (character == null || effectiveStats == null)
        {
            return "No health";
        }

        return $"{GetStoredHealth(character)}/{effectiveStats.MaxHealth}";
    }

    private void EnsureRosterHealthInitialized()
    {
        for (int i = 0; i < startingRoster.Count; i++)
        {
            GetStoredHealth(startingRoster[i]);
        }
    }

    private static void EnsurePlayerDefeatHandler(Damageable damageable)
    {
        if (damageable == null || damageable.GetComponentInParent<PlayerCharacterDefeatHandler>() != null)
        {
            return;
        }

        damageable.gameObject.AddComponent<PlayerCharacterDefeatHandler>();
    }

    private void EnsureRosterRuntimeStateInitialized()
    {
        for (int i = 0; i < startingRoster.Count; i++)
        {
            CharacterDefinition character = startingRoster[i];

            if (character == null || runtimeStateByCharacterId.ContainsKey(character.CharacterId))
            {
                continue;
            }

            CharacterRuntimeState state = new CharacterRuntimeState
            {
                DisplayName = generateRuntimeNames ? GenerateCharacterName() : character.DisplayName,
                Age = randomizeStartingAge ? GenerateStartingAge() : character.StartingAge,
                Status = CharacterStatus.Available,
                RunsUntilReturn = 0,
                Stress = 0,
                InjurySeverity = character.StartingInjurySeverity,
                VillageJob = character.StartingVillageJob,
                RandomHealthOffset = UnityEngine.Random.Range(randomHealthOffsetMin, randomHealthOffsetMax + 1),
                RandomStaminaOffset = UnityEngine.Random.Range(randomStaminaOffsetMin, randomStaminaOffsetMax),
                RandomStaminaRegenOffset = UnityEngine.Random.Range(randomStaminaRegenOffsetMin, randomStaminaRegenOffsetMax),
                RandomAttackOffset = UnityEngine.Random.Range(randomAttackOffsetMin, randomAttackOffsetMax + 1)
            };

            RebuildEffectiveStats(character, state);
            runtimeStateByCharacterId[character.CharacterId] = state;
        }
    }

    private CharacterRuntimeState GetRuntimeState(CharacterDefinition character)
    {
        if (character == null)
        {
            return null;
        }

        EnsureRosterRuntimeStateInitialized();
        runtimeStateByCharacterId.TryGetValue(character.CharacterId, out CharacterRuntimeState state);
        return state;
    }

    private void RebuildEffectiveStats(CharacterDefinition character, CharacterRuntimeState state)
    {
        if (character == null || character.Stats == null || state == null)
        {
            return;
        }

        int ageHealthOffset = 0;
        float ageStaminaOffset = 0f;
        float ageStaminaRegenOffset = 0f;

        if (state.Age <= youngAgeEndsAt)
        {
            ageHealthOffset += youngHealthBonus;
            ageStaminaOffset += youngStaminaBonus;
            ageStaminaRegenOffset += youngStaminaRegenBonus;
        }

        if (state.Age >= oldAgeStartsAt)
        {
            int oldAgeSteps = 1 + ((state.Age - oldAgeStartsAt) / oldAgePenaltyEveryYears);
            ageHealthOffset -= oldAgeSteps * healthPenaltyPerOldAgeStep;
            ageStaminaOffset -= oldAgeSteps * staminaPenaltyPerOldAgeStep;
            ageStaminaRegenOffset -= oldAgeSteps * staminaRegenPenaltyPerOldAgeStep;
        }

        state.EffectiveStats = character.Stats.CreateRuntimeCopy(
            $"{character.CharacterId} Effective Stats",
            state.RandomHealthOffset + ageHealthOffset,
            state.RandomStaminaOffset + ageStaminaOffset,
            state.RandomStaminaRegenOffset + ageStaminaRegenOffset,
            0f,
            0f,
            state.RandomAttackOffset);
    }

    private bool AdvanceFallenCharactersAfterCompletedRun(string runnerCharacterId)
    {
        bool changed = false;

        for (int i = 0; i < startingRoster.Count; i++)
        {
            CharacterDefinition character = startingRoster[i];
            CharacterRuntimeState state = GetRuntimeState(character);

            if (character == null || state == null || character.CharacterId == runnerCharacterId || state.Status != CharacterStatus.Fallen)
            {
                continue;
            }

            state.RunsUntilReturn = Mathf.Max(0, state.RunsUntilReturn - 1);
            changed = true;

            if (state.RunsUntilReturn == 0)
            {
                state.Status = CharacterStatus.Available;
                RebuildEffectiveStats(character, state);
                savedHealthByCharacterId[character.CharacterId] = Mathf.Max(1, GetEffectiveStats(character).MaxHealth / 2);
            }
        }

        return changed;
    }

    private void SelectCharacterInDirection(int direction)
    {
        if (startingRoster.Count == 0)
        {
            return;
        }

        int startIndex = ActiveCharacterIndex;

        for (int i = 1; i <= startingRoster.Count; i++)
        {
            int nextIndex = (startIndex + (direction * i)) % startingRoster.Count;

            if (nextIndex < 0)
            {
                nextIndex += startingRoster.Count;
            }

            if (CanSelectCharacter(startingRoster[nextIndex]))
            {
                SelectCharacter(nextIndex);
                return;
            }
        }
    }

    private string GenerateCharacterName()
    {
        string firstName = generatedFirstNames != null && generatedFirstNames.Length > 0 ? generatedFirstNames[UnityEngine.Random.Range(0, generatedFirstNames.Length)] : "Nameless";
        string lastName = generatedLastNames != null && generatedLastNames.Length > 0 ? generatedLastNames[UnityEngine.Random.Range(0, generatedLastNames.Length)] : "Wanderer";
        return $"{firstName} {lastName}";
    }

    private int GenerateStartingAge()
    {
        int minAge = Mathf.Clamp(randomStartingAgeMin, 1, Mathf.Max(1, permanentDeathAge - 1));
        int maxAge = Mathf.Clamp(Mathf.Max(minAge, randomStartingAgeMax), minAge, Mathf.Max(minAge, permanentDeathAge - 1));
        return UnityEngine.Random.Range(minAge, maxAge + 1);
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
        EnsureRosterRuntimeStateInitialized();
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

    private class CharacterRuntimeState
    {
        public string DisplayName;
        public int Age;
        public CharacterStatus Status;
        public int RunsUntilReturn;
        public int Stress;
        public int InjurySeverity;
        public VillageJob VillageJob;
        public int RandomHealthOffset;
        public float RandomStaminaOffset;
        public float RandomStaminaRegenOffset;
        public int RandomAttackOffset;
        public EntityStats EffectiveStats;
    }
}
