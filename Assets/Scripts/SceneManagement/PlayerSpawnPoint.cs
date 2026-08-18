using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId = "Default";

    public string SpawnId => spawnId;
}
