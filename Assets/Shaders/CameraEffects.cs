using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    [Header("Shader / Material")]
    [SerializeField] private Shader effectShader;
    [SerializeField] private Material materialOverride;

    [Header("Toon / Color")]
    [ColorUsage(false, true)][SerializeField] private Color tint = Color.white;
    [Range(0, 1)][SerializeField] private float tintIntensity = 1f;
    [Range(2, 32)][SerializeField] private int posterizeSteps = 8;

    [Header("Vignette")]
    [SerializeField] private bool enableVignette = true;
    [Tooltip("Inner radius where vignette has no effect.")]
    [Range(0f, 1f)][SerializeField] private float vignetteInnerRadius = 0.3f;
    [Tooltip("Outer radius where vignette reaches full effect.")]
    [Range(0f, 1f)][SerializeField] private float vignetteOuterRadius = 0.75f;
    [Tooltip("Softness falloff between inner and outer radii.")]
    [Range(0.0001f, 1f)][SerializeField] private float vignetteSoftness = 0.2f;
    [Tooltip("Intensity multiplier of darkening / tinting.")]
    [Range(0f, 2f)][SerializeField] private float vignetteIntensity = 1f;
    [Tooltip("Optional color tint for the vignette.")]
    [ColorUsage(false, true)][SerializeField] private Color vignetteColor = Color.black;
    [Tooltip("Center offset (0.5,0.5 is screen center).")]
    [SerializeField] private Vector2 vignetteCenter = new(0.5f, 0.5f);
    [Tooltip("Horizontal / Vertical stretch (1,1 = circle).")]
    [SerializeField] private Vector2 vignetteAxisScale = new(1f, 1f);
    [Tooltip("0 = more rectangular (respect axis scale), 1 = perfectly rounded.")]
    [Range(0f, 1f)][SerializeField] private float vignetteRoundness = 1f;

    [Header("Outline (Depth-Based)")]
    [SerializeField] private bool enableOutline = true;
    [SerializeField] private bool silhouetteOnly = true;
    [SerializeField] private Color outlineColor = Color.black;
    [Range(0.5f, 4f)][SerializeField] private float outlineThickness = 1f;
    [Range(0.0005f, 0.02f)][SerializeField] private float depthThreshold = 0.003f;
    [Range(0.0001f, 0.02f)][SerializeField] private float outlineSoftness = 0.002f;
    [Range(0, 3f)][SerializeField] private float outlineStrength = 1f;
    [Range(0.90f, 1f)][SerializeField] private float silhouetteBackgroundThreshold = 0.995f;

    [Header("Film Grain")]
    [SerializeField] private bool enableFilmGrain = false;
    [Tooltip("Overall opacity of grain overlay.")]
    [Range(0f, 1f)][SerializeField] private float grainIntensity = 0.35f;
    [Tooltip("Higher = finer (more tiles across screen).")]
    [Range(32f, 1024f)][SerializeField] private float grainScale = 320f;
    [Tooltip("Animation speed of grain movement.")]
    [Range(0f, 10f)][SerializeField] private float grainSpeed = 1.5f;
    [Tooltip("How much grain is reduced in bright areas (1 = less visible in highlights).")]
    [Range(0f, 2f)][SerializeField] private float grainLuminanceResponse = 1f;
    [Tooltip("Seed offset for per-camera variation.")]
    [SerializeField] private Vector2 grainSeedJitter = new(37.2f, 91.7f);

    [Header("Pixelation")]
    [SerializeField] private bool pixelateOn = false;
    [Range(1, 64)][SerializeField] private int pixelSize = 8;

    private Material runtimeMaterial;

    private static readonly Dictionary<Camera, CameraEffects> registry = new();
    public static bool TryGet(Camera cam, out CameraEffects fx) => registry.TryGetValue(cam, out fx);

    public bool HasVisibleChange =>
        tintIntensity > 0.0001f ||
        posterizeSteps != 8 ||
        tint != Color.white ||
        (enableVignette && vignetteIntensity > 0.001f) ||
        (enableOutline && outlineStrength > 0.01f) ||
        (enableFilmGrain && grainIntensity > 0.01f);

    public Material Material
    {
        get
        {
            if (materialOverride != null) return materialOverride;
            if (runtimeMaterial == null && effectShader != null)
                runtimeMaterial = new Material(effectShader) { hideFlags = HideFlags.HideAndDontSave };
            return runtimeMaterial;
        }
    }

    public void ApplyToMaterial(Material m)
    {
        if (m == null) return;

        // Base
        m.SetColor("_Tint", tint);
        m.SetFloat("_Intensity", tintIntensity);
        m.SetInt("_PosterizeSteps", posterizeSteps);

        // Vignette
        m.SetFloat("_VignetteInnerRadius", vignetteInnerRadius);
        m.SetFloat("_VignetteOuterRadius", Mathf.Max(vignetteOuterRadius, vignetteInnerRadius + 0.0001f));
        m.SetFloat("_VignetteSoftness", vignetteSoftness);
        m.SetFloat("_VignetteIntensity", vignetteIntensity);
        m.SetColor("_VignetteColor", vignetteColor);
        m.SetVector("_VignetteCenter", vignetteCenter);
        m.SetVector("_VignetteAxisScale", vignetteAxisScale);
        m.SetFloat("_VignetteRoundness", vignetteRoundness);

        // Outline
        m.SetColor("_OutlineColor", outlineColor);
        m.SetFloat("_OutlineThickness", outlineThickness);
        m.SetFloat("_DepthThreshold", depthThreshold);
        m.SetFloat("_OutlineSoftness", outlineSoftness);
        m.SetFloat("_OutlineStrength", outlineStrength);
        m.SetFloat("_SilhouetteBackgroundThreshold", silhouetteBackgroundThreshold);

        // Film Grain
        m.SetFloat("_GrainIntensity", grainIntensity);
        m.SetFloat("_GrainScale", grainScale);
        m.SetFloat("_GrainSpeed", grainSpeed);
        m.SetFloat("_GrainLumaResponse", grainLuminanceResponse);
        m.SetVector("_GrainSeedJitter", grainSeedJitter);

        // Pixelation
        m.SetFloat("_PixelateEnabled", pixelateOn ? 1f : 0f);
        m.SetFloat("_PixelSize", pixelSize);
        if (pixelateOn)
            m.EnableKeyword("PIXELATE_ENABLED");
        else
            m.DisableKeyword("PIXELATE_ENABLED");

        // Keywords
        SetKeyword(m, "VIGNETTE_ENABLED", enableVignette && vignetteIntensity > 0.001f);
        bool outlineActive = enableOutline && outlineStrength > 0.01f;
        SetKeyword(m, "OUTLINE_ENABLED", outlineActive);
        SetKeyword(m, "OUTLINE_SILHOUETTE", outlineActive && silhouetteOnly);
        SetKeyword(m, "FILMGRAIN_ENABLED", enableFilmGrain && grainIntensity > 0.01f);
    }

    private void SetKeyword(Material m, string keyword, bool enabled)
    {
        if (enabled) m.EnableKeyword(keyword);
        else m.DisableKeyword(keyword);
    }

    private void OnEnable()
    {
        var cam = GetComponent<Camera>();
        registry[cam] = this;

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Enable camera effects (if using a post-process or OnRenderImage, ensure it's active)
        EnableCameraEffects();
    }

    private void OnDisable()
    {
        var cam = GetComponent<Camera>();
        registry.Remove(cam);

        if (runtimeMaterial != null)
        {
            if (Application.isPlaying) Destroy(runtimeMaterial);
            else DestroyImmediate(runtimeMaterial);
        }

        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        DisableCameraEffects();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-register the camera in case the registry is cleared or objects are reloaded
        var cam = GetComponent<Camera>();
        if (!registry.ContainsKey(cam))
            registry[cam] = this;


        if (Material != null)
            ApplyToMaterial(Material);

        EnableCameraEffects();
    }

    /// <summary>
    /// Ensures camera effects are enabled (for example, by enabling this component or related scripts).
    /// </summary>
    private void EnableCameraEffects()
    {
        enabled = true;
    }

    /// <summary>
    /// Ensures camera effects are disabled (for example, by disabling this component or related scripts).
    /// </summary>
    private void DisableCameraEffects()
    {
        enabled = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        posterizeSteps = Mathf.Clamp(posterizeSteps, 2, 32);

        vignetteInnerRadius = Mathf.Clamp01(vignetteInnerRadius);
        vignetteOuterRadius = Mathf.Clamp01(vignetteOuterRadius);
        vignetteSoftness = Mathf.Max(0.0001f, vignetteSoftness);
        vignetteIntensity = Mathf.Max(0f, vignetteIntensity);
        vignetteAxisScale.x = Mathf.Max(0.0001f, vignetteAxisScale.x);
        vignetteAxisScale.y = Mathf.Max(0.0001f, vignetteAxisScale.y);
        vignetteRoundness = Mathf.Clamp01(vignetteRoundness);

        depthThreshold = Mathf.Clamp(depthThreshold, 0.0005f, 0.02f);
        outlineSoftness = Mathf.Clamp(outlineSoftness, 0.0001f, 0.02f);
        outlineThickness = Mathf.Clamp(outlineThickness, 0.5f, 4f);
        silhouetteBackgroundThreshold = Mathf.Clamp(silhouetteBackgroundThreshold, 0.90f, 1f);

        grainScale = Mathf.Clamp(grainScale, 32f, 1024f);
        grainIntensity = Mathf.Clamp01(grainIntensity);
        grainSpeed = Mathf.Clamp(grainSpeed, 0f, 10f);
        grainLuminanceResponse = Mathf.Clamp(grainLuminanceResponse, 0f, 2f);
    }
#endif

    // Public API to control vignette at runtime
    public float GetVignetteIntensity() => vignetteIntensity;

    public void SetVignetteIntensity(float value, bool autoEnable = true)
    {
        vignetteIntensity = Mathf.Max(0f, value);
        if (autoEnable)
            enableVignette = vignetteIntensity > 0.001f;
    }

    public void SetVignetteEnabled(bool enabled) => enableVignette = enabled;
}