using UnityEngine;
using DG.Tweening;

public class Fists : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leftFistMesh;
    [SerializeField] private Transform rightFistMesh;
    [SerializeField] private Camera playerCamera; // Assign the main camera in the inspector or at runtime

    [Header("Animation")]
    [SerializeField] private float squishAmount = 0.7f;
    [SerializeField] private float squishDuration = 0.08f;
    [SerializeField] private float restoreDuration = 0.12f;
    [SerializeField] private Ease squishEase = Ease.OutQuad;
    [SerializeField] private Ease restoreEase = Ease.InOutQuad;

    [Header("Punch Motion")]
    [SerializeField] private float punchDistance = 0.4f; // How far the fist moves toward the camera center
    [SerializeField] private float punchDuration = 0.12f;
    [SerializeField] private Ease punchEase = Ease.OutCubic;

    [Header("Damage")]
    [SerializeField] private int fistDamage = 10;
    [SerializeField] private float damageCooldown = 0.15f; // Prevents multiple hits per punch

    [Header("Projectile Layer")]
    [SerializeField] private LayerMask projectileLayer; // Assign the Projectile layer in the inspector

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
            InputManager.Instance.LeftClick += PunchLeftFist;
            InputManager.Instance.RightClick += PunchRightFist;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.LeftClick -= PunchLeftFist;
            InputManager.Instance.RightClick -= PunchRightFist;
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

    private void PunchLeftFist()
    {
        if (leftFistMesh == null || playerCamera == null) return;

        leftFistTween?.Kill();
        leftFistPunchTween?.Kill();
        leftFistMesh.localScale = leftFistOriginalScale;
        leftFistMesh.localPosition = leftFistOriginalPos;

        // Squish
        leftFistTween = leftFistMesh.DOScale(
                new Vector3(leftFistOriginalScale.x * squishAmount, leftFistOriginalScale.y * (2 - squishAmount), leftFistOriginalScale.z * squishAmount),
                squishDuration
            )
            .SetEase(squishEase)
            .OnComplete(() =>
                leftFistMesh.DOScale(leftFistOriginalScale, restoreDuration).SetEase(restoreEase)
            );

        // Move the left fist mesh toward the center of the camera
        Vector3 worldTarget = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, playerCamera.nearClipPlane + punchDistance));
        Vector3 localTarget = leftFistMesh.parent != null
            ? leftFistMesh.parent.InverseTransformPoint(worldTarget)
            : worldTarget;

        leftFistCanDamage = true;
        leftFistPunchTween = leftFistMesh.DOLocalMove(localTarget, punchDuration)
            .SetEase(punchEase)
            .OnComplete(() =>
            {
                leftFistMesh.DOLocalMove(leftFistOriginalPos, restoreDuration).SetEase(restoreEase);
                Invoke(nameof(ResetLeftFistDamage), damageCooldown);
            });
    }

    private void PunchRightFist()
    {
        if (rightFistMesh == null || playerCamera == null) return;

        rightFistTween?.Kill();
        rightFistPunchTween?.Kill();
        rightFistMesh.localScale = rightFistOriginalScale;
        rightFistMesh.localPosition = rightFistOriginalPos;

        // Squish
        rightFistTween = rightFistMesh.DOScale(
                new Vector3(rightFistOriginalScale.x * squishAmount, rightFistOriginalScale.y * (2 - squishAmount), rightFistOriginalScale.z * squishAmount),
                squishDuration
            )
            .SetEase(squishEase)
            .OnComplete(() =>
                rightFistMesh.DOScale(rightFistOriginalScale, restoreDuration).SetEase(restoreEase)
            );

        // Move the right fist mesh toward the center of the camera
        Vector3 worldTarget = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, playerCamera.nearClipPlane + punchDistance));
        Vector3 localTarget = rightFistMesh.parent != null
            ? rightFistMesh.parent.InverseTransformPoint(worldTarget)
            : worldTarget;

        rightFistCanDamage = true;
        rightFistPunchTween = rightFistMesh.DOLocalMove(localTarget, punchDuration)
            .SetEase(punchEase)
            .OnComplete(() =>
            {
                rightFistMesh.DOLocalMove(rightFistOriginalPos, restoreDuration).SetEase(restoreEase);
                Invoke(nameof(ResetRightFistDamage), damageCooldown);
            });
    }

    private void ResetLeftFistDamage() => leftFistCanDamage = false;
    private void ResetRightFistDamage() => rightFistCanDamage = false;

    // Called by FistHitbox
    public void TryDealDamage(Collider other, bool isLeft)
    {
        // Destroy projectile if in projectile layer
        if (((1 << other.gameObject.layer) & projectileLayer.value) != 0)
        {
            Destroy(other.gameObject);
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