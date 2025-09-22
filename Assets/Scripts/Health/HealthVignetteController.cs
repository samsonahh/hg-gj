using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HealthVignetteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Volume cameraEffectsVolume; // Reference to the global volume

    [Header("Intensity Mapping (lower health => more red)")]
    [SerializeField, Range(0f, 2f)] private float intensityAtFullHealth = 0f;
    [SerializeField, Range(0f, 2f)] private float intensityAtZeroHealth = 0.6f;

    [Header("Impulse Effect")]
    [SerializeField, Range(0f, 2f)] private float impulseStrength = 0.4f;
    [SerializeField, Range(0.01f, 2f)] private float impulseDuration = 0.4f;
    [SerializeField, Range(0.01f, 2f)] private float smoothTime = 0.15f;

    [Header("Heal Effect")]
    [SerializeField, Range(0f, 2f)] private float healImpulseStrength = 0.4f;
    [SerializeField, Range(0.01f, 2f)] private float healImpulseDuration = 0.4f;
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private Color damageColor = Color.red;

    private Vignette _vignette;
    private int _lastHealth;
    private float _baseIntensity;
    private float _impulseIntensity;
    private float _impulseVelocity;
    private float _impulseTimer;

    // Smoothing for vignette intensity
    private float _currentIntensity;
    private float _intensityVelocity;

    // Heal impulse
    private float _healImpulseIntensity;
    private float _healImpulseVelocity;
    private float _healImpulseTimer;

    private void Awake()
    {
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) health = player.GetComponentInChildren<Health>();
            if (health == null) health = Object.FindFirstObjectByType<Health>();
        }

        if (cameraEffectsVolume == null)
            cameraEffectsVolume = Object.FindFirstObjectByType<Volume>();

        // Try to get the Vignette effect from the volume profile
        if (cameraEffectsVolume != null && cameraEffectsVolume.profile != null)
            cameraEffectsVolume.profile.TryGet(out _vignette);

        if (health != null)
            _lastHealth = health.CurrentHealth;

        _baseIntensity = ComputeBaseIntensity(_lastHealth, health != null ? health.MaxHealth : _lastHealth);
        _impulseIntensity = 0f;
        _impulseTimer = 0f;
        _healImpulseIntensity = 0f;
        _healImpulseTimer = 0f;
        _currentIntensity = _baseIntensity; // Initialize smoothing
        ApplyVignette(_currentIntensity, damageColor);
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= OnHealthChanged;
    }

    private void Update()
    {
        // Smoothly decay the damage impulse effect
        if (_impulseTimer > 0f)
        {
            _impulseTimer -= Time.deltaTime;
            if (_impulseTimer <= 0f)
            {
                _impulseTimer = 0f;
            }
        }
        _impulseIntensity = Mathf.SmoothDamp(_impulseIntensity, 0f, ref _impulseVelocity, smoothTime);

        // Smoothly decay the heal impulse effect
        if (_healImpulseTimer > 0f)
        {
            _healImpulseTimer -= Time.deltaTime;
            if (_healImpulseTimer <= 0f)
            {
                _healImpulseTimer = 0f;
            }
        }
        _healImpulseIntensity = Mathf.SmoothDamp(_healImpulseIntensity, 0f, ref _healImpulseVelocity, smoothTime);

        // Smoothly interpolate vignette intensity to target
        float targetIntensity = _baseIntensity + _impulseIntensity + _healImpulseIntensity;
        _currentIntensity = Mathf.SmoothDamp(_currentIntensity, targetIntensity, ref _intensityVelocity, smoothTime);

        // Vignette is always red except during heal impulse
        Color vignetteColor = (_healImpulseIntensity > 0.01f) ? healColor : damageColor;

        // Apply the smoothed intensity and color
        ApplyVignette(_currentIntensity, vignetteColor);
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current < _lastHealth)
        {
            // Damage impulse (red)
            _impulseIntensity = impulseStrength;
            _impulseTimer = impulseDuration;
        }
        else if (current > _lastHealth)
        {
            // Heal impulse (green)
            _healImpulseIntensity = healImpulseStrength;
            _healImpulseTimer = healImpulseDuration;
        }

        _lastHealth = current;
        _baseIntensity = ComputeBaseIntensity(current, max);
    }

    private float ComputeBaseIntensity(int current, int max)
    {
        if (max <= 0) return 0f;
        return Mathf.Lerp(intensityAtZeroHealth, intensityAtFullHealth, Mathf.Clamp01((float)current / max));
    }

    private void ApplyVignette(float intensity, Color color)
    {
        if (_vignette == null)
            return;

        _vignette.intensity.value = Mathf.Clamp01(intensity);
        _vignette.intensity.overrideState = true;
        _vignette.color.value = color;
        _vignette.color.overrideState = true;
    }
}