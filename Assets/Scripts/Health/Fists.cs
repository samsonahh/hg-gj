using UnityEngine;
using DG.Tweening;

public class Fists : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leftFistMesh;
    [SerializeField] private Transform rightFistMesh;
    [SerializeField] private Camera playerCamera;

    [Header("Animation")]
    [SerializeField] private float squishAmount = 0.7f;
    [SerializeField] private float squishDuration = 0.08f;
    [SerializeField] private float restoreDuration = 0.12f;
    [SerializeField] private Ease squishEase = Ease.OutQuad;
    [SerializeField] private Ease restoreEase = Ease.InOutQuad;

    [Header("Punch Motion")]
    [SerializeField] private float punchDistance = 0.4f;
    [SerializeField] private float punchDuration = 0.12f;
    [SerializeField] private Ease punchEase = Ease.OutCubic;

    [Header("Damage")]
    [SerializeField] private int fistDamage = 10;
    [SerializeField] private float damageCooldown = 0.15f;

    [Header("Projectile Layer")]
    [SerializeField] private LayerMask projectileLayer;

    [Header("Parry")]
    [SerializeField] private float parryWindow = 0.25f;
    [SerializeField] private float parryCooldown = 1.0f; 
    [SerializeField] private GameObject parryWindowObject;

    private Vector3 leftFistOriginalScale;
    private Vector3 rightFistOriginalScale;
    private Vector3 leftFistOriginalPos;
    private Vector3 rightFistOriginalPos;
    private Tween leftFistTween;
    private Tween rightFistTween;
    private Tween leftFistPunchTween;
    private Tween rightFistPunchTween;

    private bool leftFistCanDamage = false;
    private bool rightFistCanDamage = false;
    private bool isParrying = false;
    private bool punchLeftNext = true;
    private float lastParryTime = -Mathf.Infinity;

    private void Awake()
    {
        if (leftFistMesh != null)
        {
            leftFistOriginalScale = leftFistMesh.localScale;
            leftFistOriginalPos = leftFistMesh.localPosition;
            EnsureCollider(leftFistMesh, "LeftFist");
        }
        if (rightFistMesh != null)
        {
            rightFistOriginalScale = rightFistMesh.localScale;
            rightFistOriginalPos = rightFistMesh.localPosition;
            EnsureCollider(rightFistMesh, "RightFist");
        }
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.LeftClick += PunchAlternateFist;
            InputManager.Instance.RightClick += StartParry;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.LeftClick -= PunchAlternateFist;
            InputManager.Instance.RightClick -= StartParry;
        }
    }

    private void EnsureCollider(Transform fistMesh, string fistName)
    {
        var collider = fistMesh.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = fistMesh.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }

        // Add a FistHitbox helper for collision callbacks
        var hitbox = fistMesh.GetComponent<FistHitbox>();
        if (hitbox == null)
        {
            hitbox = fistMesh.gameObject.AddComponent<FistHitbox>();
            hitbox.fists = this;
            hitbox.isLeft = fistName == "LeftFist";
        }
    }

    private void PunchAlternateFist()
    {
        if (punchLeftNext)
            PunchFist(leftFistMesh, ref leftFistTween, ref leftFistPunchTween, leftFistOriginalScale, leftFistOriginalPos, true);
        else
            PunchFist(rightFistMesh, ref rightFistTween, ref rightFistPunchTween, rightFistOriginalScale, rightFistOriginalPos, false);

        punchLeftNext = !punchLeftNext;
    }

    private void PunchFist(Transform fistMesh, ref Tween scaleTween, ref Tween punchTween, Vector3 originalScale, Vector3 originalPos, bool isLeft)
    {
        if (fistMesh == null || playerCamera == null) return;

        scaleTween?.Kill();
        punchTween?.Kill();
        fistMesh.localScale = originalScale;
        fistMesh.localPosition = originalPos;

        // Squish the fist mesh
        scaleTween = fistMesh.DOScale(
                new Vector3(originalScale.x * squishAmount, originalScale.y * (2 - squishAmount), originalScale.z * squishAmount),
                squishDuration
            )
            .SetEase(squishEase)
            .OnComplete(() =>
                fistMesh.DOScale(originalScale, restoreDuration).SetEase(restoreEase)
            );

        // Move the fist mesh toward the center of the camera
        Vector3 worldTarget = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, playerCamera.nearClipPlane + punchDistance));
        Vector3 localTarget = fistMesh.parent != null
            ? fistMesh.parent.InverseTransformPoint(worldTarget)
            : worldTarget;

        if (isLeft) leftFistCanDamage = true;
        else rightFistCanDamage = true;

        punchTween = fistMesh.DOLocalMove(localTarget, punchDuration)
            .SetEase(punchEase)
            .OnComplete(() =>
            {
                fistMesh.DOLocalMove(originalPos, restoreDuration).SetEase(restoreEase);
                if (isLeft) Invoke(nameof(ResetLeftFistDamage), damageCooldown);
                else Invoke(nameof(ResetRightFistDamage), damageCooldown);
            });
    }

    private void ResetLeftFistDamage() => leftFistCanDamage = false;
    private void ResetRightFistDamage() => rightFistCanDamage = false;

    private void StartParry()
    {
        // Cooldown check
        if (isParrying || Time.time < lastParryTime + parryCooldown)
            return;

        isParrying = true;
        lastParryTime = Time.time;
        if (parryWindowObject != null)
            parryWindowObject.SetActive(true);
        Invoke(nameof(EndParry), parryWindow);
        // sound or vfx goes here
    }

    private void EndParry()
    {
        isParrying = false;
        if (parryWindowObject != null)
            parryWindowObject.SetActive(false);
    }

    public void TryDealDamage(Collider other, bool isLeft)
    {
        if (((1 << other.gameObject.layer) & projectileLayer.value) != 0)
        {
            // Only deflect if parrying
            if (isParrying)
            {
                Rigidbody rb = other.attachedRigidbody;
                if (rb != null)
                {
                    Vector3[] directions = {
                        -transform.right, // left
                        transform.up,     // up
                        transform.right   // right
                    };
                    Vector3 deflectDir = directions[Random.Range(0, directions.Length)].normalized;
                    Vector3 force = (deflectDir + transform.forward * 0.5f).normalized * 15f;
                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(force, ForceMode.VelocityChange);
                }
            }
            return;
        }

        if ((isLeft && !leftFistCanDamage) || (!isLeft && !rightFistCanDamage))
            return;

        HealthEntity health = other.GetComponent<HealthEntity>();
        if (health != null)
        {
            health.TakeDamage(fistDamage);
            if (isLeft) leftFistCanDamage = false;
            else rightFistCanDamage = false;
        }
    }
}

public class FistHitbox : MonoBehaviour
{
    [HideInInspector] public Fists fists;
    [HideInInspector] public bool isLeft;

    private void OnTriggerEnter(Collider other)
    {
        fists?.TryDealDamage(other, isLeft);
    }
}