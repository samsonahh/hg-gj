using UnityEngine;
using DG.Tweening;
using UnityEngine.ProBuilder;
using System;

public class Shotgun : MonoBehaviour
{
    [Header("Shotgun Config")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float spreadAngle = 10f;
    [SerializeField] private int damagePerProjectile = 10;
    [SerializeField] private int maxAmmo = 8;
    [SerializeField] private int currentAmmo = 8;
    [SerializeField] private bool infiniteAmmo = false;

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

    [Header("Debug Config")]
    [SerializeField] private bool showGizmo = true;

    [Header("Camera Config")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxRange = 100f;

    [Header("Flip Config")]
    [SerializeField] private float defaultFlipAngle = 360f;
    [SerializeField] private float trickDuration = 1.0f;

    private bool canFire = true;
    private Quaternion originalRotation;

    private struct Trick
    {
        public Vector3 rotation;
        public Trick(Vector3 rotation)
        {
            this.rotation = rotation;
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

        Vector3 shootDirection = GetShootDirection();
        FireProjectile(shootDirection, 0f);

        Trick[] tricks = new Trick[]
        {
            new Trick(new Vector3(-360f, 0f, 0f)),
            new Trick(new Vector3(360f, 0f, 0f)),
            new Trick(new Vector3(0f, 360f, 0f)),
            new Trick(new Vector3(-180f, 0f, 360f)),
            new Trick(new Vector3(-540f, 180f, 0f)),
            new Trick(new Vector3(-360f, 0f, 720f)),
            new Trick(new Vector3(-720f, 360f, 360f)),
            new Trick(new Vector3(-360f, 360f, 360f)),
            new Trick(new Vector3(-1080f, 0f, 0f)),
            new Trick(new Vector3(-360f, 720f, 0f)),
        };

        Trick trick = tricks[UnityEngine.Random.Range(0, tricks.Length)];
        Vector3 targetEuler = originalRotation.eulerAngles + trick.rotation;

        transform.DOLocalRotate(
            targetEuler,
            trickDuration,
            RotateMode.FastBeyond360
        )
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            transform.DOLocalRotateQuaternion(originalRotation, 0.3f)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() => canFire = true);
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

        Gizmos.color = Color.green;
        Gizmos.DrawLine(firePoint.position, firePoint.position + forward * length);

        Gizmos.color = Color.red;
        Quaternion leftRot = Quaternion.Euler(0, -spreadAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, spreadAngle / 2f, 0);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(firePoint.position, firePoint.position + leftDir * length);
        Gizmos.DrawLine(firePoint.position, firePoint.position + rightDir * length);
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        OnAmmoChanged.Invoke(currentAmmo, maxAmmo);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }
#endif
}