using System.Collections;
using UnityEngine;

public class DamageInvincibilityBlink : MonoBehaviour
{
    [SerializeField] private Damageable damageable;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private float blinkInterval = 0.08f;

    private Coroutine blinkRoutine;

    private void Awake()
    {
        if (damageable == null)
        {
            damageable = GetComponent<Damageable>();
        }

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
    }

    private void OnValidate()
    {
        blinkInterval = Mathf.Max(0.01f, blinkInterval);
    }

    public void PlayBlink()
    {
        if (damageable == null || renderers == null || renderers.Length == 0)
        {
            return;
        }

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        bool visible = true;

        while (damageable.IsInvincible)
        {
            visible = !visible;
            SetVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
        }

        SetVisible(true);
        blinkRoutine = null;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer != null)
            {
                targetRenderer.enabled = visible;
            }
        }
    }
}
