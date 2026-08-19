using UnityEngine;

public class ExpeditionDebugHud : MonoBehaviour
{
    [SerializeField] private bool showOnlyDuringExpedition = false;
    [SerializeField] private Vector2 position = new Vector2(20f, 20f);
    [SerializeField] private Vector2 size = new Vector2(420f, 250f);

    private void OnGUI()
    {
        ExpeditionRunManager manager = ExpeditionRunManager.Instance;

        if (showOnlyDuringExpedition && !manager.IsInExpedition)
        {
            return;
        }

        ExpeditionRoomNode room = manager.CurrentRoom;
        string status = manager.IsInExpedition ? $"Depth {manager.Depth}" : $"Last: {manager.LastOutcome}";
        string roomStatus = room != null ? $"{room.RoomType}  Danger {room.DangerRating}" : "No room";

        GUI.Box(new Rect(position.x, position.y, size.x, size.y), "Expedition");
        GUI.Label(new Rect(position.x + 12f, position.y + 28f, size.x - 24f, 24f), status);
        GUI.Label(new Rect(position.x + 12f, position.y + 52f, size.x - 24f, 24f), $"Room: {roomStatus}");
        GUI.Label(new Rect(position.x + 12f, position.y + 76f, size.x - 24f, 24f), $"Content: {(room != null ? room.ContentsLabel : "None")}");
        GUI.Label(new Rect(position.x + 12f, position.y + 100f, size.x - 24f, 24f), $"Special: {GetSpecialContentLabel(room)}");
        GUI.Label(new Rect(position.x + 12f, position.y + 124f, size.x - 24f, 24f), $"Path Step: {manager.CurrentObjectivePathIndex + 1}");
        GUI.Label(new Rect(position.x + 12f, position.y + 148f, size.x - 24f, 24f), $"Objective: {manager.ObjectiveProgress}/{manager.ObjectivesRequiredToExtract}");
        GUI.Label(new Rect(position.x + 12f, position.y + 172f, size.x - 24f, 24f), $"Route: {manager.ObjectivePathLabel}");
        GUI.Label(new Rect(position.x + 12f, position.y + 196f, size.x - 24f, 24f), $"Gold: {manager.GoldCollected}  Relics: {manager.RelicsCollected}");
        GUI.Label(new Rect(position.x + 12f, position.y + 220f, size.x - 24f, 24f), $"Extract: {manager.ExtractionStatus}");
    }

    private static string GetSpecialContentLabel(ExpeditionRoomNode room)
    {
        if (room == null)
        {
            return "None";
        }

        if (room.HasObjective && room.HasExtraction)
        {
            return "Objective Extraction";
        }

        if (room.HasObjective)
        {
            return "Objective";
        }

        return room.HasExtraction ? "Extraction" : "None";
    }
}
