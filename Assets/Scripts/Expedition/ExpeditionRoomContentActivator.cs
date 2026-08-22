using UnityEngine;

public class ExpeditionRoomContentActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyObjects;
    [SerializeField] private GameObject[] pickupObjects;
    [SerializeField] private GameObject[] trapObjects;
    [SerializeField] private GameObject[] objectiveObjects;
    [SerializeField] private GameObject[] extractionObjects;
    [SerializeField] private bool randomizePickupObjects = true;

    private int appliedRunNumber = -1;
    private int appliedRoomId = -1;

    private void Start()
    {
        ApplyCurrentRoom(true);
    }

    private void OnEnable()
    {
        ExpeditionRunManager.Instance.StateChanged += ApplyCurrentRoomIfChanged;
    }

    private void OnDisable()
    {
        ExpeditionRunManager.Instance.StateChanged -= ApplyCurrentRoomIfChanged;
    }

    public void ApplyCurrentRoom()
    {
        ApplyCurrentRoom(true);
    }

    private void ApplyCurrentRoomIfChanged()
    {
        ApplyCurrentRoom(false);
    }

    private void ApplyCurrentRoom(bool force)
    {
        ExpeditionRoomNode room = ExpeditionRunManager.Instance.CurrentRoom;
        int roomId = room != null ? room.Id : -1;
        int runNumber = ExpeditionRunManager.Instance.RunNumber;

        if (!force && appliedRunNumber == runNumber && appliedRoomId == roomId)
        {
            return;
        }

        appliedRunNumber = runNumber;
        appliedRoomId = roomId;

        SetActiveCount(enemyObjects, room != null ? room.EnemyCount : 0);
        SetActivePickups(room != null ? room.PickupCount : 0);
        SetActiveCount(trapObjects, room != null ? room.TrapCount : 0);
        SetActiveAll(objectiveObjects, room != null && room.HasObjective);
        SetActiveAll(extractionObjects, room != null && room.HasExtraction);
    }

    private void SetActivePickups(int activeCount)
    {
        if (!randomizePickupObjects)
        {
            SetActiveCount(pickupObjects, activeCount);
            return;
        }

        SetActiveRandomCount(pickupObjects, activeCount);
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

    private static void SetActiveRandomCount(GameObject[] objects, int activeCount)
    {
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(false);
            }
        }

        int remainingToEnable = Mathf.Clamp(activeCount, 0, objects.Length);

        for (int i = 0; i < objects.Length && remainingToEnable > 0; i++)
        {
            int remainingSlots = objects.Length - i;

            if (UnityEngine.Random.Range(0, remainingSlots) < remainingToEnable)
            {
                if (objects[i] != null)
                {
                    objects[i].SetActive(true);
                }

                remainingToEnable--;
            }
        }
    }
}
