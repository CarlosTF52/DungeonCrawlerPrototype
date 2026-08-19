using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DoorSceneTransition : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnId = "Default";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to enter";
    [SerializeField] private bool advanceExpeditionRoomIfActive = true;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key interactionKey = Key.E;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode legacyInteractionKey = KeyCode.E;
#endif

    private bool isPlayerInRange;
    private bool isLoading;

    private void Reset()
    {
        Collider doorCollider = GetComponent<Collider>();

        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (!isPlayerInRange || isLoading || !WasInteractionPressed())
        {
            return;
        }

        TryEnterDoor();
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
        if (!isPlayerInRange || isLoading || string.IsNullOrWhiteSpace(promptText))
        {
            return;
        }

        GUI.Label(new Rect(20f, Screen.height - 60f, 240f, 40f), promptText);
    }

    private void TryEnterDoor()
    {
        ExpeditionRunManager expeditionRunManager = ExpeditionRunManager.Instance;

        if (advanceExpeditionRoomIfActive && expeditionRunManager.IsInExpedition)
        {
            isLoading = true;
            expeditionRunManager.DescendToNextDepth();
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("Door has no target scene assigned.", this);
            return;
        }

        isLoading = true;
        SceneTransitionManager.LoadScene(targetSceneName, targetSpawnId);
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
