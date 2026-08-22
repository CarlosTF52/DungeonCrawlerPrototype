using TMPro;
using UnityEngine;

public class PlayerCharacterHud : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text vitalsText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private TMP_Text combatText;
    [SerializeField] private GameObject contentRoot;

    [Header("Labels")]
    [SerializeField] private string noCharacterText = "No active character";
    [SerializeField] private string ageLabel = "Age";
    [SerializeField] private string statusLabel = "Status";
    [SerializeField] private string jobLabel = "Job";
    [SerializeField] private string healthLabel = "Health";
    [SerializeField] private string staminaLabel = "Stamina";
    [SerializeField] private string injuryLabel = "Injury";
    [SerializeField] private string stressLabel = "Stress";
    [SerializeField] private string stressToleranceLabel = "Stress Tol";
    [SerializeField] private string attackLabel = "Attack";

    private CharacterRosterManager rosterManager;
    private CharacterDefinition displayedCharacter;
    private int displayedHealth = int.MinValue;
    private int displayedMaxHealth = int.MinValue;
    private float displayedMaxStamina = float.MinValue;
    private int displayedAge = int.MinValue;
    private CharacterStatus displayedStatus;
    private VillageJob displayedJob;
    private int displayedInjury = int.MinValue;
    private int displayedStress = int.MinValue;
    private int displayedMaxStress = int.MinValue;
    private int displayedStressTolerance = int.MinValue;
    private int displayedAttack = int.MinValue;

    private void OnEnable()
    {
        RefreshManagerReference(true);
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        RefreshManagerReference(false);

        if (NeedsRefresh())
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        CharacterDefinition activeCharacter = rosterManager != null ? rosterManager.ActiveCharacter : null;
        displayedCharacter = activeCharacter;

        if (activeCharacter == null)
        {
            SetContentVisible(false);
            SetText(nameText, noCharacterText);
            SetText(statusText, string.Empty);
            SetText(vitalsText, string.Empty);
            SetText(conditionText, string.Empty);
            SetText(combatText, string.Empty);
            return;
        }

        SetContentVisible(true);

        EntityStats stats = rosterManager.GetEffectiveStats(activeCharacter);
        displayedHealth = rosterManager.GetStoredHealth(activeCharacter);
        displayedMaxHealth = stats != null ? stats.MaxHealth : 0;
        displayedMaxStamina = stats != null ? stats.MaxStamina : 0f;
        displayedAge = rosterManager.GetAge(activeCharacter);
        displayedStatus = rosterManager.GetStatus(activeCharacter);
        displayedJob = rosterManager.GetVillageJob(activeCharacter);
        displayedInjury = rosterManager.GetInjurySeverity(activeCharacter);
        displayedStress = rosterManager.GetStress(activeCharacter);
        displayedMaxStress = rosterManager.GetMaxStress(activeCharacter);
        displayedStressTolerance = rosterManager.GetStressToleranceBonus(activeCharacter);
        displayedAttack = stats != null ? stats.AttackPower : 0;

        string stressToleranceSummary = displayedStressTolerance > 0 ? $"+{displayedStressTolerance}" : "0";

        SetText(nameText, rosterManager.GetDisplayName(activeCharacter));
        SetText(statusText, $"{ageLabel}: {displayedAge}   {statusLabel}: {displayedStatus}   {jobLabel}: {displayedJob}");
        SetText(vitalsText, $"{healthLabel}: {displayedHealth}/{displayedMaxHealth}   {staminaLabel}: {displayedMaxStamina:0.#}");
        SetText(conditionText, $"{injuryLabel}: {displayedInjury}   {stressLabel}: {displayedStress}/{displayedMaxStress}   {stressToleranceLabel}: {stressToleranceSummary}");
        SetText(combatText, $"{attackLabel}: {displayedAttack}");
    }

    private bool NeedsRefresh()
    {
        CharacterDefinition activeCharacter = rosterManager != null ? rosterManager.ActiveCharacter : null;

        if (activeCharacter != displayedCharacter)
        {
            return true;
        }

        if (activeCharacter == null)
        {
            return false;
        }

        EntityStats stats = rosterManager.GetEffectiveStats(activeCharacter);
        int health = rosterManager.GetStoredHealth(activeCharacter);
        int maxHealth = stats != null ? stats.MaxHealth : 0;
        float maxStamina = stats != null ? stats.MaxStamina : 0f;
        int attack = stats != null ? stats.AttackPower : 0;

        return health != displayedHealth
            || maxHealth != displayedMaxHealth
            || !Mathf.Approximately(maxStamina, displayedMaxStamina)
            || rosterManager.GetAge(activeCharacter) != displayedAge
            || rosterManager.GetStatus(activeCharacter) != displayedStatus
            || rosterManager.GetVillageJob(activeCharacter) != displayedJob
            || rosterManager.GetInjurySeverity(activeCharacter) != displayedInjury
            || rosterManager.GetStress(activeCharacter) != displayedStress
            || rosterManager.GetMaxStress(activeCharacter) != displayedMaxStress
            || rosterManager.GetStressToleranceBonus(activeCharacter) != displayedStressTolerance
            || attack != displayedAttack;
    }

    private void RefreshManagerReference(bool force)
    {
        CharacterRosterManager currentManager = CharacterRosterManager.Instance;

        if (!force && rosterManager == currentManager)
        {
            return;
        }

        Unsubscribe();
        rosterManager = currentManager;
        Subscribe();
    }

    private void Subscribe()
    {
        if (rosterManager == null)
        {
            return;
        }

        rosterManager.ActiveCharacterChanged -= Refresh;
        rosterManager.CharacterStatusChanged -= Refresh;
        rosterManager.ActiveCharacterChanged += Refresh;
        rosterManager.CharacterStatusChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (rosterManager == null)
        {
            return;
        }

        rosterManager.ActiveCharacterChanged -= Refresh;
        rosterManager.CharacterStatusChanged -= Refresh;
    }

    private void SetContentVisible(bool visible)
    {
        if (contentRoot != null)
        {
            contentRoot.SetActive(visible);
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
