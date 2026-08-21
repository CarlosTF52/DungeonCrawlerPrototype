using System.Text;
using UnityEngine;

public class CharacterRosterDebugHud : MonoBehaviour
{
    [SerializeField] private Vector2 position = new Vector2(20f, 300f);
    [SerializeField] private Vector2 size = new Vector2(400f, 190f);

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

        EntityStats stats = activeCharacter.Stats;

        GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), $"Active: {activeCharacter.DisplayName}  ({activeCharacter.RoleName})");
        GUI.Label(new Rect(position.x + 12f, position.y + 52f, size.x - 24f, 24f), stats != null ? $"Health: {stats.MaxHealth}  Stamina: {stats.MaxStamina:0.#}  Attack: {stats.AttackPower}" : "No stats assigned.");
        GUI.Label(new Rect(position.x + 12f, position.y + 76f, size.x - 24f, 24f), $"Stress: {activeCharacter.StartingStress}  Injury: {activeCharacter.StartingInjurySeverity}");
        GUI.Label(new Rect(position.x + 12f, position.y + 100f, size.x - 24f, 24f), BuildSkillsText(activeCharacter.SkillNames));
        GUI.Label(new Rect(position.x + 12f, position.y + 124f, size.x - 24f, 52f), activeCharacter.Description);
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
