using UnityEngine;

public class SceneExitTrigger : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnId = "Default";

    private bool isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !other.CompareTag("Player"))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("Scene exit trigger has no target scene assigned.", this);
            return;
        }

        isLoading = true;
        SceneTransitionManager.LoadScene(targetSceneName, targetSpawnId);
    }
}
