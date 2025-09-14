using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public abstract class PickupBase : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private string playerTag = "Player"; // Tag for player objects

    [Header("Pickup Visuals")]
    [SerializeField] private Transform targetMesh; // Assign the mesh
    [SerializeField] private float bobHeight = 0.25f;
    [SerializeField] private float bobDuration = 1.2f;
    [SerializeField] private float spinSpeed = 90f; // degrees per second

    private Tween bobTween;
    private Tween spinTween;

    protected virtual void Reset()
    {
        // collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    protected virtual void Start()
    {
        // If not set, try to use the first child as the mesh
        if (targetMesh == null && transform.childCount > 0)
            targetMesh = transform.GetChild(0);

        if (targetMesh != null)
        {
            // Bobbing (Y axis)
            bobTween = targetMesh.DOLocalMoveY(targetMesh.localPosition.y + bobHeight, bobDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            // Spinning (Y axis)
            spinTween = targetMesh.DOLocalRotate(new Vector3(0, 360, 0), 360f / spinSpeed, RotateMode.LocalAxisAdd)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
        }
    }

    protected virtual void OnDestroy()
    {
        // Clean up tweens
        if (bobTween != null) bobTween.Kill();
        if (spinTween != null) spinTween.Kill();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only allow pickup if collider has the player tag
        if (!other.CompareTag(playerTag))
            return;

        if (CanPickup(other))
        {
            OnPickup(other);
            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }

    // Override this to check if the collider is a valid target
    protected abstract bool CanPickup(Collider other);

    // Override this to define pickup behavior
    protected abstract void OnPickup(Collider other);
}