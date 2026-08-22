using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class VillageBankInteractable : MonoBehaviour
{
    public enum BankAction
    {
        DepositAllCarried,
        WithdrawConfiguredAmount,
        WithdrawAllStored
    }

    [SerializeField] private BankAction action = BankAction.DepositAllCarried;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to use bank";
    [SerializeField] private int withdrawGold;
    [SerializeField] private int withdrawRelics;
    [SerializeField] private bool showCurrencySummary = true;
    [SerializeField] private Vector2 hudSize = new Vector2(420f, 116f);

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key interactionKey = Key.E;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyInteractionKey = KeyCode.E;
#endif

    private bool isPlayerInRange;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        withdrawGold = Mathf.Max(0, withdrawGold);
        withdrawRelics = Mathf.Max(0, withdrawRelics);
        hudSize.x = Mathf.Max(300f, hudSize.x);
        hudSize.y = Mathf.Max(72f, hudSize.y);
    }

    private void Update()
    {
        if (isPlayerInRange && WasInteractionPressed())
        {
            UseBank();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }

    private void OnGUI()
    {
        if (!isPlayerInRange || string.IsNullOrWhiteSpace(promptText))
        {
            return;
        }

        if (!showCurrencySummary)
        {
            GUI.Label(new Rect(20f, Screen.height - 140f, 360f, 40f), promptText);
            return;
        }

        Rect panelRect = new Rect(20f, Screen.height - hudSize.y - 92f, hudSize.x, hudSize.y);
        VillageBank bank = VillageBank.Instance;
        PlayerCurrencyPouch pouch = PlayerCurrencyPouch.Instance;

        GUI.Box(panelRect, "Village Bank");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 26f, panelRect.width - 24f, 22f), promptText);
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 50f, panelRect.width - 24f, 22f), $"Carried: {pouch.Gold} gold, {pouch.Relics} relics");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 74f, panelRect.width - 24f, 22f), $"Banked: {bank.Gold} gold, {bank.Relics} relics");
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 98f, panelRect.width - 24f, 22f), BuildActionSummary(bank, pouch));
    }

    public void UseBank()
    {
        VillageBank bank = VillageBank.Instance;
        bool succeeded = false;

        switch (action)
        {
            case BankAction.DepositAllCarried:
                succeeded = bank.TryDepositAllFromPouch();
                break;
            case BankAction.WithdrawConfiguredAmount:
                succeeded = bank.TryWithdrawToPouch(withdrawGold, withdrawRelics);
                break;
            case BankAction.WithdrawAllStored:
                succeeded = bank.TryWithdrawAllToPouch();
                break;
        }

        if (succeeded)
        {
            Debug.Log($"Bank action complete: {action}.", this);
            return;
        }

        Debug.LogWarning($"Bank action failed: {action}.", this);
    }

    private string BuildActionSummary(VillageBank bank, PlayerCurrencyPouch pouch)
    {
        switch (action)
        {
            case BankAction.DepositAllCarried:
                return pouch.HasAnyCurrency ? $"Deposit: {pouch.Gold} gold, {pouch.Relics} relics" : "Nothing carried to deposit";
            case BankAction.WithdrawConfiguredAmount:
                return bank.CanAfford(withdrawGold, withdrawRelics) ? $"Withdraw: {withdrawGold} gold, {withdrawRelics} relics" : $"Cannot withdraw: need {withdrawGold} gold, {withdrawRelics} relics banked";
            case BankAction.WithdrawAllStored:
                return bank.HasAnyCurrency ? $"Withdraw all: {bank.Gold} gold, {bank.Relics} relics" : "Nothing banked to withdraw";
            default:
                return string.Empty;
        }
    }

    private bool WasInteractionPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current[interactionKey].wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(legacyInteractionKey))
        {
            return true;
        }
#endif

        return false;
    }
}
