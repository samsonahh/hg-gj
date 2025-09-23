using UnityEngine;
using Animancer;

public class FistState : WeaponState
{
    [Header("Animancer")]
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private AnimationClip walkClip, parryClip, punchClip;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject parryWindowObject;
    [SerializeField] private ReticleUI reticleUI;

    [Header("Punch Settings")]
    [SerializeField] private int fistDamage = 10;
    [SerializeField] private float punchRayDistance = 2.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float punchCooldown = 0.2f;

    [Header("Parry Settings")]
    [SerializeField] private float parryWindow = 0.25f;
    [SerializeField] private float parryCooldown = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debugRaycast = false;

    private bool isParrying = false;
    private float lastParryTime = -Mathf.Infinity;
    private float lastPunchTime = -Mathf.Infinity;

    public override void Enter()
    {
        animancer.Play(walkClip);
        gameObject.SetActive(true);

        // Set the reticle ray distance to match the punch
        if (reticleUI != null)
            reticleUI.RayDistance = punchRayDistance;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.LeftClick += OnShoot;
            InputManager.Instance.RightClick += OnParry;
        }
    }

    public override void Exit()
    {
        animancer.Stop();
        gameObject.SetActive(false);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.LeftClick -= OnShoot;
            InputManager.Instance.RightClick -= OnParry;
        }
    }

    public override void OnShoot()
    {
        animancer.Play(punchClip);

        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float distance = punchRayDistance;
        if (reticleUI != null)
            distance = reticleUI.RayDistance;

        if (debugRaycast)
        {
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 0.5f);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, distance, enemyLayer))
        {
            var health = hit.collider.GetComponent<HealthEntity>();
            if (health != null)
            {
                health.TakeDamage(fistDamage);
            }
        }
    }

    public override void OnParry()
    {
        if (isParrying || Time.time < lastParryTime + parryCooldown)
            return;

        isParrying = true;
        lastParryTime = Time.time;
        animancer.Play(parryClip);

        if (parryWindowObject != null)
            parryWindowObject.SetActive(true);

        Invoke(nameof(EndParry), parryWindow);
    }

    private void EndParry()
    {
        isParrying = false;
        if (parryWindowObject != null)
            parryWindowObject.SetActive(false);

        OnWalk();
    }

    public override void OnReload() { /* Not used for fists */ }

    public override void OnWalk()
    {
        animancer.Play(walkClip);
    }

    public override void OnJump()
    {
        // Optionally play a jump animation if you have one
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugRaycast || playerCamera == null)
            return;

        // Use the same ray as in OnShoot
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float distance = punchRayDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * distance);
        Gizmos.DrawSphere(ray.origin + ray.direction * distance, 0.05f);
    }
#endif
}