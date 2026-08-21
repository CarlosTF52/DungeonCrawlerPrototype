using UnityEngine;

public class VillageBankDebugHud : MonoBehaviour
{
    [SerializeField] private Vector2 position = new Vector2(20f, 158f);
    [SerializeField] private Vector2 size = new Vector2(320f, 122f);

    private void OnGUI()
    {
        VillageBank bank = VillageBank.Instance;
        PlayerCurrencyPouch pouch = PlayerCurrencyPouch.Instance;

        GUI.Box(new Rect(position.x, position.y, size.x, size.y), "Village Currency");
        GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), $"Carried Gold: {pouch.Gold}  Relics: {pouch.Relics}");
        GUI.Label(new Rect(position.x + 12f, position.y + 52f, size.x - 24f, 24f), $"Banked Gold: {bank.Gold}  Relics: {bank.Relics}");
        GUI.Label(new Rect(position.x + 12f, position.y + 76f, size.x - 24f, 24f), "Deposit at the physical bank before entering dungeon.");
        GUI.Label(new Rect(position.x + 12f, position.y + 100f, size.x - 24f, 24f), "Carried currency can fund dungeon merchants later.");
    }
}
