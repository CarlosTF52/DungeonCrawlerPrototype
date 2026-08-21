using System.Collections;
using UnityEngine;

public class SlimeVisualDeformer : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SlimeLaunchEnemy slimeLaunchEnemy;
    [SerializeField] private Damageable damageable;

    [Header("Idle Wobble")]
    [SerializeField] private bool useIdleWobble = true;
    [SerializeField] private float idleWobbleSpeed = 2.5f;
    [SerializeField] private float idleSquashAmount = 0.06f;
    [SerializeField] private float idleSideStretchAmount = 0.035f;

    [Header("Attack Shapes")]
    [SerializeField] private Vector3 windupScale = new Vector3(1.25f, 0.68f, 1.25f);
    [SerializeField] private Vector3 launchScale = new Vector3(0.82f, 1.32f, 1.18f);
    [SerializeField] private Vector3 recoveryScale = new Vector3(1.16f, 0.82f, 1.16f);
    [SerializeField] private float attackBlendTime = 0.08f;

    [Header("Damage Shape")]
    [SerializeField] private Vector3 damagedScale = new Vector3(1.3f, 0.72f, 1.3f);
    [SerializeField] private float damagedInTime = 0.05f;
    [SerializeField] private float damagedOutTime = 0.14f;

    [Header("Death Shape")]
    [SerializeField] private Vector3 deathScale = new Vector3(1.45f, 0.08f, 1.45f);
    [SerializeField] private float deathSquishTime = 0.28f;
    [SerializeField] private bool disableObjectAfterDeath = true;
    [SerializeField] private GameObject objectToDisableOnDeath;

    private Vector3 baseScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private Coroutine shapeRoutine;
    private bool isDead;

    private void Reset()
    {
        visualRoot = transform;
        slimeLaunchEnemy = GetComponentInParent<SlimeLaunchEnemy>();
        damageable = GetComponentInParent<Damageable>();
        objectToDisableOnDeath = transform.root.gameObject;
    }

    private void Awake()
    {
        if (visualRoot == null)
        {
            Debug.LogWarning($"{name}: SlimeVisualDeformer needs a visual root assigned. Disabling to avoid scaling gameplay colliders.", this);
            enabled = false;
            return;
        }

        if (slimeLaunchEnemy == null)
        {
            slimeLaunchEnemy = GetComponentInParent<SlimeLaunchEnemy>();
        }

        if (damageable == null)
        {
            damageable = GetComponentInParent<Damageable>();
        }

        if (objectToDisableOnDeath == null)
        {
            objectToDisableOnDeath = transform.root.gameObject;
        }

        baseScale = visualRoot.localScale;
        targetScale = baseScale;
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnValidate()
    {
        idleWobbleSpeed = Mathf.Max(0f, idleWobbleSpeed);
        idleSquashAmount = Mathf.Max(0f, idleSquashAmount);
        idleSideStretchAmount = Mathf.Max(0f, idleSideStretchAmount);
        attackBlendTime = Mathf.Max(0.01f, attackBlendTime);
        damagedInTime = Mathf.Max(0.01f, damagedInTime);
        damagedOutTime = Mathf.Max(0.01f, damagedOutTime);
        deathSquishTime = Mathf.Max(0.01f, deathSquishTime);
    }

    private void LateUpdate()
    {
        if (visualRoot == null || isDead || shapeRoutine != null)
        {
            return;
        }

        if (!useIdleWobble)
        {
            visualRoot.localScale = targetScale;
            return;
        }

        float wobble = Mathf.Sin(Time.time * idleWobbleSpeed);
        float sideScale = 1f + wobble * idleSideStretchAmount;
        float heightScale = 1f - wobble * idleSquashAmount;
        visualRoot.localScale = Vector3.Scale(targetScale, new Vector3(sideScale, heightScale, sideScale));
    }

    private void Subscribe()
    {
        if (slimeLaunchEnemy != null)
        {
            slimeLaunchEnemy.WindupStarted += HandleWindupStarted;
            slimeLaunchEnemy.LaunchStarted += HandleLaunchStarted;
            slimeLaunchEnemy.RecoveryStarted += HandleRecoveryStarted;
            slimeLaunchEnemy.AttackFinished += HandleAttackFinished;
            slimeLaunchEnemy.AttackInterrupted += HandleAttackInterrupted;
        }

        if (damageable != null)
        {
            damageable.Damaged += HandleDamaged;
            damageable.Died += HandleDied;
        }
    }

    private void Unsubscribe()
    {
        if (slimeLaunchEnemy != null)
        {
            slimeLaunchEnemy.WindupStarted -= HandleWindupStarted;
            slimeLaunchEnemy.LaunchStarted -= HandleLaunchStarted;
            slimeLaunchEnemy.RecoveryStarted -= HandleRecoveryStarted;
            slimeLaunchEnemy.AttackFinished -= HandleAttackFinished;
            slimeLaunchEnemy.AttackInterrupted -= HandleAttackInterrupted;
        }

        if (damageable != null)
        {
            damageable.Damaged -= HandleDamaged;
            damageable.Died -= HandleDied;
        }
    }

    private void HandleWindupStarted()
    {
        BlendTo(Vector3.Scale(baseScale, windupScale), attackBlendTime);
    }

    private void HandleLaunchStarted()
    {
        BlendTo(Vector3.Scale(baseScale, launchScale), attackBlendTime);
    }

    private void HandleRecoveryStarted()
    {
        BlendTo(Vector3.Scale(baseScale, recoveryScale), attackBlendTime);
    }

    private void HandleAttackFinished()
    {
        BlendTo(baseScale, attackBlendTime);
    }

    private void HandleAttackInterrupted()
    {
        BlendTo(baseScale, damagedOutTime);
    }

    private void HandleDamaged()
    {
        if (isDead)
        {
            return;
        }

        StartShapeRoutine(DamagedRoutine());
    }

    private void HandleDied()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        StartShapeRoutine(DeathRoutine());
    }

    private void BlendTo(Vector3 scale, float duration)
    {
        if (isDead)
        {
            return;
        }

        targetScale = scale;
        StartShapeRoutine(BlendRoutine(scale, duration));
    }

    private void StartShapeRoutine(IEnumerator routine)
    {
        if (shapeRoutine != null)
        {
            StopCoroutine(shapeRoutine);
        }

        shapeRoutine = StartCoroutine(RunShapeRoutine(routine));
    }

    private IEnumerator DamagedRoutine()
    {
        yield return BlendRoutine(Vector3.Scale(baseScale, damagedScale), damagedInTime);
        yield return BlendRoutine(targetScale, damagedOutTime);
    }

    private IEnumerator DeathRoutine()
    {
        yield return BlendRoutine(Vector3.Scale(baseScale, deathScale), deathSquishTime);

        if (disableObjectAfterDeath && objectToDisableOnDeath != null)
        {
            objectToDisableOnDeath.SetActive(false);
        }
    }

    private IEnumerator RunShapeRoutine(IEnumerator routine)
    {
        yield return routine;
        shapeRoutine = null;
    }

    private IEnumerator BlendRoutine(Vector3 scale, float duration)
    {
        Vector3 startScale = visualRoot.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            visualRoot.localScale = Vector3.LerpUnclamped(startScale, scale, t);
            yield return null;
        }

        visualRoot.localScale = scale;
    }
}
