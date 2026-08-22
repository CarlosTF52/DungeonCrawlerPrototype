using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Dungeon Crawler/Village/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string upgradeId = "new_upgrade";
    [SerializeField] private string displayName = "New Upgrade";
    [TextArea]
    [SerializeField] private string description;

    [Header("Progression")]
    [SerializeField] private UpgradeEffectType effectType = UpgradeEffectType.WeaponDamage;
    [SerializeField] private UpgradeOwnerScope ownerScope = UpgradeOwnerScope.ActiveCharacter;
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private int effectAmountPerLevel = 1;

    [Header("Cost")]
    [SerializeField] private int baseGoldCost = 25;
    [SerializeField] private int goldCostIncreasePerLevel = 15;
    [SerializeField] private int baseRelicCost;
    [SerializeField] private int relicCostIncreasePerLevel;

    public string UpgradeId => string.IsNullOrWhiteSpace(upgradeId) ? name : upgradeId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public UpgradeEffectType EffectType => effectType;
    public UpgradeOwnerScope OwnerScope => ownerScope;
    public int MaxLevel => maxLevel;
    public int EffectAmountPerLevel => effectAmountPerLevel;
    public bool IsVillageOwned => ownerScope == UpgradeOwnerScope.Village;
    public bool IsCharacterOwned => ownerScope == UpgradeOwnerScope.ActiveCharacter;

    public int GetGoldCostForNextLevel(int currentLevel)
    {
        return Mathf.Max(0, baseGoldCost + Mathf.Max(0, currentLevel) * goldCostIncreasePerLevel);
    }

    public int GetRelicCostForNextLevel(int currentLevel)
    {
        return Mathf.Max(0, baseRelicCost + Mathf.Max(0, currentLevel) * relicCostIncreasePerLevel);
    }

    public int GetEffectTotal(int currentLevel)
    {
        return Mathf.Max(0, currentLevel) * effectAmountPerLevel;
    }

    private void OnValidate()
    {
        maxLevel = Mathf.Max(1, maxLevel);
        effectAmountPerLevel = Mathf.Max(0, effectAmountPerLevel);
        baseGoldCost = Mathf.Max(0, baseGoldCost);
        goldCostIncreasePerLevel = Mathf.Max(0, goldCostIncreasePerLevel);
        baseRelicCost = Mathf.Max(0, baseRelicCost);
        relicCostIncreasePerLevel = Mathf.Max(0, relicCostIncreasePerLevel);
    }
}
