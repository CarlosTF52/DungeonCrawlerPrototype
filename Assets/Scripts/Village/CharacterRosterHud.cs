using System.Text;
using TMPro;
using UnityEngine;

public class CharacterRosterHud : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text vitalsText;
    [SerializeField] private TMP_Text offenseText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private TMP_Text skillsText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private bool showOnlyNearTavernSelector = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool forceEnableCanvasParents = true;

    [Header("Labels")]
    [SerializeField] private string emptyRosterText = "No character selected";
    [SerializeField] private string healthLabel = "Health";
    [SerializeField] private string staminaLabel = "Stamina";
    [SerializeField] private string staminaRegenLabel = "Regen";
    [SerializeField] private string attackLabel = "Attack";
    [SerializeField] private string ageLabel = "Age";
    [SerializeField] private string statusLabel = "Status";
    [SerializeField] private string jobLabel = "Job";
    [SerializeField] private string stressLabel = "Stress";
    [SerializeField] private string stressToleranceLabel = "Stress Tol";
    [SerializeField] private string injuryLabel = "Injury";

    private CharacterRosterManager rosterManager;
    private VillageUpgradeManager upgradeManager;
    private CharacterDefinition displayedCharacter;
    private bool displayedVisibility;
    private int displayedHealth = int.MinValue;
    private float displayedStamina = float.MinValue;
    private float displayedStaminaRegen = float.MinValue;
    private int displayedAttack = int.MinValue;
    private int displayedWeaponDamageBonus = int.MinValue;
    private int displayedAge = int.MinValue;
    private CharacterStatus displayedStatus;
    private VillageJob displayedJob;
    private int displayedStress = int.MinValue;
    private int displayedStressToleranceBonus = int.MinValue;
    private int displayedInjury = int.MinValue;
    private bool contentRootCanBeDisabled = true;
    private readonly StringBuilder builder = new StringBuilder();

    private void Awake()
    {
        contentRootCanBeDisabled = CanSafelyDisableContentRoot();
    }

    private void OnEnable()
    {
        contentRootCanBeDisabled = CanSafelyDisableContentRoot();
        RefreshManagerReferences(true);
        rosterManager.ActiveCharacterChanged += Refresh;
        rosterManager.CharacterStatusChanged += Refresh;
        upgradeManager.UpgradesChanged += Refresh;
        TavernCharacterSelector.PlayerRangeChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (rosterManager != null)
        {
            rosterManager.ActiveCharacterChanged -= Refresh;
            rosterManager.CharacterStatusChanged -= Refresh;
        }

        if (upgradeManager != null)
        {
            upgradeManager.UpgradesChanged -= Refresh;
        }

        TavernCharacterSelector.PlayerRangeChanged -= Refresh;
    }

    private void LateUpdate()
    {
        RefreshManagerReferences(false);

        if (NeedsRefresh())
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        CharacterDefinition activeCharacter = rosterManager != null ? rosterManager.ActiveCharacter : null;
        displayedCharacter = activeCharacter;

        if (!ShouldShowHud(activeCharacter))
        {
            HideHud();
            return;
        }

        displayedVisibility = true;

        EnsureVisibleParentsEnabled();

        if (contentRoot != null)
        {
            contentRoot.SetActive(true);
        }

        EntityStats stats = rosterManager.GetEffectiveStats(activeCharacter);
        displayedHealth = stats != null ? rosterManager.GetStoredHealth(activeCharacter) : int.MinValue;
        displayedStamina = stats != null ? stats.MaxStamina : float.MinValue;
        displayedStaminaRegen = stats != null ? stats.StaminaRegenPerSecond : float.MinValue;
        displayedAttack = stats != null ? stats.AttackPower : int.MinValue;
        displayedWeaponDamageBonus = GetWeaponDamageBonus(activeCharacter);
        displayedAge = rosterManager.GetAge(activeCharacter);
        displayedStatus = rosterManager.GetStatus(activeCharacter);
        displayedJob = rosterManager.GetVillageJob(activeCharacter);
        displayedStress = rosterManager.GetStress(activeCharacter);
        displayedStressToleranceBonus = rosterManager.GetStressToleranceBonus(activeCharacter);
        displayedInjury = rosterManager.GetInjurySeverity(activeCharacter);

        SetText(nameText, rosterManager.GetDisplayName(activeCharacter));
        SetText(roleText, activeCharacter.RoleName);
        SetText(vitalsText, stats != null ? $"{healthLabel}: {displayedHealth}/{stats.MaxHealth}   {staminaLabel}: {stats.MaxStamina:0.#}   {staminaRegenLabel}: {stats.StaminaRegenPerSecond:0.##}/s" : "No stats assigned");
        SetText(offenseText, stats != null ? BuildAttackText(displayedAttack, displayedWeaponDamageBonus) : string.Empty);
        SetText(conditionText, BuildConditionText(activeCharacter));
        SetText(skillsText, BuildSkillsText(activeCharacter.SkillNames));
        SetText(descriptionText, activeCharacter.Description);
    }

    private bool NeedsRefresh()
    {
        CharacterDefinition activeCharacter = rosterManager != null ? rosterManager.ActiveCharacter : null;
        bool shouldShowHud = ShouldShowHud(activeCharacter);

        if (shouldShowHud != displayedVisibility)
        {
            return true;
        }

        if (activeCharacter != displayedCharacter)
        {
            return true;
        }

        if (!shouldShowHud || activeCharacter == null)
        {
            return false;
        }

        EntityStats stats = rosterManager.GetEffectiveStats(activeCharacter);
        int health = stats != null ? rosterManager.GetStoredHealth(activeCharacter) : int.MinValue;
        float stamina = stats != null ? stats.MaxStamina : float.MinValue;
        float staminaRegen = stats != null ? stats.StaminaRegenPerSecond : float.MinValue;
        int attack = stats != null ? stats.AttackPower : int.MinValue;
        int weaponDamageBonus = GetWeaponDamageBonus(activeCharacter);

        return health != displayedHealth
            || !Mathf.Approximately(stamina, displayedStamina)
            || !Mathf.Approximately(staminaRegen, displayedStaminaRegen)
            || attack != displayedAttack
            || weaponDamageBonus != displayedWeaponDamageBonus
            || rosterManager.GetAge(activeCharacter) != displayedAge
            || rosterManager.GetStatus(activeCharacter) != displayedStatus
            || rosterManager.GetVillageJob(activeCharacter) != displayedJob
            || rosterManager.GetStress(activeCharacter) != displayedStress
            || rosterManager.GetStressToleranceBonus(activeCharacter) != displayedStressToleranceBonus
            || rosterManager.GetInjurySeverity(activeCharacter) != displayedInjury;
    }

    private bool ShouldShowHud(CharacterDefinition activeCharacter)
    {
        if (activeCharacter == null)
        {
            return false;
        }

        return !showOnlyNearTavernSelector || TavernCharacterSelector.IsPlayerInAnySelectorRangeFor(playerTag);
    }

    private void RefreshManagerReferences(bool force)
    {
        CharacterRosterManager currentRosterManager = CharacterRosterManager.Instance;
        VillageUpgradeManager currentUpgradeManager = VillageUpgradeManager.Instance;

        if (!force && rosterManager == currentRosterManager && upgradeManager == currentUpgradeManager)
        {
            return;
        }

        if (rosterManager != null && rosterManager != currentRosterManager)
        {
            rosterManager.ActiveCharacterChanged -= Refresh;
            rosterManager.CharacterStatusChanged -= Refresh;
        }

        if (upgradeManager != null && upgradeManager != currentUpgradeManager)
        {
            upgradeManager.UpgradesChanged -= Refresh;
        }

        rosterManager = currentRosterManager;
        upgradeManager = currentUpgradeManager;

        if (!force)
        {
            if (rosterManager != null)
            {
                rosterManager.ActiveCharacterChanged -= Refresh;
                rosterManager.CharacterStatusChanged -= Refresh;
                rosterManager.ActiveCharacterChanged += Refresh;
                rosterManager.CharacterStatusChanged += Refresh;
            }

            if (upgradeManager != null)
            {
                upgradeManager.UpgradesChanged -= Refresh;
                upgradeManager.UpgradesChanged += Refresh;
            }
        }
    }

    private void EnsureVisibleParentsEnabled()
    {
        if (!forceEnableCanvasParents)
        {
            return;
        }

        Transform current = contentRoot != null ? contentRoot.transform.parent : transform.parent;

        while (current != null)
        {
            if (current.GetComponent<Canvas>() != null || current.GetComponent<CanvasGroup>() != null)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    private void HideHud()
    {
        displayedVisibility = false;
        displayedHealth = int.MinValue;
        displayedStamina = float.MinValue;
        displayedStaminaRegen = float.MinValue;
        displayedAttack = int.MinValue;
        displayedWeaponDamageBonus = int.MinValue;
        displayedAge = int.MinValue;
        displayedStress = int.MinValue;
        displayedStressToleranceBonus = int.MinValue;
        displayedInjury = int.MinValue;

        if (contentRoot != null && contentRootCanBeDisabled)
        {
            contentRoot.SetActive(false);
        }

        SetText(nameText, string.Empty);
        SetText(roleText, string.Empty);
        SetText(vitalsText, string.Empty);
        SetText(offenseText, string.Empty);
        SetText(conditionText, string.Empty);
        SetText(skillsText, string.Empty);
        SetText(descriptionText, string.Empty);
    }

    private bool CanSafelyDisableContentRoot()
    {
        if (contentRoot == null)
        {
            return true;
        }

        Transform contentTransform = contentRoot.transform;
        Transform hudTransform = transform;

        return contentTransform != hudTransform && !hudTransform.IsChildOf(contentTransform);
    }

    private string BuildAttackText(int baseAttack, int weaponDamageBonus)
    {
        int effectiveAttack = Mathf.Max(1, baseAttack + weaponDamageBonus);

        if (weaponDamageBonus <= 0)
        {
            return $"{attackLabel}: {effectiveAttack}";
        }

        return $"{attackLabel}: {effectiveAttack}  ({baseAttack} +{weaponDamageBonus} weapon)";
    }

    private string BuildConditionText(CharacterDefinition character)
    {
        int stressToleranceBonus = rosterManager.GetStressToleranceBonus(character);
        string stressToleranceSummary = stressToleranceBonus > 0 ? $"+{stressToleranceBonus}" : "0";
        string returnSummary = rosterManager.GetStatus(character) == CharacterStatus.Fallen ? $"  Return: {rosterManager.GetRunsUntilReturn(character)} run(s)" : string.Empty;

        return $"{ageLabel}: {rosterManager.GetAge(character)}   {statusLabel}: {rosterManager.GetStatus(character)}   {jobLabel}: {rosterManager.GetVillageJob(character)}\n"
            + $"{stressLabel}: {rosterManager.GetStress(character)}   {stressToleranceLabel}: {stressToleranceSummary}   {injuryLabel}: {rosterManager.GetInjurySeverity(character)}{returnSummary}";
    }

    private int GetWeaponDamageBonus(CharacterDefinition character)
    {
        if (upgradeManager == null || character == null)
        {
            return 0;
        }

        return upgradeManager.GetEffectTotal(character.CharacterId, UpgradeEffectType.WeaponDamage);
    }

    private string BuildSkillsText(string[] skillNames)
    {
        if (skillNames == null || skillNames.Length == 0)
        {
            return "Skills: None";
        }

        builder.Clear();
        builder.Append("Skills: ");

        for (int i = 0; i < skillNames.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(skillNames[i]);
        }

        return builder.ToString();
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
