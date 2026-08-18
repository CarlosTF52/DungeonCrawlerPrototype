using UnityEngine;

public class DeathDisableObject : MonoBehaviour
{
    [SerializeField] private GameObject target;

    private void Awake()
    {
        if (target == null)
        {
            target = gameObject;
        }
    }

    public void DisableTarget()
    {
        if (target != null)
        {
            target.SetActive(false);
        }
    }
}
