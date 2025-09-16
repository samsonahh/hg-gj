using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawner Config")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField, Min(1)] private int maxInstances = 5;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField, Min(0f)] private float spawnDelay = 1f;
    [SerializeField, Min(0f)] private float minSeparation = 0.75f;

    private readonly List<GameObject> spawnedInstances = new();
    private int pendingSpawns = 0;
    private bool isSpawning = false;

    private void Start()
    {
        // Queue initial spawns
        if (prefabToSpawn == null)
            return;

        pendingSpawns = Mathf.Max(0, maxInstances);
        TrySpawnWithDelay();
    }

    private void OnDisable()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    private void OnInstanceDied(GameObject instance)
    {
        if (instance != null)
            spawnedInstances.Remove(instance);

        if (spawnedInstances.Count < maxInstances)
            pendingSpawns++;

        TrySpawnWithDelay();
    }

    private void TrySpawnWithDelay()
    {
        if (isSpawning)
            return;

        if (pendingSpawns <= 0)
            return;

        if (spawnedInstances.Count >= maxInstances)
            return;

        StartCoroutine(SpawnWithDelayCoroutine());
    }

    private IEnumerator SpawnWithDelayCoroutine()
    {
        isSpawning = true;

        while (pendingSpawns > 0 && spawnedInstances.Count < maxInstances)
        {
            yield return new WaitForSeconds(spawnDelay);

            if (SpawnNew())
                pendingSpawns--;
            else
                yield return null; // wait a frame and try again
        }

        isSpawning = false;
    }

    private bool SpawnNew()
    {
        if (prefabToSpawn == null)
            return false;

        // Resolve spawn point
        Transform spawnPoint = GetFreeSpawnPoint();
        if (spawnPoint == null)
            return false;

        GameObject instance = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        if (instance == null)
            return false;

        spawnedInstances.Add(instance);

        // Hook death events if available
        var health = instance.GetComponent<Health>();
        if (health != null)
            health.OnDied += () => OnInstanceDied(instance);

        var healthEntity = instance.GetComponent<HealthEntity>();
        if (healthEntity != null)
            healthEntity.OnDied += () => OnInstanceDied(instance);

        return true;
    }

    private Transform GetFreeSpawnPoint()
    {
        // Fallback to self if no points set
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // Ensure separation at this transform too
            if (IsPositionClear(transform.position))
                return transform;
            return null;
        }

        // Try a few random points; early-continue on busy spots
        const int maxAttempts = 8;
        for (int i = 0; i < maxAttempts; i++)
        {
            Transform candidate = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (candidate == null)
                continue;

            if (!IsPositionClear(candidate.position))
                continue;

            return candidate;
        }

        return null;
    }

    private bool IsPositionClear(Vector3 position)
    {
        if (spawnedInstances.Count == 0)
            return true;

        float minSepSqr = minSeparation * minSeparation;
        for (int i = 0; i < spawnedInstances.Count; i++)
        {
            GameObject inst = spawnedInstances[i];
            if (inst == null)
                continue;

            if ((inst.transform.position - position).sqrMagnitude < minSepSqr)
                return false;
        }
        return true;
    }
}