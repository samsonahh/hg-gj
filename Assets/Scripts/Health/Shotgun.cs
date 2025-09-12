using UnityEngine;
using UnityEngine.ProBuilder;
using DG.Tweening;

public class Shotgun : MonoBehaviour
{
    [Header("Shotgun Config")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float spreadAngle = 10f;
    [SerializeField] private int damagePerProjectile = 10;

    [Header("Debug Config")]
    [SerializeField] private bool showGizmo = true;

    [Header("Camera Config")]
    [SerializeField] private Camera playerCamera; // Assign in inspector or at runtime
    [SerializeField] private float maxRange = 100f;

    [Header("Flip Config")]
    [SerializeField] private float defaultFlipAngle = 360f;

    private bool canFire = true;
    private Quaternion originalRotation;

    // Struct to hold trick data
    private struct Trick
    {
        public Vector3 rotation;
        public float duration;
        public Trick(Vector3 rotation, float duration)
        {
            this.rotation = rotation;
            this.duration = duration;
        }
    }

    private void Awake()
    {
        originalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.Shoot += Fire;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.Shoot -= Fire;
    }

    public void Fire()
    {
        if (!canFire)
            return;

        canFire = false;

        Vector3 shootDirection = GetShootDirection();
        FireProjectile(shootDirection, -spreadAngle / 2f);
        FireProjectile(shootDirection, spreadAngle / 2f);

        // Library for tricks
        Trick[] tricks = new Trick[]
        {
            // Casual flips
            new Trick(new Vector3(-360f, 0f, 0f), 0.5f),    // Kickflip (backward flip)
            new Trick(new Vector3(360f, 0f, 0f), 0.5f),     // Heelflip (forward flip)
            new Trick(new Vector3(0f, 360f, 0f), 0.7f),     // 360 Shuvit (horizontal spin)
            new Trick(new Vector3(-180f, 0f, 360f), 0.8f),  // Impossible (half flip + full roll)
            // More complex combos (3 is buggy)
            new Trick(new Vector3(-540f, 180f, 0f), 1.0f),  // Bigspin (1.5x flip + half spin)
            new Trick(new Vector3(-360f, 0f, 720f), 1.2f),  // Double roll + flip
            new Trick(new Vector3(-720f, 360f, 360f), 1.5f),// Double flip + spin + roll
            new Trick(new Vector3(-360f, 360f, 360f), 1.0f),// All axis 360
            new Trick(new Vector3(-1080f, 0f, 0f), 1.8f),   // Triple flip (3x flip)
            new Trick(new Vector3(-360f, 720f, 0f), 1.3f),  // Flip + double spin
        };

        Trick trick = tricks[Random.Range(0, tricks.Length)];
        Vector3 targetEuler = originalRotation.eulerAngles + trick.rotation;

        transform.DOLocalRotate(
            targetEuler,
            trick.duration,
            RotateMode.FastBeyond360
        )
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            transform.localRotation = originalRotation;
            canFire = true;
        });
    }

    private Vector3 GetShootDirection()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            return (hit.point - firePoint.position).normalized;
        }
        else
        {
            // If nothing is hit, shoot straight forward from the camera
            return ray.direction;
        }
    }

    private void FireProjectile(Vector3 baseDirection, float angleOffset)
    {
        Quaternion spread = Quaternion.Euler(0, angleOffset, 0);
        Vector3 directionWithSpread = spread * baseDirection;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(directionWithSpread));
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetDamage(damagePerProjectile);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo || firePoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(firePoint.position, 0.05f);

        Vector3 forward = firePoint.forward;
        float length = 1.5f;

        // Draw center line
        Gizmos.color = Color.green;
        Gizmos.DrawLine(firePoint.position, firePoint.position + forward * length);

        // Draw spread lines
        Gizmos.color = Color.red;
        Quaternion leftRot = Quaternion.Euler(0, -spreadAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, spreadAngle / 2f, 0);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(firePoint.position, firePoint.position + leftDir * length);
        Gizmos.DrawLine(firePoint.position, firePoint.position + rightDir * length);
    }
}