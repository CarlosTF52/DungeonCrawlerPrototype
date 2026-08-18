using UnityEngine;

public class ExpeditionDebugHud : MonoBehaviour
{
    [SerializeField] private bool showOnlyDuringExpedition = false;
    [SerializeField] private Vector2 position = new Vector2(20f, 20f);
    [SerializeField] private Vector2 size = new Vector2(280f, 154f);

    private void OnGUI()
    {
        ExpeditionRunManager manager = ExpeditionRunManager.Instance;

        if (showOnlyDuringExpedition && !manager.IsInExpedition)
        {
            return;
        }

        string status = manager.IsInExpedition ? $"Depth {manager.Depth}" : $"Last: {manager.LastOutcome}";
        string extractStatus = manager.CanExtract ? "Ready" : "Objective incomplete";

        GUI.Box(new Rect(position.x, position.y, size.x, size.y), "Expedition");
        GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), status);
        GUI.Label(new Rect(position.x + 12f, position.y + 52f, size.x - 24f, 24f), $"Objective: {manager.ObjectiveProgress}/{manager.ObjectivesRequiredToExtract}");
        GUI.Label(new Rect(position.x + 12f, position.y + 76f, size.x - 24f, 24f), $"Gold: {manager.GoldCollected}  Relics: {manager.RelicsCollected}");
        GUI.Label(new Rect(position.x + 12f, position.y + 100f, size.x - 24f, 24f), $"Extract: {extractStatus}");
        GUI.Label(new Rect(position.x + 12f, position.y + 124f, size.x - 24f, 24f), $"Last Banked: {manager.LastBankedGold}g / {manager.LastBankedRelics}r");
    }
}
