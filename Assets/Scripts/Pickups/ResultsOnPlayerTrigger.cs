using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class ResultsOnPlayerTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    [Header("Pickup Animation")]
    [SerializeField] private Transform targetTransform; // The transform to animate
    [SerializeField] private float floatDistance = 0.3f;
    [SerializeField] private float floatDuration = 1.2f;
    [SerializeField] private float spinSpeed = 90f; // degrees per second

    private bool _hasTriggered;
    private Tween _floatTween;
    private Tween _spinTween;

    private void Start()
    {
        // Use assigned targetTransform or fallback to this.transform
        var t = targetTransform != null ? targetTransform : transform;

        // Floating animation (Y axis up and down)
        Vector3 startPos = t.localPosition;
        _floatTween = t.DOLocalMoveY(startPos.y + floatDistance, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // Spinning animation (Y axis)
        _spinTween = t.DOLocalRotate(
            new Vector3(0, 360, 0),
            360f / spinSpeed,
            RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    private void OnDestroy()
    {
        _floatTween?.Kill();
        _spinTween?.Kill();
    }

    private void Reset()
    {
        // Ensure this collider acts as a trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered && triggerOnce) return;
        if (other == null || !other.CompareTag(playerTag)) return;

        if (!GameManager.IsLoaded)
        {
            Debug.LogWarning("[ResultsOnPlayerTrigger] GameManager is not loaded; cannot change state.");
            return;
        }

        GameManager.Instance.ChangeState(GameState.Results);
        _hasTriggered = true;
        Destroy(gameObject);
    }
}