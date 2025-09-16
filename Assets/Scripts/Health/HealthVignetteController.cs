using UnityEngine;
using DG.Tweening;

public class HealthVignetteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private CameraEffects cameraEffects;
    [Tooltip("Force-enable vignette when animating.")]
    [SerializeField] private bool ensureEnabled = true;

    [Header("Intensity Mapping (lower health => more red)")]
    [SerializeField, Range(0f, 2f)] private float intensityAtFullHealth = 0f;
    [SerializeField, Range(0f, 2f)] private float intensityAtZeroHealth = 0.6f;

    [Header("Pulse Settings")]
    [SerializeField] private bool pulseOnDamage = true;
    [SerializeField, Min(0f)] private float pulseOvershoot = 0.15f;
    [SerializeField, Min(0.05f)] private float pulseUpDuration = 0.1f;
    [SerializeField, Min(0.05f)] private float pulseDownDuration = 0.2f;

    [Header("Heal Fade")]
    [SerializeField, Min(0.05f)] private float healFadeDuration = 0.2f;

    private int _lastHealth;
    private Tween _tween;

    private void Awake()
    {
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) health = player.GetComponentInChildren<Health>();
            if (health == null) health = Object.FindFirstObjectByType<Health>();
        }

        if (cameraEffects == null)
        {
            var cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            if (cam != null) cam.TryGetComponent(out cameraEffects);
        }

        if (health != null)
            _lastHealth = health.CurrentHealth;

        // Initialize to current health baseline
        ApplyBaseForHealth(_lastHealth, health != null ? health.MaxHealth : _lastHealth, instant: true);
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += OnHealthChanged; // for damage pulse detection
            health.OnHealed += OnHealed; // for heal fade
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
            health.OnHealed -= OnHealed;
        }

        _tween?.Kill(false);
    }

    // Damage path: only pulse on damage
    private void OnHealthChanged(int current, int max)
    {
        if (cameraEffects == null || max <= 0)
            return;

        bool tookDamage = current < _lastHealth;
        _lastHealth = current;

        if (!tookDamage)
            return; // safety ignore heals here; handled by OnHealed

        float baseTarget = EvaluateBaseIntensity(current, max);

        _tween?.Kill(false);
        if (ensureEnabled) cameraEffects.SetVignetteEnabled(true);

        if (pulseOnDamage)
        {
            float start = cameraEffects.GetVignetteIntensity();
            float peakCap = Mathf.Max(intensityAtFullHealth, intensityAtZeroHealth);
            float peak = Mathf.Min(baseTarget + pulseOvershoot, peakCap);

            var seq = DOTween.Sequence();
            seq.Append(DOVirtual.Float(start, peak, pulseUpDuration, v => cameraEffects.SetVignetteIntensity(v)));
            seq.Append(DOVirtual.Float(peak, baseTarget, pulseDownDuration, v => cameraEffects.SetVignetteIntensity(v)));
            _tween = seq;
        }
        else
        {
            _tween = DOVirtual.Float(cameraEffects.GetVignetteIntensity(), baseTarget, healFadeDuration, v => cameraEffects.SetVignetteIntensity(v));
        }
    }

    // Heal path: fade intensity down toward new base corresponding to increased health
    private void OnHealed(int current, int max)
    {
        if (cameraEffects == null || max <= 0)
            return;

        float baseTarget = EvaluateBaseIntensity(current, max);
        _tween?.Kill(false);
        if (ensureEnabled) cameraEffects.SetVignetteEnabled(true);

        _tween = DOVirtual.Float(cameraEffects.GetVignetteIntensity(), baseTarget, healFadeDuration,
            v => cameraEffects.SetVignetteIntensity(v));
    }

    private void ApplyBaseForHealth(int current, int max, bool instant)
    {
        if (cameraEffects == null || max <= 0)
            return;

        float target = EvaluateBaseIntensity(current, max);
        if (ensureEnabled) cameraEffects.SetVignetteEnabled(true);

        if (instant)
        {
            cameraEffects.SetVignetteIntensity(target);
        }
        else
        {
            _tween?.Kill(false);
            _tween = DOVirtual.Float(cameraEffects.GetVignetteIntensity(), target, healFadeDuration,
                v => cameraEffects.SetVignetteIntensity(v));
        }
    }

    // Maps health fraction to base vignette intensity (lower health => higher intensity).
    private float EvaluateBaseIntensity(int current, int max)
    {
        float healthFrac = Mathf.Clamp01(max > 0 ? (float)current / max : 0f);
        return Mathf.Lerp(intensityAtZeroHealth, intensityAtFullHealth, healthFrac);
    }
}