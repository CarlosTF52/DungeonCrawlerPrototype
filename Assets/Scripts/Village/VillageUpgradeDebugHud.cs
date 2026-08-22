using UnityEngine;

public class VillageUpgradeDebugHud : MonoBehaviour
{
    [SerializeField] private UpgradeDefinition trackedUpgrade;
    [SerializeField] private UpgradeEffectType fallbackEffectType = UpgradeEffectType.WeaponDamage;
    [SerializeField] private Vector2 position = new Vector2(20f, 500f);
    [SerializeField] private Vector2 size = new Vector2(460f, 154f);

    private void OnGUI()
    {
        VillageUpgradeManager upgrades = VillageUpgradeManager.Instance;
        UpgradeDefinition definition = trackedUpgrade != null ? trackedUpgrade : upgrades.FindFirstDefinitionByEffect(fallbackEffectType);

        GUI.Box(new Rect(position.x, position.y, size.x, size.y), "Upgrades");

        if (definition == null)
        {
            GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), "No upgrade definition configured.");
            return;
        }

        int level = upgrades.GetLevelForPurchaseTarget(definition);
        int bonus = definition.GetEffectTotal(level);
        VillageBank bank = VillageBank.Instance;
        CharacterRosterManager roster = CharacterRosterManager.Instance;

        GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), $"{upgrades.GetPurchaseTargetDisplayName(definition)}: {definition.DisplayName} {level}/{definition.MaxLevel}  Bonus +{bonus}");
        GUI.Label(new Rect(position.x + 12f, position.y + 52f, size.x - 24f, 24f), $"Weapon Damage Bonus: +{upgrades.WeaponDamageBonus}");
        GUI.Label(new Rect(position.x + 12f, position.y + 76f, size.x - 24f, 24f), $"Tavern Recovery: {roster.RestingHealthRecoveredAfterExtractedRun} HP/run (+{upgrades.TavernRestingHealthRecoveryBonus})");
        GUI.Label(new Rect(position.x + 12f, position.y + 100f, size.x - 24f, 24f), upgrades.IsMaxedForPurchaseTarget(definition) ? "Next Cost: Maxed" : $"Next Cost: {upgrades.GetNextGoldCostForPurchaseTarget(definition)} gold, {upgrades.GetNextRelicCostForPurchaseTarget(definition)} relics");
        GUI.Label(new Rect(position.x + 12f, position.y + 124f, size.x - 24f, 24f), $"Bank: {bank.Gold} gold, {bank.Relics} relics");
    }
}
