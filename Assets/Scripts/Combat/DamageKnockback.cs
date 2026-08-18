using System.Collections;
using StarterAssets;
using UnityEngine;

public class DamageKnockback : MonoBehaviour
{
    [SerializeField] private Damageable damageable;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Rigidbody attachedRigidbody;
    [SerializeField] private SimpleEnemyChase enemyChase;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private bool preferRigidbody;
    [SerializeField] private float distance = 0.35f;
    [SerializeField] private bool useUpwardImpulse = true;
    [SerializeField] private float upwardImpulse = 2f;
    [SerializeField] private float duration = 0.08f;
    [SerializeField] private float staggerDuration = 0.25f;

    private Coroutine knockbackRoutine;

    private void Awake()
    {
        if (damageable == null)
        {
            damageable = GetComponent<Damageable>();
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (firstPersonController == null)
        {
            firstPersonController = GetComponent<FirstPersonController>();
        }

        if (attachedRigidbody == null)
        {
            attachedRigidbody = GetComponent<Rigidbody>();
        }

        if (enemyChase == null)
        {
            enemyChase = GetComponent<SimpleEnemyChase>();
        }
    }

    private void OnValidate()
    {
        distance = Mathf.Max(0f, distance);
        upwardImpulse = Mathf.Max(0f, upwardImpulse);
        duration = Mathf.Max(0.01f, duration);
        staggerDuration = Mathf.Max(0f, staggerDuration);
    }

    public void PlayKnockback()
    {
        if (damageable == null || damageable.LastDamageSource == null)
        {
            return;
        }

        Vector3 direction = transform.position - damageable.LastDamageSource.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = -transform.forward;
        }

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        if (enemyChase != null)
        {
            enemyChase.Stagger(staggerDuration);
        }

        if (useUpwardImpulse && attachedRigidbody != null && !attachedRigidbody.isKinematic)
        {
            attachedRigidbody.AddForce(Vector3.up * upwardImpulse, ForceMode.VelocityChange);
        }

        if (firstPersonController != null)
        {
            firstPersonController.AddExternalVelocity(direction.normalized * (distance / duration));
            return;
        }

        knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float previousProgress = elapsed / duration;
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            Vector3 previousOffset = CalculateOffset(direction, previousProgress);
            Vector3 nextOffset = CalculateOffset(direction, progress);
            Vector3 movement = nextOffset - previousOffset;

            if (preferRigidbody && attachedRigidbody != null)
            {
                attachedRigidbody.MovePosition(attachedRigidbody.position + movement);
            }
            else if (characterController != null)
            {
                characterController.Move(movement);
            }
            else if (attachedRigidbody != null)
            {
                attachedRigidbody.MovePosition(attachedRigidbody.position + movement);
            }
            else
            {
                transform.position += movement;
            }

            yield return null;
        }

        knockbackRoutine = null;
    }

    private Vector3 CalculateOffset(Vector3 direction, float progress)
    {
        float horizontalDistance = distance * progress;
        return direction * horizontalDistance;
    }
}
