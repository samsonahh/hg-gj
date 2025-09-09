using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    [ExecuteAlways]
    [Header("Shader / Material")]
    [SerializeField] private Shader effectShader;
    [SerializeField] private Material materialOverride;

    [Header("Toon / Color")]
    [ColorUsage(false, true)][SerializeField] private Color tint = Color.white;
    [Range(0, 1)][SerializeField] private float tintIntensity = 1f;
    [Range(2, 32)][SerializeField] private int posterizeSteps = 8;

    [Header("Vignette")]
    [SerializeField] private bool enableVignette = true;
    [Range(0, 1)][SerializeField] private float vignetteRadius = 0.65f;
    [Range(0.001f, 0.5f)][SerializeField] private float vignetteSoftness = 0.2f;

    [Header("Outline (Depth-Based)")]
    [SerializeField] private bool enableOutline = true;
    [SerializeField] private bool silhouetteOnly = true; // NEW: outlines only on outer silhouette
    [SerializeField] private Color outlineColor = Color.black;
    [Range(0.5f, 4f)][SerializeField] private float outlineThickness = 1f;
    [Range(0.0005f, 0.02f)][SerializeField] private float depthThreshold = 0.003f;
    [Range(0.0001f, 0.02f)][SerializeField] private float outlineSoftness = 0.002f;
    [Range(0, 3f)][SerializeField] private float outlineStrength = 1f;
    [Range(0.90f, 1f)][SerializeField] private float silhouetteBackgroundThreshold = 0.995f;

    private Material runtimeMaterial;

    private static readonly Dictionary<Camera, CameraEffects> registry = new();
    public static bool TryGet(Camera cam, out CameraEffects fx) => registry.TryGetValue(cam, out fx);

    public bool HasVisibleChange =>
        tintIntensity > 0.0001f ||
        posterizeSteps != 8 ||
        (enableVignette && (vignetteRadius != 0.65f || Mathf.Abs(vignetteSoftness - 0.2f) > 0.0001f)) ||
        tint != Color.white ||
        (enableOutline && outlineStrength > 0.01f);

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

        m.SetColor("_Tint", tint);
        m.SetFloat("_Intensity", tintIntensity);
        m.SetInt("_PosterizeSteps", posterizeSteps);

        m.SetFloat("_VignetteRadius", vignetteRadius);
        m.SetFloat("_VignetteSoftness", vignetteSoftness);

        m.SetColor("_OutlineColor", outlineColor);
        m.SetFloat("_OutlineThickness", outlineThickness);
        m.SetFloat("_DepthThreshold", depthThreshold);
        m.SetFloat("_OutlineSoftness", outlineSoftness);
        m.SetFloat("_OutlineStrength", outlineStrength);
        m.SetFloat("_SilhouetteBackgroundThreshold", silhouetteBackgroundThreshold);

        SetKeyword(m, "VIGNETTE_ENABLED", enableVignette);
        bool outlineActive = enableOutline && outlineStrength > 0.01f;
        SetKeyword(m, "OUTLINE_ENABLED", outlineActive);
        SetKeyword(m, "OUTLINE_SILHOUETTE", outlineActive && silhouetteOnly);
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
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        posterizeSteps = Mathf.Max(2, posterizeSteps);
        vignetteSoftness = Mathf.Max(0.0001f, vignetteSoftness);
        depthThreshold = Mathf.Clamp(depthThreshold, 0.0005f, 0.02f);
        outlineSoftness = Mathf.Clamp(outlineSoftness, 0.0001f, 0.02f);
        outlineThickness = Mathf.Clamp(outlineThickness, 0.5f, 4f);
        silhouetteBackgroundThreshold = Mathf.Clamp(silhouetteBackgroundThreshold, 0.90f, 1f);
    }
#endif
}