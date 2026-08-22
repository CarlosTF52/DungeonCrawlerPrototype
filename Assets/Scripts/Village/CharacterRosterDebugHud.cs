using System.Text;
using UnityEngine;

public class CharacterRosterDebugHud : MonoBehaviour
{
    [SerializeField] private Vector2 position = new Vector2(20f, 300f);
    [SerializeField] private Vector2 size = new Vector2(460f, 230f);

    private readonly StringBuilder builder = new StringBuilder();

    private void OnGUI()
    {
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        CharacterDefinition activeCharacter = rosterManager.ActiveCharacter;

        GUI.Box(new Rect(position.x, position.y, size.x, size.y), "Roster");

        if (activeCharacter == null)
        {
            GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), "No active character configured.");
            return;
        }

        EntityStats stats = rosterManager.GetEffectiveStats(activeCharacter);
        int weaponDamageBonus = VillageUpgradeManager.Instance.GetEffectTotal(activeCharacter.CharacterId, UpgradeEffectType.WeaponDamage);

        GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), $"Active: {rosterManager.GetDisplayName(activeCharacter)}  ({activeCharacter.RoleName})");
        GUI.Label(new Rect(position.x + 12f, position.y + 52f, size.x - 24f, 24f), stats != null ? $"Health: {rosterManager.GetStoredHealth(activeCharacter)}/{stats.MaxHealth}  Stamina: {stats.MaxStamina:0.#}  Regen: {stats.StaminaRegenPerSecond:0.##}/s  Attack: {BuildAttackText(stats.AttackPower, weaponDamageBonus)}" : "No stats assigned.");
        GUI.Label(new Rect(position.x + 12f, position.y + 76f, size.x - 24f, 24f), $"Age: {rosterManager.GetAge(activeCharacter)}  Status: {rosterManager.GetStatus(activeCharacter)}  Job: {rosterManager.GetVillageJob(activeCharacter)}");
        GUI.Label(new Rect(position.x + 12f, position.y + 100f, size.x - 24f, 24f), $"Stress: {rosterManager.GetStress(activeCharacter)}  Stress Tol: +{rosterManager.GetStressToleranceBonus(activeCharacter)}  Injury: {rosterManager.GetInjurySeverity(activeCharacter)}  Return: {rosterManager.GetRunsUntilReturn(activeCharacter)}");
        GUI.Label(new Rect(position.x + 12f, position.y + 124f, size.x - 24f, 24f), BuildSkillsText(activeCharacter.SkillNames));
        GUI.Label(new Rect(position.x + 12f, position.y + 148f, size.x - 24f, 68f), activeCharacter.Description);
    }

    private static string BuildAttackText(int baseAttack, int weaponDamageBonus)
    {
        int effectiveAttack = Mathf.Max(1, baseAttack + weaponDamageBonus);
        return weaponDamageBonus > 0 ? $"{effectiveAttack} ({baseAttack} +{weaponDamageBonus})" : effectiveAttack.ToString();
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
}
