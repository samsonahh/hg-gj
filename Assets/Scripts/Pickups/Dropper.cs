using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
[RequireComponent(typeof(HealthEntity))]
public class Dropper : MonoBehaviour
{
    [Header("Drop Config")]
    [Tooltip("One of these prefabs will be spawned when this entity dies.")]
    [SerializeField] private List<GameObject> dropPrefabs = new List<GameObject>();

    [Header("Drop Chance")]
    [Tooltip("Percentage chance [0-100] that a drop will spawn on death.")]
    [SerializeField, Range(0, 100)] private int dropChancePercent = 100;

    [Header("Arc Settings (DOTween)")]
    [SerializeField, Min(0f)] private float spawnHeightOffset = 0.1f;
    [SerializeField, Min(0f)] private float minHorizontalDistance = 1.0f;
    [SerializeField, Min(0f)] private float maxHorizontalDistance = 2.5f;
    [SerializeField, Min(0f)] private float jumpPower = 1.5f;   // arc height
    [SerializeField, Min(0.05f)] private float jumpDuration = 0.75f;
    [SerializeField, Min(1)] private int numJumps = 1;
    [SerializeField] private bool addRandomSpin = true;

    private HealthEntity _health;

    private void Awake()
    {
        _health = GetComponent<HealthEntity>();
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (dropPrefabs == null || dropPrefabs.Count == 0)
            return;

        // Roll chance to drop
        if (Random.Range(0, 100) >= dropChancePercent)
            return;

        GameObject prefab = GetRandomValidPrefab();
        if (prefab == null)
            return;

        // Start a little above to avoid ground clipping
        Vector3 startPos = transform.position + Vector3.up * spawnHeightOffset;

        // Pick a random horizontal direction and distance
        Vector2 dir2D = Random.insideUnitCircle.normalized;
        float dist = Random.Range(minHorizontalDistance, Mathf.Max(minHorizontalDistance, maxHorizontalDistance));
        Vector3 endPos = startPos + new Vector3(dir2D.x, 0f, dir2D.y) * dist;

        // Spawn (no parent)
        GameObject drop = Instantiate(prefab, startPos, Quaternion.identity);
        if (drop == null)
            return;

        // Arc using DOTween's DOJump (Rigidbody if available, else Transform)
        var rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
            rb.DOJump(endPos, jumpPower, numJumps, jumpDuration).SetEase(Ease.OutQuad);
        else
            drop.transform.DOJump(endPos, jumpPower, numJumps, jumpDuration).SetEase(Ease.OutQuad);

        // Optional random spin while flying; ensure it ends upright when the SPIN completes
        if (addRandomSpin)
        {
            Vector3 spin = new Vector3(Random.Range(90f, 360f), Random.Range(90f, 360f), Random.Range(90f, 360f));
            drop.transform
                .DORotate(drop.transform.eulerAngles + spin, jumpDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    // Preserve yaw, zero pitch/roll
                    float yaw = drop.transform.eulerAngles.y;
                    Quaternion upright = Quaternion.Euler(0f, yaw, 0f);

                    if (rb != null)
                    {
                        rb.angularVelocity = Vector3.zero;
                        rb.rotation = upright;
                    }
                    else
                    {
                        drop.transform.rotation = upright;
                    }
                });
        }
    }

    private GameObject GetRandomValidPrefab()
    {
        const int maxAttempts = 8;
        for (int i = 0; i < maxAttempts; i++)
        {
            var candidate = dropPrefabs[Random.Range(0, dropPrefabs.Count)];
            if (candidate != null)
                return candidate;
        }

        for (int i = 0; i < dropPrefabs.Count; i++)
        {
            if (dropPrefabs[i] != null)
                return dropPrefabs[i];
        }

        return null;
    }
}