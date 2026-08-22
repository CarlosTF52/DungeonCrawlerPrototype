using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class StressAtmosphereController : MonoBehaviour
{
    [SerializeField] private Volume targetVolume;
    [SerializeField] private bool autoFindGlobalVolume = true;
    [SerializeField] private bool enableCameraPostProcessing = true;
    [SerializeField] private bool useCurrentVolumeAsCalm = true;
    [SerializeField] private float smoothSpeed = 3.5f;
    [SerializeField] private AnimationCurve stressResponse = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Vignette")]
    [SerializeField, Range(0f, 1f)] private float calmVignetteIntensity = 0.15f;
    [SerializeField, Range(0f, 1f)] private float stressedVignetteIntensity = 0.48f;
    [SerializeField] private Color calmVignetteColor = Color.black;
    [SerializeField] private Color stressedVignetteColor = new Color(0.08f, 0.02f, 0.16f, 1f);

    [Header("Color")]
    [SerializeField] private float calmPostExposure;
    [SerializeField] private float stressedPostExposure = -1.05f;
    [SerializeField] private float calmSaturation;
    [SerializeField] private float stressedSaturation = -45f;
    [SerializeField] private float calmContrast;
    [SerializeField] private float stressedContrast = 18f;
    [SerializeField] private Color calmColorFilter = Color.white;
    [SerializeField] private Color stressedColorFilter = new Color(0.72f, 0.62f, 1f, 1f);

    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private float displayedStressPercent;
    private bool capturedCalmValues;
    private float nextCameraRefreshTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<StressAtmosphereController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("Stress Atmosphere Controller");
        DontDestroyOnLoad(controllerObject);
        controllerObject.AddComponent<StressAtmosphereController>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ResolveVolume();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnValidate()
    {
        smoothSpeed = Mathf.Max(0f, smoothSpeed);
    }

    private void Update()
    {
        if (targetVolume == null || vignette == null || colorAdjustments == null)
        {
            ResolveVolume();
        }

        RefreshCameraPostProcessing();

        float stressPercent = GetStressPercent();

        if (smoothSpeed <= 0f)
        {
            displayedStressPercent = stressPercent;
        }
        else
        {
            displayedStressPercent = Mathf.Lerp(displayedStressPercent, stressPercent, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        }

        ApplyAtmosphere(stressResponse.Evaluate(Mathf.Clamp01(displayedStressPercent)));
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        targetVolume = null;
        vignette = null;
        colorAdjustments = null;
        capturedCalmValues = false;
        nextCameraRefreshTime = 0f;
        ResolveVolume();
    }

    private void ResolveVolume()
    {
        if (targetVolume == null && autoFindGlobalVolume)
        {
            targetVolume = FindBestGlobalVolume();
        }

        if (targetVolume == null)
        {
            return;
        }

        targetVolume.isGlobal = true;
        targetVolume.weight = 1f;

        VolumeProfile profile = targetVolume.profile;

        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            targetVolume.sharedProfile = profile;
        }

        if (!profile.TryGet(out vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }

        if (!profile.TryGet(out colorAdjustments))
        {
            colorAdjustments = profile.Add<ColorAdjustments>(true);
        }

        CaptureCalmValuesIfNeeded();
        RefreshCameraPostProcessing();
    }

    private void RefreshCameraPostProcessing()
    {
        if (!enableCameraPostProcessing || Time.unscaledTime < nextCameraRefreshTime)
        {
            return;
        }

        nextCameraRefreshTime = Time.unscaledTime + 1f;
        Camera[] cameras = Camera.allCameras;

        for (int i = 0; i < cameras.Length; i++)
        {
            UniversalAdditionalCameraData cameraData = cameras[i] != null ? cameras[i].GetUniversalAdditionalCameraData() : null;

            if (cameraData != null)
            {
                cameraData.renderPostProcessing = true;
            }
        }
    }

    private void CaptureCalmValuesIfNeeded()
    {
        if (!useCurrentVolumeAsCalm || capturedCalmValues || vignette == null || colorAdjustments == null)
        {
            return;
        }

        calmVignetteIntensity = vignette.intensity.value;
        calmVignetteColor = vignette.color.value;
        calmPostExposure = colorAdjustments.postExposure.value;
        calmSaturation = colorAdjustments.saturation.value;
        calmContrast = colorAdjustments.contrast.value;
        calmColorFilter = colorAdjustments.colorFilter.value;
        capturedCalmValues = true;
    }

    private void ApplyAtmosphere(float stressAmount)
    {
        if (vignette == null || colorAdjustments == null)
        {
            return;
        }

        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.color.overrideState = true;
        vignette.intensity.value = Mathf.Lerp(calmVignetteIntensity, stressedVignetteIntensity, stressAmount);
        vignette.color.value = Color.Lerp(calmVignetteColor, stressedVignetteColor, stressAmount);

        colorAdjustments.active = true;
        colorAdjustments.postExposure.overrideState = true;
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.postExposure.value = Mathf.Lerp(calmPostExposure, stressedPostExposure, stressAmount);
        colorAdjustments.saturation.value = Mathf.Lerp(calmSaturation, stressedSaturation, stressAmount);
        colorAdjustments.contrast.value = Mathf.Lerp(calmContrast, stressedContrast, stressAmount);
        colorAdjustments.colorFilter.value = Color.Lerp(calmColorFilter, stressedColorFilter, stressAmount);
    }

    private static Volume FindBestGlobalVolume()
    {
        Volume[] volumes = FindObjectsOfType<Volume>();
        Volume bestVolume = null;
        float bestPriority = float.NegativeInfinity;

        for (int i = 0; i < volumes.Length; i++)
        {
            Volume volume = volumes[i];

            if (volume == null || !volume.isGlobal || !volume.isActiveAndEnabled || volume.priority < bestPriority)
            {
                continue;
            }

            bestVolume = volume;
            bestPriority = volume.priority;
        }

        return bestVolume;
    }

    private static float GetStressPercent()
    {
        CharacterRosterManager rosterManager = FindObjectOfType<CharacterRosterManager>();

        if (rosterManager == null || rosterManager.ActiveCharacter == null)
        {
            return 0f;
        }

        return rosterManager.GetStressPercent(rosterManager.ActiveCharacter);
    }
}
