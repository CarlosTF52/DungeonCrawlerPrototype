using System;
using System.Collections.Generic;
using UnityEngine;

public class VillageUpgradeManager : MonoBehaviour
{
    private const string DefaultResourcesPath = "Upgrades";
    private const string NoCharacterUpgradeOwnerId = "__no_active_character__";

    private static VillageUpgradeManager instance;
    private static readonly Dictionary<string, Dictionary<string, int>> savedLevelsByCharacterId = new Dictionary<string, Dictionary<string, int>>();
    private static readonly Dictionary<string, int> savedVillageLevels = new Dictionary<string, int>();

    [SerializeField] private UpgradeDefinition[] upgradeDefinitions;
    [SerializeField] private bool loadResourceDefinitions = true;

    private readonly Dictionary<string, UpgradeDefinition> definitionsById = new Dictionary<string, UpgradeDefinition>();
    private readonly Dictionary<string, Dictionary<string, int>> levelsByCharacterId = new Dictionary<string, Dictionary<string, int>>();
    private readonly Dictionary<string, int> villageLevels = new Dictionary<string, int>();
    private bool hasBuiltDefinitionCache;

    public static VillageUpgradeManager Instance
    {
        get
        {
            EnsureInstanceExists();
            return instance;
        }
    }

    public event Action UpgradesChanged;

    public IReadOnlyDictionary<string, UpgradeDefinition> DefinitionsById => definitionsById;
    public int WeaponDamageBonus => GetEffectTotalForActiveCharacter(UpgradeEffectType.WeaponDamage);
    public int TavernRestingHealthRecoveryBonus => GetVillageEffectTotal(UpgradeEffectType.TavernRestingHealthRecovery);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        instance = null;
        savedLevelsByCharacterId.Clear();
        savedVillageLevels.Clear();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        RebuildDefinitionCache();
        LoadSavedLevels();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        CharacterRosterManager.Instance.ActiveCharacterChanged -= HandleActiveCharacterChanged;
        CharacterRosterManager.Instance.ActiveCharacterChanged += HandleActiveCharacterChanged;
    }

    private void OnDisable()
    {
        CharacterRosterManager rosterManager = FindObjectOfType<CharacterRosterManager>();

        if (rosterManager != null)
        {
            rosterManager.ActiveCharacterChanged -= HandleActiveCharacterChanged;
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        RebuildDefinitionCache();
    }

    public string GetActiveCharacterId()
    {
        CharacterDefinition activeCharacter = CharacterRosterManager.Instance.ActiveCharacter;
        return activeCharacter != null ? activeCharacter.CharacterId : NoCharacterUpgradeOwnerId;
    }

    public string GetActiveCharacterDisplayName()
    {
        CharacterDefinition activeCharacter = CharacterRosterManager.Instance.ActiveCharacter;
        return activeCharacter != null ? CharacterRosterManager.Instance.GetDisplayName(activeCharacter) : "No active character";
    }

    public UpgradeDefinition FindFirstDefinitionByEffect(UpgradeEffectType effectType)
    {
        EnsureDefinitionsLoaded();

        foreach (UpgradeDefinition definition in definitionsById.Values)
        {
            if (definition != null && definition.EffectType == effectType)
            {
                return definition;
            }
        }

        return null;
    }

    public int GetLevelForPurchaseTarget(UpgradeDefinition definition)
    {
        if (definition == null)
        {
            return 0;
        }

        return definition.IsVillageOwned ? GetVillageLevel(definition) : GetLevelForActiveCharacter(definition);
    }

    public string GetPurchaseTargetDisplayName(UpgradeDefinition definition)
    {
        if (definition == null)
        {
            return "No upgrade";
        }

        return definition.IsVillageOwned ? "Village" : GetActiveCharacterDisplayName();
    }

    public int GetLevelForActiveCharacter(UpgradeDefinition definition)
    {
        return GetLevel(GetActiveCharacterId(), definition);
    }

    public int GetLevel(string characterId, UpgradeDefinition definition)
    {
        if (definition == null)
        {
            return 0;
        }

        return GetLevel(characterId, definition.UpgradeId);
    }

    public int GetLevel(string characterId, string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId))
        {
            return 0;
        }

        Dictionary<string, int> characterLevels = GetCharacterLevels(characterId, false);
        return characterLevels != null && characterLevels.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    public int GetVillageLevel(UpgradeDefinition definition)
    {
        if (definition == null)
        {
            return 0;
        }

        return GetVillageLevel(definition.UpgradeId);
    }

    public int GetVillageLevel(string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId))
        {
            return 0;
        }

        return villageLevels.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    public bool IsMaxedForPurchaseTarget(UpgradeDefinition definition)
    {
        return definition != null && GetLevelForPurchaseTarget(definition) >= definition.MaxLevel;
    }

    public bool IsMaxedForActiveCharacter(UpgradeDefinition definition)
    {
        return definition != null && GetLevelForActiveCharacter(definition) >= definition.MaxLevel;
    }

    public int GetNextGoldCostForPurchaseTarget(UpgradeDefinition definition)
    {
        if (definition == null || IsMaxedForPurchaseTarget(definition))
        {
            return 0;
        }

        return definition.GetGoldCostForNextLevel(GetLevelForPurchaseTarget(definition));
    }

    public int GetNextRelicCostForPurchaseTarget(UpgradeDefinition definition)
    {
        if (definition == null || IsMaxedForPurchaseTarget(definition))
        {
            return 0;
        }

        return definition.GetRelicCostForNextLevel(GetLevelForPurchaseTarget(definition));
    }

    public int GetNextGoldCostForActiveCharacter(UpgradeDefinition definition)
    {
        if (definition == null || IsMaxedForActiveCharacter(definition))
        {
            return 0;
        }

        return definition.GetGoldCostForNextLevel(GetLevelForActiveCharacter(definition));
    }

    public int GetNextRelicCostForActiveCharacter(UpgradeDefinition definition)
    {
        if (definition == null || IsMaxedForActiveCharacter(definition))
        {
            return 0;
        }

        return definition.GetRelicCostForNextLevel(GetLevelForActiveCharacter(definition));
    }

    public bool CanPurchase(UpgradeDefinition definition)
    {
        if (definition == null || IsMaxedForPurchaseTarget(definition))
        {
            return false;
        }

        return VillageBank.Instance.CanAfford(GetNextGoldCostForPurchaseTarget(definition), GetNextRelicCostForPurchaseTarget(definition));
    }

    public bool CanPurchaseForActiveCharacter(UpgradeDefinition definition)
    {
        if (definition == null || IsMaxedForActiveCharacter(definition))
        {
            return false;
        }

        return VillageBank.Instance.CanAfford(GetNextGoldCostForActiveCharacter(definition), GetNextRelicCostForActiveCharacter(definition));
    }

    public bool TryPurchase(UpgradeDefinition definition)
    {
        if (definition == null || IsMaxedForPurchaseTarget(definition))
        {
            return false;
        }

        int goldCost = GetNextGoldCostForPurchaseTarget(definition);
        int relicCost = GetNextRelicCostForPurchaseTarget(definition);

        if (!VillageBank.Instance.TrySpend(goldCost, relicCost))
        {
            return false;
        }

        if (definition.IsVillageOwned)
        {
            villageLevels[definition.UpgradeId] = GetVillageLevel(definition.UpgradeId) + 1;
        }
        else
        {
            string characterId = GetActiveCharacterId();
            Dictionary<string, int> characterLevels = GetCharacterLevels(characterId, true);
            characterLevels[definition.UpgradeId] = GetLevel(characterId, definition.UpgradeId) + 1;
        }

        SaveCurrentLevels();
        UpgradesChanged?.Invoke();
        return true;
    }

    public bool TryPurchaseForActiveCharacter(UpgradeDefinition definition)
    {
        return TryPurchase(definition);
    }

    public int GetEffectTotalForActiveCharacter(UpgradeEffectType effectType)
    {
        return GetEffectTotal(GetActiveCharacterId(), effectType);
    }

    public int GetEffectTotal(string characterId, UpgradeEffectType effectType)
    {
        EnsureDefinitionsLoaded();

        int total = 0;

        foreach (UpgradeDefinition definition in definitionsById.Values)
        {
            if (definition != null && definition.IsCharacterOwned && definition.EffectType == effectType)
            {
                total += definition.GetEffectTotal(GetLevel(characterId, definition));
            }
        }

        return total;
    }

    public int GetVillageEffectTotal(UpgradeEffectType effectType)
    {
        EnsureDefinitionsLoaded();

        int total = 0;

        foreach (UpgradeDefinition definition in definitionsById.Values)
        {
            if (definition != null && definition.IsVillageOwned && definition.EffectType == effectType)
            {
                total += definition.GetEffectTotal(GetVillageLevel(definition));
            }
        }

        return total;
    }

    public void ClearCharacterUpgrades(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        if (levelsByCharacterId.Remove(characterId))
        {
            SaveCurrentLevels();
            UpgradesChanged?.Invoke();
        }
    }

    private void HandleActiveCharacterChanged()
    {
        UpgradesChanged?.Invoke();
    }

    private Dictionary<string, int> GetCharacterLevels(string characterId, bool createIfMissing)
    {
        string resolvedCharacterId = string.IsNullOrWhiteSpace(characterId) ? NoCharacterUpgradeOwnerId : characterId;

        if (levelsByCharacterId.TryGetValue(resolvedCharacterId, out Dictionary<string, int> characterLevels))
        {
            return characterLevels;
        }

        if (!createIfMissing)
        {
            return null;
        }

        characterLevels = new Dictionary<string, int>();
        levelsByCharacterId.Add(resolvedCharacterId, characterLevels);
        return characterLevels;
    }

    private void EnsureDefinitionsLoaded()
    {
        if (hasBuiltDefinitionCache)
        {
            return;
        }

        RebuildDefinitionCache();
    }

    private void RebuildDefinitionCache()
    {
        definitionsById.Clear();
        AddDefinitions(upgradeDefinitions);

        if (loadResourceDefinitions)
        {
            AddDefinitions(Resources.LoadAll<UpgradeDefinition>(DefaultResourcesPath));
        }

        hasBuiltDefinitionCache = true;
    }

    private void AddDefinitions(IEnumerable<UpgradeDefinition> definitions)
    {
        if (definitions == null)
        {
            return;
        }

        foreach (UpgradeDefinition definition in definitions)
        {
            if (definition == null)
            {
                continue;
            }

            string upgradeId = definition.UpgradeId;

            if (!definitionsById.ContainsKey(upgradeId))
            {
                definitionsById.Add(upgradeId, definition);
            }
        }
    }

    private void LoadSavedLevels()
    {
        levelsByCharacterId.Clear();
        villageLevels.Clear();

        foreach (KeyValuePair<string, Dictionary<string, int>> savedCharacterLevels in savedLevelsByCharacterId)
        {
            Dictionary<string, int> loadedCharacterLevels = GetCharacterLevels(savedCharacterLevels.Key, true);

            foreach (KeyValuePair<string, int> savedLevel in savedCharacterLevels.Value)
            {
                loadedCharacterLevels[savedLevel.Key] = Mathf.Max(0, savedLevel.Value);
            }
        }

        foreach (KeyValuePair<string, int> savedVillageLevel in savedVillageLevels)
        {
            villageLevels[savedVillageLevel.Key] = Mathf.Max(0, savedVillageLevel.Value);
        }
    }

    private void SaveCurrentLevels()
    {
        savedLevelsByCharacterId.Clear();
        savedVillageLevels.Clear();

        foreach (KeyValuePair<string, Dictionary<string, int>> characterLevels in levelsByCharacterId)
        {
            Dictionary<string, int> savedCharacterLevels = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> level in characterLevels.Value)
            {
                savedCharacterLevels[level.Key] = Mathf.Max(0, level.Value);
            }

            savedLevelsByCharacterId[characterLevels.Key] = savedCharacterLevels;
        }

        foreach (KeyValuePair<string, int> villageLevel in villageLevels)
        {
            savedVillageLevels[villageLevel.Key] = Mathf.Max(0, villageLevel.Value);
        }
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        VillageUpgradeManager sceneManager = FindObjectOfType<VillageUpgradeManager>();

        if (sceneManager != null)
        {
            instance = sceneManager;
            return;
        }

        GameObject managerObject = new GameObject("VillageUpgradeManager");
        managerObject.AddComponent<VillageUpgradeManager>();
    }
}
