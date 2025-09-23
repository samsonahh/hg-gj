using UnityEngine;
using Animancer;
using System;

[System.Serializable]
public class ShotgunState : WeaponState
{
    [Header("Animancer")]
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private AnimationClip walkClip, reload1Clip, reload2Clip, jumpClip, shootClip;

    [Header("Shotgun Config")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float spreadAngle = 10f;
    [SerializeField] private int damagePerProjectile = 10;
    [SerializeField] private int maxAmmo = 8;
    [SerializeField] private int currentAmmo = 8;
    [SerializeField] private bool infiniteAmmo = false;

    [Header("Camera Config")]
    [SerializeField] private Camera playerCamera;

    [Header("Reticle & Raycast")]
    [SerializeField] private ReticleUI reticleUI;
    [SerializeField] private float shootRayDistance = 10f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation Timing")]
    [SerializeField, Min(0.01f)] private float reloadDuration = 0.7f;
    [SerializeField, Min(0.01f)] private float shootDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugRaycast = false;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool InfiniteAmmo
    {
        get => infiniteAmmo;
        set
        {
            if (infiniteAmmo != value)
            {
                infiniteAmmo = value;
                OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            }
        }
    }
    public event Action<int, int> OnAmmoChanged = delegate { };

    private bool canFire = true;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalRotation = transform.localRotation;
    }

    public override void Enter()
    {
        animancer.Play(walkClip);
        gameObject.SetActive(true);

        // Set the reticle ray distance to match the shotgun
        if (reticleUI != null)
            reticleUI.RayDistance = shootRayDistance;

        canFire = true;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Shoot += OnShoot;
            InputManager.Instance.Jump += OnJump;
        }
    }

    public override void Exit()
    {
        animancer.Stop();
        gameObject.SetActive(false);
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Shoot -= OnShoot;
            InputManager.Instance.Jump -= OnJump;
        }
    }

    public override void OnShoot()
    {
        if (!canFire || (!infiniteAmmo && currentAmmo <= 0))
            return;

        canFire = false;

        if (!infiniteAmmo)
        {
            currentAmmo--;
            OnAmmoChanged.Invoke(currentAmmo, maxAmmo);
        }
        else
        {
            OnAmmoChanged.Invoke(currentAmmo, maxAmmo);
        }

        var state = animancer.Play(shootClip);

        // Set shoot animation duration
        if (shootClip != null && state != null)
        {
            state.Duration = shootDuration;
        }

        if (state.Events(this, out var events))
        {
            events.Clear();
            events.Add(0.6f, () =>
            {
                Vector3 shootDirection = GetShootDirection();
                FireProjectile(shootDirection, 0f);

                // Example: Raycast for hit detection (center of camera, enemyLayer, shootRayDistance)
                if (playerCamera != null)
                {
                    Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    float distance = shootRayDistance;
                    if (reticleUI != null)
                        distance = reticleUI.RayDistance;

                    if (Physics.Raycast(ray, out RaycastHit hit, distance, enemyLayer))
                    {
                        var health = hit.collider.GetComponent<HealthEntity>();
                        if (health != null)
                        {
                            health.TakeDamage(damagePerProjectile);
                        }
                    }
                }
            });
            events.OnEnd = OnReload;
        }
    }

    public override void OnReload()
    {
        // Randomly pick a reload animation
        AnimationClip reloadClip = UnityEngine.Random.value < 0.5f ? reload1Clip : reload2Clip;
        var reloadState = animancer.Play(reloadClip);

        // Set speed so the animation finishes in reloadDuration seconds
        if (reloadClip != null && reloadState != null)
        {
            reloadState.Speed = reloadClip.length / reloadDuration;
        }

        if (reloadState.Events(this, out var reloadEvents))
        {
            reloadEvents.Clear();
            // The event fires at the end of the reload animation (normalized time 0.999f to avoid looping issues)
            reloadEvents.Add(0.999f, OnReloadAnimationEnd);
        }

        currentAmmo = maxAmmo;
    }

    private void OnReloadAnimationEnd()
    {
        canFire = true;
        animancer.Play(walkClip);
    }

    public override void OnParry() { }

    public override void OnJump()
    {
        animancer.Play(jumpClip);
    }

    public override void OnWalk()
    {
        animancer.Play(walkClip);
    }

    private Vector3 GetShootDirection()
    {
        if (playerCamera == null || firePoint == null)
            return transform.forward;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, shootRayDistance, enemyLayer))
        {
            return (hit.point - firePoint.position).normalized;
        }
        else
        {
            return ray.direction;
        }
    }

    private void FireProjectile(Vector3 baseDirection, float angleOffset)
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        Quaternion spread = Quaternion.Euler(0, angleOffset, 0);
        Vector3 directionWithSpread = spread * baseDirection;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(directionWithSpread));
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetDamage(damagePerProjectile);
        }
    }

    public void Reload()
    {
        OnReload();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }
#endif
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugRaycast || playerCamera == null)
            return;

        // Use the same ray as in GetShootDirection
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float distance = shootRayDistance;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * distance);
        Gizmos.DrawSphere(ray.origin + ray.direction * distance, 0.05f);
    }
#endif
}