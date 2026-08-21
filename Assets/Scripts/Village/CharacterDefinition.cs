using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDefinition", menuName = "Dungeon Crawler/Village/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string characterId = "new_character";
    [SerializeField] private string displayName = "New Character";
    [SerializeField] private string roleName = "Adventurer";
    [TextArea]
    [SerializeField] private string description;

    [Header("Combat")]
    [SerializeField] private EntityStats stats;
    [SerializeField] private string[] skillNames;

    [Header("Future Conditions")]
    [SerializeField] private int startingStress;
    [SerializeField] private int startingInjurySeverity;

    public string CharacterId => string.IsNullOrWhiteSpace(characterId) ? name : characterId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string RoleName => roleName;
    public string Description => description;
    public EntityStats Stats => stats;
    public string[] SkillNames => skillNames;
    public int StartingStress => startingStress;
    public int StartingInjurySeverity => startingInjurySeverity;

    private void OnValidate()
    {
        startingStress = Mathf.Max(0, startingStress);
        startingInjurySeverity = Mathf.Max(0, startingInjurySeverity);
    }
}
