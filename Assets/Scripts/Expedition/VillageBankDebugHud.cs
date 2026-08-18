using UnityEngine;

public class VillageBankDebugHud : MonoBehaviour
{
    [SerializeField] private Vector2 position = new Vector2(20f, 158f);
    [SerializeField] private Vector2 size = new Vector2(280f, 74f);

    private void OnGUI()
    {
        VillageBank bank = VillageBank.Instance;

        GUI.Box(new Rect(position.x, position.y, size.x, size.y), "Village Bank");
        GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), $"Gold: {bank.Gold}");
        GUI.Label(new Rect(position.x + 12f, position.y + 50f, size.x - 24f, 24f), $"Relics: {bank.Relics}");
    }
}
