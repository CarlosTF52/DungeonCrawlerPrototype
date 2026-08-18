using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;
    private static string targetSpawnId = "Default";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public static void LoadScene(string sceneName, string spawnId)
    {
        EnsureInstanceExists();
        targetSpawnId = string.IsNullOrWhiteSpace(spawnId) ? "Default" : spawnId;
        SceneManager.LoadScene(sceneName);
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("SceneTransitionManager");
        managerObject.AddComponent<SceneTransitionManager>();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PersistentPlayer player = FindObjectOfType<PersistentPlayer>();

        if (player == null)
        {
            Debug.LogWarning("No PersistentPlayer found after scene load.");
            return;
        }

        PlayerSpawnPoint spawnPoint = FindSpawnPoint(targetSpawnId);

        if (spawnPoint == null)
        {
            Debug.LogWarning($"No PlayerSpawnPoint found with spawn id '{targetSpawnId}'.");
            return;
        }

        MovePlayerToSpawn(player.gameObject, spawnPoint.transform);
    }

    private static PlayerSpawnPoint FindSpawnPoint(string spawnId)
    {
        PlayerSpawnPoint[] spawnPoints = FindObjectsOfType<PlayerSpawnPoint>();

        foreach (PlayerSpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.SpawnId == spawnId)
            {
                return spawnPoint;
            }
        }

        return null;
    }

    private static void MovePlayerToSpawn(GameObject player, Transform spawnTransform)
    {
        CharacterController characterController = player.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        player.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}
