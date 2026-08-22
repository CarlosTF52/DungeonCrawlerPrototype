using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TavernUpgradeInteractable : MonoBehaviour
{
    [SerializeField] private UpgradeDefinition upgradeDefinition;
    [SerializeField] private UpgradeEffectType fallbackEffectType = UpgradeEffectType.TavernRestingHealthRecovery;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to improve tavern";
    [SerializeField] private bool showUpgradeDetails = true;

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

    private void Update()
    {
        if (isPlayerInRange && WasInteractionPressed())
        {
            TryUpgrade();
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

        string text = showUpgradeDetails ? $"{promptText}\n{BuildUpgradeSummary()}" : promptText;
        GUI.Label(new Rect(20f, Screen.height - 205f, 600f, 96f), text);
    }

    public void TryUpgrade()
    {
        UpgradeDefinition definition = ResolveUpgradeDefinition();

        if (definition == null)
        {
            Debug.LogWarning("Tavern upgrade failed: no upgrade definition assigned or found.", this);
            return;
        }

        if (VillageUpgradeManager.Instance.TryPurchase(definition))
        {
            Debug.Log($"Purchased village upgrade: {definition.DisplayName} level {VillageUpgradeManager.Instance.GetLevelForPurchaseTarget(definition)}.", this);
            return;
        }

        Debug.LogWarning($"Tavern upgrade failed: {BuildUpgradeSummary()}", this);
    }

    private string BuildUpgradeSummary()
    {
        UpgradeDefinition definition = ResolveUpgradeDefinition();

        if (definition == null)
        {
            return "No upgrade configured.";
        }

        VillageUpgradeManager upgrades = VillageUpgradeManager.Instance;
        CharacterRosterManager roster = CharacterRosterManager.Instance;
        int level = upgrades.GetLevelForPurchaseTarget(definition);
        int bonus = definition.GetEffectTotal(level);
        string targetName = upgrades.GetPurchaseTargetDisplayName(definition);
        string recoverySummary = $"Rest recovery: {roster.RestingHealthRecoveredAfterExtractedRun} HP/run";

        if (upgrades.IsMaxedForPurchaseTarget(definition))
        {
            return $"{targetName} - {definition.DisplayName}: Level {level}/{definition.MaxLevel}  Bonus +{bonus}\n{recoverySummary}  Maxed";
        }

        int goldCost = upgrades.GetNextGoldCostForPurchaseTarget(definition);
        int relicCost = upgrades.GetNextRelicCostForPurchaseTarget(definition);
        string affordability = upgrades.CanPurchase(definition) ? "Ready" : "Need banked resources";
        return $"{targetName} - {definition.DisplayName}: Level {level}/{definition.MaxLevel}  Bonus +{bonus}\nCost: {goldCost} gold, {relicCost} relics  {recoverySummary}  {affordability}";
    }

    private UpgradeDefinition ResolveUpgradeDefinition()
    {
        if (upgradeDefinition != null)
        {
            return upgradeDefinition;
        }

        return VillageUpgradeManager.Instance.FindFirstDefinitionByEffect(fallbackEffectType);
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
