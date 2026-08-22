using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class SlimeLaunchEnemy : MonoBehaviour
{
    [SerializeField] private EnemyLaunchAttackProfile attackProfile;
    [SerializeField] private Transform target;
    [SerializeField] private Collider launchDamageCollider;
    [SerializeField] private SimpleEnemyChase chase;
    [SerializeField] private Damageable damageable;
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private bool cancelAttackWhenDamaged = true;
    [SerializeField] private float interruptedCooldown = 0.75f;
    [SerializeField] private bool endLaunchAfterDamagingTarget = true;
    [SerializeField] private float postHitRecoveryDelay;
    [SerializeField] private bool ignoreTargetBodyCollisionDuringLaunch = true;
    [SerializeField] private bool restoreBodyCollisionOnlyAfterSeparation = true;
    [SerializeField] private float collisionRestoreCheckInterval = 0.05f;
    [SerializeField] private bool disableLaunchContactKnockback = true;
    [SerializeField] private Collider[] bodyColliders;

    [Header("Events")]
    [SerializeField] private UnityEvent windupStarted;
    [SerializeField] private UnityEvent launchStarted;
    [SerializeField] private UnityEvent recoveryStarted;
    [SerializeField] private UnityEvent attackFinished;

    private Rigidbody attachedRigidbody;
    private Coroutine attackRoutine;
    private float nextAttackTime;
    private ContactDamage launchContactDamage;
    private bool launchHitTarget;
    private Collider[] ignoredTargetColliders;
    private bool isIgnoringTargetCollision;
    private Coroutine restoreCollisionRoutine;

    private bool IsAttacking => attackRoutine != null;

    public event Action WindupStarted;
    public event Action LaunchStarted;
    public event Action RecoveryStarted;
    public event Action AttackFinished;
    public event Action AttackInterrupted;

    private void Reset()
    {
        chase = GetComponent<SimpleEnemyChase>();
        damageable = GetComponent<Damageable>();

        Rigidbody body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Awake()
    {
        attachedRigidbody = GetComponent<Rigidbody>();

        if (chase == null)
        {
            chase = GetComponent<SimpleEnemyChase>();
        }

        if (damageable == null)
        {
            damageable = GetComponent<Damageable>();
        }

        if (bodyColliders == null || bodyColliders.Length == 0)
        {
            bodyColliders = GetComponentsInChildren<Collider>();
        }

        if (launchDamageCollider == null)
        {
            Debug.LogWarning($"{name}: SlimeLaunchEnemy needs a launch damage trigger assigned.", this);
        }
        else
        {
            launchContactDamage = launchDamageCollider.GetComponent<ContactDamage>();

            if (launchContactDamage != null && disableLaunchContactKnockback)
            {
                launchContactDamage.SetPlayTargetKnockback(false);
            }
        }

        SetLaunchDamageActive(false);
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.Damaged += HandleDamaged;
        }

        if (launchContactDamage != null)
        {
            launchContactDamage.DamageDealt += HandleLaunchDamageDealt;
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.Damaged -= HandleDamaged;
        }

        if (launchContactDamage != null)
        {
            launchContactDamage.DamageDealt -= HandleLaunchDamageDealt;
        }

        CancelAttack();
    }

    private void OnValidate()
    {
        interruptedCooldown = Mathf.Max(0f, interruptedCooldown);
        postHitRecoveryDelay = Mathf.Max(0f, postHitRecoveryDelay);
        collisionRestoreCheckInterval = Mathf.Max(0.01f, collisionRestoreCheckInterval);
    }

    private void Update()
    {
        if (attackProfile == null || IsAttacking)
        {
            if (damageable != null && !damageable.IsAlive)
            {
                CancelAttack();
            }

            return;
        }

        if (damageable != null && !damageable.IsAlive)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (target == null)
        {
            FindTarget();
        }

        if (target == null || !CanStartAttack())
        {
            return;
        }

        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        float totalLockedTime = attackProfile.WindupDuration + attackProfile.LaunchDuration + attackProfile.RecoveryDuration;
        StaggerChase(totalLockedTime);

        WindupStarted?.Invoke();
        windupStarted?.Invoke();
        yield return FaceTargetForDuration(attackProfile.WindupDuration);

        Vector3 launchDirection = GetDirectionToTarget();
        if (launchDirection.sqrMagnitude < 0.001f)
        {
            launchDirection = transform.forward;
        }

        launchDirection.Normalize();

        LaunchStarted?.Invoke();
        launchStarted?.Invoke();
        launchHitTarget = false;
        BeginIgnoringTargetBodyCollision();
        SetLaunchDamageActive(true);

        if (attackProfile.ClearVelocityBeforeLaunch)
        {
            attachedRigidbody.linearVelocity = Vector3.zero;
            attachedRigidbody.angularVelocity = Vector3.zero;
        }

        Vector3 launchVelocity = launchDirection * attackProfile.HorizontalLaunchSpeed;
        launchVelocity.y = attackProfile.UpwardLaunchSpeed;
        attachedRigidbody.AddForce(launchVelocity, ForceMode.VelocityChange);

        float elapsed = 0f;

        while (elapsed < attackProfile.LaunchDuration)
        {
            elapsed += Time.deltaTime;

            if (attackProfile.EndLaunchWhenGrounded
                && elapsed >= attackProfile.MinimumAirTime
                && IsGrounded())
            {
                break;
            }

            if (launchHitTarget)
            {
                break;
            }

            yield return null;
        }

        SetLaunchDamageActive(false);

        if (attackProfile.StopVelocityOnRecovery || launchHitTarget)
        {
            attachedRigidbody.linearVelocity = Vector3.zero;
            attachedRigidbody.angularVelocity = Vector3.zero;
        }

        if (launchHitTarget && postHitRecoveryDelay > 0f)
        {
            yield return new WaitForSeconds(postHitRecoveryDelay);
        }

        RecoveryStarted?.Invoke();
        recoveryStarted?.Invoke();
        yield return new WaitForSeconds(attackProfile.RecoveryDuration);

        StartRestoringTargetBodyCollision();

        nextAttackTime = Time.time + attackProfile.CooldownDuration;
        AttackFinished?.Invoke();
        attackFinished?.Invoke();
        attackRoutine = null;
    }

    private IEnumerator FaceTargetForDuration(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            FaceTarget(Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void FaceTarget(float deltaTime)
    {
        Vector3 direction = GetDirectionToTarget();

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, attackProfile.TurnSpeed * deltaTime);
        attachedRigidbody.MoveRotation(rotation);
    }

    private void FindTarget()
    {
        GameObject targetObject = GameObject.FindGameObjectWithTag(attackProfile.TargetTag);

        if (targetObject != null)
        {
            target = targetObject.transform;
        }
    }

    private bool CanStartAttack()
    {
        Vector3 toTarget = GetDirectionToTarget();
        float distance = toTarget.magnitude;

        if (distance < attackProfile.MinAttackRange || distance > attackProfile.AttackRange)
        {
            return false;
        }

        return !attackProfile.RequireLineOfSight || HasLineOfSight();
    }

    private Vector3 GetDirectionToTarget()
    {
        if (target == null)
        {
            return Vector3.zero;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        return direction;
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 destination = target.position + Vector3.up * 0.5f;
        Vector3 direction = destination - origin;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, attackProfile.LineOfSightBlockers, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return hit.transform == target || hit.transform.IsChildOf(target);
    }

    private bool IsGrounded()
    {
        Vector3 origin = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position + Vector3.up * 0.05f;
        return Physics.Raycast(origin, Vector3.down, attackProfile.GroundCheckDistance, attackProfile.GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void SetLaunchDamageActive(bool isActive)
    {
        if (launchDamageCollider != null)
        {
            launchDamageCollider.enabled = isActive;
        }
    }

    private void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        SetLaunchDamageActive(false);
        launchHitTarget = false;
        EndIgnoringTargetBodyCollision();

        if (attachedRigidbody != null)
        {
            attachedRigidbody.linearVelocity = Vector3.zero;
            attachedRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void HandleDamaged()
    {
        if (!cancelAttackWhenDamaged || !IsAttacking)
        {
            return;
        }

        CancelAttack();
        nextAttackTime = Time.time + interruptedCooldown;
        AttackInterrupted?.Invoke();
    }

    private void HandleLaunchDamageDealt(Damageable target)
    {
        if (!endLaunchAfterDamagingTarget || !IsAttacking)
        {
            return;
        }

        launchHitTarget = true;
        SetLaunchDamageActive(false);
    }

    private void BeginIgnoringTargetBodyCollision()
    {
        if (!ignoreTargetBodyCollisionDuringLaunch || target == null || bodyColliders == null)
        {
            return;
        }

        if (restoreCollisionRoutine != null)
        {
            StopCoroutine(restoreCollisionRoutine);
            restoreCollisionRoutine = null;
        }

        ignoredTargetColliders = target.GetComponentsInChildren<Collider>();

        for (int i = 0; i < bodyColliders.Length; i++)
        {
            Collider bodyCollider = bodyColliders[i];

            if (!IsBodyCollisionCollider(bodyCollider))
            {
                continue;
            }

            for (int j = 0; j < ignoredTargetColliders.Length; j++)
            {
                Collider targetCollider = ignoredTargetColliders[j];

                if (targetCollider != null && !targetCollider.isTrigger)
                {
                    Physics.IgnoreCollision(bodyCollider, targetCollider, true);
                }
            }
        }

        isIgnoringTargetCollision = true;
    }

    private void StartRestoringTargetBodyCollision()
    {
        if (!isIgnoringTargetCollision)
        {
            return;
        }

        if (!restoreBodyCollisionOnlyAfterSeparation)
        {
            EndIgnoringTargetBodyCollision();
            return;
        }

        if (restoreCollisionRoutine != null)
        {
            StopCoroutine(restoreCollisionRoutine);
        }

        restoreCollisionRoutine = StartCoroutine(RestoreCollisionAfterSeparationRoutine());
    }

    private IEnumerator RestoreCollisionAfterSeparationRoutine()
    {
        while (IsOverlappingIgnoredTargetCollider())
        {
            yield return new WaitForSeconds(collisionRestoreCheckInterval);
        }

        EndIgnoringTargetBodyCollision();
        restoreCollisionRoutine = null;
    }

    private void EndIgnoringTargetBodyCollision()
    {
        if (restoreCollisionRoutine != null)
        {
            StopCoroutine(restoreCollisionRoutine);
            restoreCollisionRoutine = null;
        }

        if (!isIgnoringTargetCollision || bodyColliders == null || ignoredTargetColliders == null)
        {
            isIgnoringTargetCollision = false;
            ignoredTargetColliders = null;
            return;
        }

        for (int i = 0; i < bodyColliders.Length; i++)
        {
            Collider bodyCollider = bodyColliders[i];

            if (!IsBodyCollisionCollider(bodyCollider))
            {
                continue;
            }

            for (int j = 0; j < ignoredTargetColliders.Length; j++)
            {
                Collider targetCollider = ignoredTargetColliders[j];

                if (targetCollider != null && !targetCollider.isTrigger)
                {
                    Physics.IgnoreCollision(bodyCollider, targetCollider, false);
                }
            }
        }

        isIgnoringTargetCollision = false;
        ignoredTargetColliders = null;
    }

    private bool IsBodyCollisionCollider(Collider bodyCollider)
    {
        return bodyCollider != null
            && !bodyCollider.isTrigger
            && bodyCollider != launchDamageCollider;
    }

    private bool IsOverlappingIgnoredTargetCollider()
    {
        if (bodyColliders == null || ignoredTargetColliders == null)
        {
            return false;
        }

        for (int i = 0; i < bodyColliders.Length; i++)
        {
            Collider bodyCollider = bodyColliders[i];

            if (!IsBodyCollisionCollider(bodyCollider))
            {
                continue;
            }

            for (int j = 0; j < ignoredTargetColliders.Length; j++)
            {
                Collider targetCollider = ignoredTargetColliders[j];

                if (targetCollider == null || targetCollider.isTrigger)
                {
                    continue;
                }

                if (Physics.ComputePenetration(
                    bodyCollider,
                    bodyCollider.transform.position,
                    bodyCollider.transform.rotation,
                    targetCollider,
                    targetCollider.transform.position,
                    targetCollider.transform.rotation,
                    out _,
                    out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void StaggerChase(float duration)
    {
        if (chase != null)
        {
            chase.Stagger(duration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackProfile == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackProfile.AttackRange);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, attackProfile.MinAttackRange);
    }
}
