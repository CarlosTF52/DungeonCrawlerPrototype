using UnityEngine;
using UnityEngine.UI;

public class PlayerVitalsHud : MonoBehaviour
{
    [SerializeField] private Damageable playerHealth;
    [SerializeField] private StaminaPool playerStamina;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image staminaFill;
    [SerializeField] private float fillSmoothSpeed = 18f;

    private float displayedHealthFill = 1f;
    private float displayedStaminaFill = 1f;

    private void Awake()
    {
        ResolvePlayerReferences();
        InitializeFills();
    }

    private void OnValidate()
    {
        fillSmoothSpeed = Mathf.Max(0f, fillSmoothSpeed);
    }

    private void Update()
    {
        if (playerHealth == null || playerStamina == null)
        {
            ResolvePlayerReferences();
        }

        UpdateFill(ref displayedHealthFill, GetHealthPercent(), healthFill);
        UpdateFill(ref displayedStaminaFill, GetStaminaPercent(), staminaFill);
    }

    public void SetPlayer(Damageable health, StaminaPool stamina)
    {
        playerHealth = health;
        playerStamina = stamina;
        InitializeFills();
    }

    private void ResolvePlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            return;
        }

        if (playerHealth == null)
        {
            playerHealth = player.GetComponentInParent<Damageable>();
        }

        if (playerStamina == null)
        {
            playerStamina = player.GetComponentInParent<StaminaPool>();
        }
    }

    private void InitializeFills()
    {
        displayedHealthFill = GetHealthPercent();
        displayedStaminaFill = GetStaminaPercent();

        SetFill(healthFill, displayedHealthFill);
        SetFill(staminaFill, displayedStaminaFill);
    }

    private void UpdateFill(ref float displayedValue, float targetValue, Image fill)
    {
        if (fill == null)
        {
            return;
        }

        if (fillSmoothSpeed <= 0f)
        {
            displayedValue = targetValue;
        }
        else
        {
            displayedValue = Mathf.Lerp(displayedValue, targetValue, 1f - Mathf.Exp(-fillSmoothSpeed * Time.deltaTime));
        }

        SetFill(fill, displayedValue);
    }

    private float GetHealthPercent()
    {
        if (playerHealth == null || playerHealth.MaxHealth <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01((float)playerHealth.CurrentHealth / playerHealth.MaxHealth);
    }

    private float GetStaminaPercent()
    {
        if (playerStamina == null || playerStamina.MaxStamina <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(playerStamina.CurrentStamina / playerStamina.MaxStamina);
    }

    private static void SetFill(Image fill, float value)
    {
        if (fill == null)
        {
            return;
        }

        fill.fillAmount = Mathf.Clamp01(value);
    }
}
