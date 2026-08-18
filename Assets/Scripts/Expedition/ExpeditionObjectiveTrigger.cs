using UnityEngine;

public class ExpeditionObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int progressValue = 1;
    [SerializeField] private bool destroyOnComplete = true;
    [SerializeField] private bool logObjectiveEvents = true;

    private bool hasCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (hasCompleted)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            if (logObjectiveEvents)
            {
                Debug.Log($"Objective ignored collider tagged '{other.tag}'. Expected '{playerTag}'.", this);
            }

            return;
        }

        if (!ExpeditionRunManager.Instance.AddObjectiveProgress(progressValue))
        {
            return;
        }

        hasCompleted = true;

        if (logObjectiveEvents)
        {
            Debug.Log($"Objective progress added: {progressValue}.", this);
        }

        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
    }
}
