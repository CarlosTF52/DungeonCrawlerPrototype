using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private string colorProperty = "_BaseColor";

    private Color originalColor;
    private Coroutine flashRoutine;
    private Material runtimeMaterial;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            runtimeMaterial = targetRenderer.material;
            originalColor = GetMaterialColor();
        }
    }

    private void OnValidate()
    {
        flashDuration = Mathf.Max(0.01f, flashDuration);
    }

    public void PlayFlash()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetMaterialColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetMaterialColor(originalColor);
        flashRoutine = null;
    }

    private Color GetMaterialColor()
    {
        if (runtimeMaterial.HasProperty(colorProperty))
        {
            return runtimeMaterial.GetColor(colorProperty);
        }

        return runtimeMaterial.color;
    }

    private void SetMaterialColor(Color color)
    {
        if (runtimeMaterial.HasProperty(colorProperty))
        {
            runtimeMaterial.SetColor(colorProperty, color);
            return;
        }

        runtimeMaterial.color = color;
    }
}
