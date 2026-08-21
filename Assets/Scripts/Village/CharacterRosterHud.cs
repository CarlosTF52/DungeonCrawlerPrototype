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

    [Header("Labels")]
    [SerializeField] private string emptyRosterText = "No character selected";
    [SerializeField] private string healthLabel = "Health";
    [SerializeField] private string staminaLabel = "Stamina";
    [SerializeField] private string attackLabel = "Attack";
    [SerializeField] private string stressLabel = "Stress";
    [SerializeField] private string injuryLabel = "Injury";

    private CharacterRosterManager rosterManager;
    private CharacterDefinition displayedCharacter;
    private int displayedHealth = int.MinValue;
    private float displayedStamina = float.MinValue;
    private int displayedAttack = int.MinValue;
    private int displayedStress = int.MinValue;
    private int displayedInjury = int.MinValue;
    private readonly StringBuilder builder = new StringBuilder();

    private void OnEnable()
    {
        rosterManager = CharacterRosterManager.Instance;
        rosterManager.ActiveCharacterChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (rosterManager != null)
        {
            rosterManager.ActiveCharacterChanged -= Refresh;
        }
    }

    private void LateUpdate()
    {
        if (NeedsRefresh())
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        CharacterDefinition activeCharacter = rosterManager != null ? rosterManager.ActiveCharacter : null;
        displayedCharacter = activeCharacter;

        if (contentRoot != null)
        {
            contentRoot.SetActive(activeCharacter != null);
        }

        if (activeCharacter == null)
        {
            displayedHealth = int.MinValue;
            displayedStamina = float.MinValue;
            displayedAttack = int.MinValue;
            displayedStress = int.MinValue;
            displayedInjury = int.MinValue;
            SetText(nameText, emptyRosterText);
            SetText(roleText, string.Empty);
            SetText(vitalsText, string.Empty);
            SetText(offenseText, string.Empty);
            SetText(conditionText, string.Empty);
            SetText(skillsText, string.Empty);
            SetText(descriptionText, string.Empty);
            return;
        }

        EntityStats stats = activeCharacter.Stats;
        displayedHealth = stats != null ? stats.MaxHealth : int.MinValue;
        displayedStamina = stats != null ? stats.MaxStamina : float.MinValue;
        displayedAttack = stats != null ? stats.AttackPower : int.MinValue;
        displayedStress = activeCharacter.StartingStress;
        displayedInjury = activeCharacter.StartingInjurySeverity;

        SetText(nameText, activeCharacter.DisplayName);
        SetText(roleText, activeCharacter.RoleName);
        SetText(vitalsText, stats != null ? $"{healthLabel}: {stats.MaxHealth}   {staminaLabel}: {stats.MaxStamina:0.#}" : "No stats assigned");
        SetText(offenseText, stats != null ? $"{attackLabel}: {stats.AttackPower}" : string.Empty);
        SetText(conditionText, $"{stressLabel}: {activeCharacter.StartingStress}   {injuryLabel}: {activeCharacter.StartingInjurySeverity}");
        SetText(skillsText, BuildSkillsText(activeCharacter.SkillNames));
        SetText(descriptionText, activeCharacter.Description);
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

        EntityStats stats = activeCharacter.Stats;
        int health = stats != null ? stats.MaxHealth : int.MinValue;
        float stamina = stats != null ? stats.MaxStamina : float.MinValue;
        int attack = stats != null ? stats.AttackPower : int.MinValue;

        return health != displayedHealth
            || !Mathf.Approximately(stamina, displayedStamina)
            || attack != displayedAttack
            || activeCharacter.StartingStress != displayedStress
            || activeCharacter.StartingInjurySeverity != displayedInjury;
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
