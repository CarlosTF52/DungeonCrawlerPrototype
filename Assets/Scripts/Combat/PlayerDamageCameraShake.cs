using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerDamageCameraShake : MonoBehaviour
{
    [SerializeField] private Damageable playerHealth;
    [SerializeField] private Transform cameraShakeTarget;
    [SerializeField] private float basePositionStrength = 0.04f;
    [SerializeField] private float positionStrengthPerDamage = 0.015f;
    [SerializeField] private float baseRotationStrength = 1.2f;
    [SerializeField] private float rotationStrengthPerDamage = 0.35f;
    [SerializeField] private float baseDuration = 0.12f;
    [SerializeField] private float durationPerDamage = 0.025f;
    [SerializeField] private float frequency = 32f;
    [SerializeField] private float recoverySpeed = 22f;

    private Coroutine shakeRoutine;
    private Vector3 originalLocalPosition;
    private Quaternion previousShakeRotation = Quaternion.identity;
    private float trauma;
    private float seed;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<Damageable>();
        }

        if (cameraShakeTarget == null)
        {
            cameraShakeTarget = transform;
        }

        originalLocalPosition = cameraShakeTarget.localPosition;
        seed = Random.value * 1000f;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged += HandlePlayerDamaged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged -= HandlePlayerDamaged;
        }

        ResetShakeTarget();
    }

    private void OnValidate()
    {
        basePositionStrength = Mathf.Max(0f, basePositionStrength);
        positionStrengthPerDamage = Mathf.Max(0f, positionStrengthPerDamage);
        baseRotationStrength = Mathf.Max(0f, baseRotationStrength);
        rotationStrengthPerDamage = Mathf.Max(0f, rotationStrengthPerDamage);
        baseDuration = Mathf.Max(0f, baseDuration);
        durationPerDamage = Mathf.Max(0f, durationPerDamage);
        frequency = Mathf.Max(0f, frequency);
        recoverySpeed = Mathf.Max(0f, recoverySpeed);
    }

    public void ShakeForDamage()
    {
        int damageAmount = playerHealth != null ? playerHealth.LastDamageAmount : 1;
        Shake(damageAmount);
    }

    public void Shake(int damageAmount)
    {
        damageAmount = Mathf.Max(1, damageAmount);

        float duration = baseDuration + durationPerDamage * damageAmount;
        float positionStrength = basePositionStrength + positionStrengthPerDamage * damageAmount;
        float rotationStrength = baseRotationStrength + rotationStrengthPerDamage * damageAmount;

        trauma = 1f;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, positionStrength, rotationStrength));
    }

    private void HandlePlayerDamaged()
    {
        ShakeForDamage();
    }

    private IEnumerator ShakeRoutine(float duration, float positionStrength, float rotationStrength)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float life = 1f - Mathf.Clamp01(elapsed / duration);
            trauma = Mathf.MoveTowards(trauma, 0f, Time.deltaTime * recoverySpeed / Mathf.Max(0.01f, duration));
            float intensity = Mathf.Max(life, trauma);

            float time = Time.time * frequency;
            Vector3 positionOffset = new Vector3(
                Noise(time, seed),
                Noise(time, seed + 17f),
                0f) * positionStrength * intensity;

            Vector3 rotationOffset = new Vector3(
                Noise(time, seed + 31f),
                Noise(time, seed + 47f),
                Noise(time, seed + 59f)) * rotationStrength * intensity;

            Quaternion nextShakeRotation = Quaternion.Euler(rotationOffset);
            cameraShakeTarget.localPosition = originalLocalPosition + positionOffset;
            cameraShakeTarget.localRotation = cameraShakeTarget.localRotation * Quaternion.Inverse(previousShakeRotation) * nextShakeRotation;
            previousShakeRotation = nextShakeRotation;

            yield return null;
        }

        ResetShakeTarget();
        shakeRoutine = null;
    }

    private static float Noise(float time, float offset)
    {
        return Mathf.PerlinNoise(time + offset, offset) * 2f - 1f;
    }

    private void ResetShakeTarget()
    {
        if (cameraShakeTarget != null)
        {
            cameraShakeTarget.localPosition = originalLocalPosition;
            cameraShakeTarget.localRotation *= Quaternion.Inverse(previousShakeRotation);
        }

        previousShakeRotation = Quaternion.identity;
    }
}
