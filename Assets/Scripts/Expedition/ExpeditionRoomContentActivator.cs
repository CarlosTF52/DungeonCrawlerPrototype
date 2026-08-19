using UnityEngine;

public class ExpeditionRoomContentActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyObjects;
    [SerializeField] private GameObject[] pickupObjects;
    [SerializeField] private GameObject[] trapObjects;
    [SerializeField] private GameObject[] objectiveObjects;
    [SerializeField] private GameObject[] extractionObjects;

    private void Start()
    {
        ApplyCurrentRoom();
    }

    private void OnEnable()
    {
        ExpeditionRunManager.Instance.StateChanged += ApplyCurrentRoom;
    }

    private void OnDisable()
    {
        ExpeditionRunManager.Instance.StateChanged -= ApplyCurrentRoom;
    }

    public void ApplyCurrentRoom()
    {
        ExpeditionRoomNode room = ExpeditionRunManager.Instance.CurrentRoom;

        SetActiveCount(enemyObjects, room != null ? room.EnemyCount : 0);
        SetActiveCount(pickupObjects, room != null ? room.PickupCount : 0);
        SetActiveCount(trapObjects, room != null ? room.TrapCount : 0);
        SetActiveAll(objectiveObjects, room != null && room.HasObjective);
        SetActiveAll(extractionObjects, room != null && room.HasExtraction);
    }

    private static void SetActiveCount(GameObject[] objects, int activeCount)
    {
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(i < activeCount);
            }
        }
    }

    private static void SetActiveAll(GameObject[] objects, bool isActive)
    {
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(isActive);
            }
        }
    }
}
