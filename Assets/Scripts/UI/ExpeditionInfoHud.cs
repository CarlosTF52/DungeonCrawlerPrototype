using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExpeditionInfoHud : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text roomText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text lootText;
    [SerializeField] private TMP_Text extractionText;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private bool showOnlyDuringExpedition = true;
    [SerializeField] private string idleTitle = "Expedition";

    [Header("Input")]
    [SerializeField] private bool requireTabHeld = true;
    [SerializeField] private Key tabKey = Key.Tab;
    [SerializeField] private KeyCode legacyTabKey = KeyCode.Tab;

    private ExpeditionRunManager expeditionRunManager;
    private PlayerCurrencyPouch playerCurrencyPouch;
    private int displayedRunNumber = int.MinValue;
    private int displayedDepth = int.MinValue;
    private int displayedObjectiveProgress = int.MinValue;
    private int displayedGold = int.MinValue;
    private int displayedRelics = int.MinValue;
    private int displayedRoomId = int.MinValue;
    private ExpeditionOutcome displayedOutcome;
    private bool displayedInExpedition;
    private bool displayedTabHeld;

    private void OnEnable()
    {
        RefreshReferences(true);
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        RefreshReferences(false);

        if (NeedsRefresh())
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (expeditionRunManager == null)
        {
            SetContentVisible(false);
            return;
        }

        displayedInExpedition = expeditionRunManager.IsInExpedition;
        displayedTabHeld = IsTabHeld();

        if ((showOnlyDuringExpedition && !displayedInExpedition) || !ShouldShowForInput())
        {
            HideHud();
            return;
        }

        SetContentVisible(true);

        ExpeditionRoomNode room = expeditionRunManager.CurrentRoom;
        displayedRunNumber = expeditionRunManager.RunNumber;
        displayedDepth = expeditionRunManager.Depth;
        displayedObjectiveProgress = expeditionRunManager.ObjectiveProgress;
        displayedGold = playerCurrencyPouch != null ? playerCurrencyPouch.Gold : expeditionRunManager.GoldCollected;
        displayedRelics = playerCurrencyPouch != null ? playerCurrencyPouch.Relics : expeditionRunManager.RelicsCollected;
        displayedRoomId = room != null ? room.Id : int.MinValue;
        displayedOutcome = expeditionRunManager.LastOutcome;

        string title = displayedInExpedition ? $"Run {displayedRunNumber}   Depth {displayedDepth}" : $"{idleTitle}   Last: {displayedOutcome}";
        string roomSummary = room != null ? $"{room.RoomType}   Danger {room.DangerRating}" : "No active room";
        string objectiveSummary = $"{displayedObjectiveProgress}/{expeditionRunManager.ObjectivesRequiredToExtract}";

        SetText(titleText, title);
        SetText(roomText, roomSummary);
        SetText(objectiveText, $"Objective: {objectiveSummary}");
        SetText(lootText, $"Carried: {displayedGold} Gold   {displayedRelics} Relics");
        SetText(extractionText, $"Extraction: {expeditionRunManager.ExtractionStatus}");
    }

    private bool NeedsRefresh()
    {
        if (expeditionRunManager == null)
        {
            return false;
        }

        ExpeditionRoomNode room = expeditionRunManager.CurrentRoom;
        int roomId = room != null ? room.Id : int.MinValue;
        int gold = playerCurrencyPouch != null ? playerCurrencyPouch.Gold : expeditionRunManager.GoldCollected;
        int relics = playerCurrencyPouch != null ? playerCurrencyPouch.Relics : expeditionRunManager.RelicsCollected;

        return expeditionRunManager.IsInExpedition != displayedInExpedition
            || expeditionRunManager.RunNumber != displayedRunNumber
            || expeditionRunManager.Depth != displayedDepth
            || expeditionRunManager.ObjectiveProgress != displayedObjectiveProgress
            || expeditionRunManager.LastOutcome != displayedOutcome
            || IsTabHeld() != displayedTabHeld
            || roomId != displayedRoomId
            || gold != displayedGold
            || relics != displayedRelics;
    }

    private void RefreshReferences(bool force)
    {
        ExpeditionRunManager currentRunManager = ExpeditionRunManager.Instance;
        PlayerCurrencyPouch currentPouch = PlayerCurrencyPouch.Instance;

        if (!force && expeditionRunManager == currentRunManager && playerCurrencyPouch == currentPouch)
        {
            return;
        }

        Unsubscribe();
        expeditionRunManager = currentRunManager;
        playerCurrencyPouch = currentPouch;
        Subscribe();
    }

    private void Subscribe()
    {
        if (expeditionRunManager != null)
        {
            expeditionRunManager.StateChanged -= Refresh;
            expeditionRunManager.StateChanged += Refresh;
        }

        if (playerCurrencyPouch != null)
        {
            playerCurrencyPouch.BalanceChanged -= Refresh;
            playerCurrencyPouch.BalanceChanged += Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (expeditionRunManager != null)
        {
            expeditionRunManager.StateChanged -= Refresh;
        }

        if (playerCurrencyPouch != null)
        {
            playerCurrencyPouch.BalanceChanged -= Refresh;
        }
    }

    private void SetContentVisible(bool visible)
    {
        if (contentRoot != null)
        {
            contentRoot.SetActive(visible);
        }
    }

    private void HideHud()
    {
        SetContentVisible(false);
        SetText(titleText, string.Empty);
        SetText(roomText, string.Empty);
        SetText(objectiveText, string.Empty);
        SetText(lootText, string.Empty);
        SetText(extractionText, string.Empty);
    }

    private bool ShouldShowForInput()
    {
        return !requireTabHeld || displayedTabHeld;
    }

    private bool IsTabHeld()
    {
        if (!requireTabHeld)
        {
            return true;
        }

        if (Keyboard.current != null && Keyboard.current[tabKey].isPressed)
        {
            return true;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(legacyTabKey);
#else
        return false;
#endif
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
