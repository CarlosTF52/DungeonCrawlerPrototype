using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ExpeditionGateway : MonoBehaviour
{
    public enum GatewayAction
    {
        BeginExpedition,
        Descend,
        Extract,
        Fail,
        Abandon
    }

    [SerializeField] private GatewayAction action = GatewayAction.BeginExpedition;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E";
    [SerializeField] private bool showBlockedExtractionPrompt = true;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key interactionKey = Key.E;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyInteractionKey = KeyCode.E;
#endif

    private bool isPlayerInRange;
    private bool hasActivated;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (!isPlayerInRange || hasActivated || !WasInteractionPressed())
        {
            return;
        }

        Activate();
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
        if (!isPlayerInRange || hasActivated)
        {
            return;
        }

        string text = GetPromptText();

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        GUI.Label(new Rect(20f, Screen.height - 100f, 320f, 40f), text);
    }

    private void Activate()
    {
        ExpeditionRunManager manager = ExpeditionRunManager.Instance;

        if (action == GatewayAction.Extract && !manager.CanExtract)
        {
            Debug.LogWarning($"Cannot extract: {manager.ExtractionStatus}.", this);
            return;
        }

        hasActivated = true;

        switch (action)
        {
            case GatewayAction.BeginExpedition:
                manager.BeginExpedition();
                break;
            case GatewayAction.Descend:
                manager.DescendToNextDepth();
                break;
            case GatewayAction.Extract:
                manager.Extract();
                break;
            case GatewayAction.Fail:
                manager.FailExpedition();
                break;
            case GatewayAction.Abandon:
                manager.AbandonExpedition();
                break;
        }
    }

    private string GetPromptText()
    {
        ExpeditionRunManager manager = ExpeditionRunManager.Instance;

        if (action == GatewayAction.Extract && !manager.CanExtract && showBlockedExtractionPrompt)
        {
            return manager.ExtractionStatus;
        }

        return promptText;
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
