using UnityEngine;

public class SimpleEnemyChase : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private Transform target;
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float stopDistance = 1.25f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private bool requireLineOfSight;
    [SerializeField] private LayerMask lineOfSightBlockers = ~0;

    private CharacterController characterController;
    private Rigidbody attachedRigidbody;
    private Damageable damageable;
    private Vector3 desiredDirection;
    private bool shouldMove;
    private float staggeredUntilTime;

    private void Reset()
    {
        attachedRigidbody = GetComponent<Rigidbody>();

        if (attachedRigidbody != null)
        {
            attachedRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            attachedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        attachedRigidbody = GetComponent<Rigidbody>();
        damageable = GetComponent<Damageable>();
    }

    private void OnValidate()
    {
        aggroRange = Mathf.Max(0f, aggroRange);
        stopDistance = Mathf.Max(0f, stopDistance);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        turnSpeed = Mathf.Max(0f, turnSpeed);
    }

    private void FixedUpdate()
    {
        desiredDirection = Vector3.zero;
        shouldMove = false;

        if (damageable != null && !damageable.IsAlive)
        {
            return;
        }

        if (Time.time < staggeredUntilTime)
        {
            return;
        }

        if (target == null)
        {
            FindTarget();
        }

        if (target == null || !ShouldChaseTarget())
        {
            return;
        }

        UpdateChaseIntent();
        ApplyMovement();
    }

    public void Stagger(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        staggeredUntilTime = Mathf.Max(staggeredUntilTime, Time.time + duration);
    }

    private void FindTarget()
    {
        GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);

        if (targetObject != null)
        {
            target = targetObject.transform;
        }
    }

    private bool ShouldChaseTarget()
    {
        Vector3 toTarget = GetFlatDirectionToTarget();

        if (toTarget.sqrMagnitude > aggroRange * aggroRange)
        {
            return false;
        }

        return !requireLineOfSight || HasLineOfSight();
    }

    private void UpdateChaseIntent()
    {
        Vector3 toTarget = GetFlatDirectionToTarget();
        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
        {
            return;
        }

        Vector3 direction = toTarget / distance;
        desiredDirection = direction;

        if (distance <= stopDistance)
        {
            return;
        }

        shouldMove = true;
    }

    private Vector3 GetFlatDirectionToTarget()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        return toTarget;
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 0.6f;
        Vector3 destination = target.position + Vector3.up * 0.6f;
        Vector3 direction = destination - origin;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, lineOfSightBlockers, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return hit.transform == target || hit.transform.IsChildOf(target);
    }

    private void ApplyMovement()
    {
        if (desiredDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        RotateToward(desiredDirection);

        if (shouldMove)
        {
            Move(desiredDirection);
        }
    }

    private void RotateToward(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        if (attachedRigidbody != null)
        {
            Quaternion rotation = Quaternion.RotateTowards(attachedRigidbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            attachedRigidbody.MoveRotation(rotation);
            return;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
    }

    private void Move(Vector3 direction)
    {
        Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;

        if (attachedRigidbody != null)
        {
            attachedRigidbody.MovePosition(attachedRigidbody.position + movement);
            return;
        }

        if (characterController != null)
        {
            characterController.Move(movement);
            return;
        }

        transform.position += movement;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
