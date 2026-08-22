using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVitalsHud : MonoBehaviour
{
    private enum StressDisplayMode
    {
        StressAccumulated,
        SanityRemaining
    }

    [SerializeField] private Damageable playerHealth;
    [SerializeField] private StaminaPool playerStamina;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image staminaFill;
    [SerializeField] private Image staminaSpentFlashFill;
    [SerializeField] private Image stressFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private TMP_Text stressText;
    [SerializeField] private float fillSmoothSpeed = 18f;

    [Header("Stress / Sanity")]
    [SerializeField] private StressDisplayMode stressDisplayMode = StressDisplayMode.StressAccumulated;
    [SerializeField] private string sanityLabel = "Sanity";
    [SerializeField] private string stressLabel = "Stress";

    [Header("Warning State")]
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;
    [SerializeField, Range(0f, 1f)] private float lowStaminaThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] private float lowSanityThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] private float highStressThreshold = 0.75f;
    [SerializeField] private Color normalHealthColor = new Color(0.8f, 0.08f, 0.06f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(1f, 0.18f, 0.08f, 1f);
    [SerializeField] private Color healthDamageFlashColor = Color.white;
    [SerializeField] private float healthDamageFlashDuration = 0.12f;
    [SerializeField] private Color normalStaminaColor = new Color(0.9f, 0.72f, 0.18f, 1f);
    [SerializeField] private Color lowStaminaColor = new Color(0.95f, 0.4f, 0.08f, 1f);
    [SerializeField] private Color staminaSpentFlashColor = Color.white;
    [SerializeField] private float staminaSpentFlashDuration = 0.1f;
    [SerializeField] private Color normalSanityColor = new Color(0.55f, 0.15f, 0.95f, 1f);
    [SerializeField] private Color lowSanityColor = new Color(0.78f, 0.2f, 1f, 1f);
    [SerializeField] private Color normalStressColor = new Color(0.55f, 0.15f, 0.95f, 1f);
    [SerializeField] private Color highStressColor = new Color(1f, 0.15f, 0.55f, 1f);

    private float displayedHealthFill = 1f;
    private float displayedStaminaFill = 1f;
    private float displayedStressFill = 1f;
    private int displayedHealth = int.MinValue;
    private int displayedMaxHealth = int.MinValue;
    private int displayedStamina = int.MinValue;
    private int displayedMaxStamina = int.MinValue;
    private int displayedStress = int.MinValue;
    private int displayedMaxStress = int.MinValue;
    private int displayedSanity = int.MinValue;
    private Damageable subscribedHealth;
    private float healthFlashUntilTime;
    private float previousStaminaValue = float.NaN;
    private float staminaFlashUntilTime;
    private float staminaFlashStartedTime;

    private void Awake()
    {
        ResolvePlayerReferences();
        InitializeFills();
    }

    private void OnValidate()
    {
        fillSmoothSpeed = Mathf.Max(0f, fillSmoothSpeed);
        healthDamageFlashDuration = Mathf.Max(0f, healthDamageFlashDuration);
        staminaSpentFlashDuration = Mathf.Max(0f, staminaSpentFlashDuration);
    }

    private void OnEnable()
    {
        SubscribeToHealthDamage();
    }

    private void OnDisable()
    {
        UnsubscribeFromHealthDamage();
    }

    private void Update()
    {
        if (playerHealth == null || playerStamina == null)
        {
            ResolvePlayerReferences();
        }

        SubscribeToHealthDamage();
        UpdateStaminaSpentFlash();
        UpdateFill(ref displayedHealthFill, GetHealthPercent(), healthFill);
        UpdateFill(ref displayedStaminaFill, GetStaminaPercent(), staminaFill);
        UpdateFill(ref displayedStressFill, GetStressFillPercent(), stressFill);
        UpdateLabels();
        UpdateWarningColors();
    }

    public void SetPlayer(Damageable health, StaminaPool stamina)
    {
        UnsubscribeFromHealthDamage();
        playerHealth = health;
        playerStamina = stamina;
        previousStaminaValue = float.NaN;
        SubscribeToHealthDamage();
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
        displayedStressFill = GetStressFillPercent();
        previousStaminaValue = GetCurrentStaminaValue();

        SetFill(healthFill, displayedHealthFill);
        SetFill(staminaFill, displayedStaminaFill);
        SetFill(stressFill, displayedStressFill);
        SetStaminaSpentFlashVisible(false);
        UpdateLabels(true);
        UpdateWarningColors();
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

    private int GetCurrentHealth()
    {
        return playerHealth != null ? Mathf.Max(0, playerHealth.CurrentHealth) : 0;
    }

    private int GetMaxHealth()
    {
        return playerHealth != null ? Mathf.Max(0, playerHealth.MaxHealth) : 0;
    }

    private float GetStaminaPercent()
    {
        if (playerStamina == null || playerStamina.MaxStamina <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(playerStamina.CurrentStamina / playerStamina.MaxStamina);
    }

    private int GetCurrentStaminaRounded()
    {
        return playerStamina != null ? Mathf.CeilToInt(Mathf.Max(0f, playerStamina.CurrentStamina)) : 0;
    }

    private int GetMaxStaminaRounded()
    {
        return playerStamina != null ? Mathf.CeilToInt(Mathf.Max(0f, playerStamina.MaxStamina)) : 0;
    }

    private float GetStressFillPercent()
    {
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        CharacterDefinition activeCharacter = rosterManager.ActiveCharacter;

        if (activeCharacter == null)
        {
            return 0f;
        }

        return stressDisplayMode == StressDisplayMode.SanityRemaining
            ? rosterManager.GetSanityPercent(activeCharacter)
            : rosterManager.GetStressPercent(activeCharacter);
    }

    private int GetCurrentStress()
    {
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        CharacterDefinition activeCharacter = rosterManager.ActiveCharacter;
        return activeCharacter != null ? rosterManager.GetStress(activeCharacter) : 0;
    }

    private int GetMaxStress()
    {
        CharacterRosterManager rosterManager = CharacterRosterManager.Instance;
        CharacterDefinition activeCharacter = rosterManager.ActiveCharacter;
        return activeCharacter != null ? rosterManager.GetMaxStress(activeCharacter) : rosterManager.BaseMaxStress;
    }

    private void UpdateLabels(bool force = false)
    {
        int health = GetCurrentHealth();
        int maxHealth = GetMaxHealth();
        int stamina = GetCurrentStaminaRounded();
        int maxStamina = GetMaxStaminaRounded();
        int stress = GetCurrentStress();
        int maxStress = GetMaxStress();
        int sanity = Mathf.Max(0, maxStress - stress);

        if (force || health != displayedHealth || maxHealth != displayedMaxHealth)
        {
            displayedHealth = health;
            displayedMaxHealth = maxHealth;
            SetText(healthText, $"{health}/{maxHealth}");
        }

        if (force || stamina != displayedStamina || maxStamina != displayedMaxStamina)
        {
            displayedStamina = stamina;
            displayedMaxStamina = maxStamina;
            SetText(staminaText, $"{stamina}/{maxStamina}");
        }

        if (force || stress != displayedStress || maxStress != displayedMaxStress || sanity != displayedSanity)
        {
            displayedStress = stress;
            displayedMaxStress = maxStress;
            displayedSanity = sanity;
            SetText(stressText, BuildStressText(stress, sanity, maxStress));
        }
    }

    private void UpdateWarningColors()
    {
        SetColor(healthFill, GetHealthColor());
        SetColor(staminaFill, GetStaminaColor());
        SetColor(stressFill, GetStressColor());
    }

    private Color GetHealthColor()
    {
        if (Time.time < healthFlashUntilTime)
        {
            return healthDamageFlashColor;
        }

        return displayedHealthFill <= lowHealthThreshold ? lowHealthColor : normalHealthColor;
    }

    private Color GetStaminaColor()
    {
        return displayedStaminaFill <= lowStaminaThreshold ? lowStaminaColor : normalStaminaColor;
    }

    private Color GetStressColor()
    {
        if (stressDisplayMode == StressDisplayMode.SanityRemaining)
        {
            return displayedStressFill <= lowSanityThreshold ? lowSanityColor : normalSanityColor;
        }

        return displayedStressFill >= highStressThreshold ? highStressColor : normalStressColor;
    }

    private string BuildStressText(int stress, int sanity, int maxStress)
    {
        if (stressDisplayMode == StressDisplayMode.SanityRemaining)
        {
            return $"{sanityLabel}: {sanity}/{maxStress}";
        }

        return $"{stressLabel}: {stress}/{maxStress}";
    }

    private void SubscribeToHealthDamage()
    {
        if (subscribedHealth == playerHealth)
        {
            return;
        }

        UnsubscribeFromHealthDamage();
        subscribedHealth = playerHealth;

        if (subscribedHealth != null)
        {
            subscribedHealth.Damaged += HandlePlayerDamaged;
        }
    }

    private void UnsubscribeFromHealthDamage()
    {
        if (subscribedHealth != null)
        {
            subscribedHealth.Damaged -= HandlePlayerDamaged;
            subscribedHealth = null;
        }
    }

    private void HandlePlayerDamaged()
    {
        healthFlashUntilTime = Time.time + healthDamageFlashDuration;
    }

    private void UpdateStaminaSpentFlash()
    {
        float currentStamina = GetCurrentStaminaValue();

        if (!float.IsNaN(previousStaminaValue) && currentStamina < previousStaminaValue)
        {
            ShowStaminaSpentFlash(previousStaminaValue, currentStamina);
        }

        previousStaminaValue = currentStamina;
        UpdateStaminaSpentFlashFade();
    }

    private float GetCurrentStaminaValue()
    {
        return playerStamina != null ? Mathf.Max(0f, playerStamina.CurrentStamina) : 0f;
    }

    private void ShowStaminaSpentFlash(float previousStamina, float currentStamina)
    {
        if (playerStamina == null || playerStamina.MaxStamina <= 0f || !CanUseStaminaSpentFlashFill())
        {
            return;
        }

        float previousPercent = Mathf.Clamp01(previousStamina / playerStamina.MaxStamina);
        float currentPercent = Mathf.Clamp01(currentStamina / playerStamina.MaxStamina);

        if (previousPercent <= currentPercent)
        {
            return;
        }

        RectTransform flashRect = staminaSpentFlashFill.rectTransform;
        flashRect.anchorMin = new Vector2(currentPercent, flashRect.anchorMin.y);
        flashRect.anchorMax = new Vector2(previousPercent, flashRect.anchorMax.y);
        flashRect.offsetMin = new Vector2(0f, flashRect.offsetMin.y);
        flashRect.offsetMax = new Vector2(0f, flashRect.offsetMax.y);

        staminaFlashStartedTime = Time.time;
        staminaFlashUntilTime = Time.time + staminaSpentFlashDuration;
        SetStaminaSpentFlashVisible(true);
        SetColor(staminaSpentFlashFill, staminaSpentFlashColor);
    }

    private void UpdateStaminaSpentFlashFade()
    {
        if (!CanUseStaminaSpentFlashFill())
        {
            return;
        }

        if (Time.time >= staminaFlashUntilTime || staminaSpentFlashDuration <= 0f)
        {
            SetStaminaSpentFlashVisible(false);
            return;
        }

        float progress = Mathf.InverseLerp(staminaFlashStartedTime, staminaFlashUntilTime, Time.time);
        Color color = staminaSpentFlashColor;
        color.a *= 1f - progress;
        SetColor(staminaSpentFlashFill, color);
    }

    private void SetStaminaSpentFlashVisible(bool visible)
    {
        if (CanUseStaminaSpentFlashFill())
        {
            staminaSpentFlashFill.gameObject.SetActive(visible);
        }
    }

    private bool CanUseStaminaSpentFlashFill()
    {
        return staminaSpentFlashFill != null && staminaSpentFlashFill != staminaFill;
    }

    private static void SetFill(Image fill, float value)
    {
        if (fill == null)
        {
            return;
        }

        fill.fillAmount = Mathf.Clamp01(value);
    }

    private static void SetColor(Image fill, Color color)
    {
        if (fill != null)
        {
            fill.color = color;
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
