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

        GUI.Label(new Rect(20f, Screen.height - 140f, 360f, 40f), promptText);
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
