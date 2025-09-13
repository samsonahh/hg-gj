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

    private readonly List<GameObject> spawnedInstances = new();
    private int pendingSpawns = 0;
    private bool isSpawning = false;

    private void Start()
    {
        for (int i = 0; i < maxInstances; i++)
        {
            SpawnNew();
        }
    }

    private void SpawnNew()
    {
        if (prefabToSpawn == null || (spawnPoints != null && spawnPoints.Length == 0))
            return;

        Transform spawnPoint = (spawnPoints != null && spawnPoints.Length > 0)
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        GameObject instance = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        spawnedInstances.Add(instance);

        Health health = instance.GetComponent<Health>();
        if (health != null)
        {
            health.OnDied += () => OnInstanceDied(instance);
        }
        HealthEntity healthEntity = instance.GetComponent<HealthEntity>();
        if (healthEntity != null)
        {
            healthEntity.OnDied += () => OnInstanceDied(instance);
        }
    }

    private void OnInstanceDied(GameObject instance)
    {
        spawnedInstances.Remove(instance);
        pendingSpawns++;
        TrySpawnWithDelay();
    }

    private void TrySpawnWithDelay()
    {
        if (!isSpawning && pendingSpawns > 0 && spawnedInstances.Count < maxInstances)
        {
            StartCoroutine(SpawnWithDelayCoroutine());
        }
    }

    private IEnumerator SpawnWithDelayCoroutine()
    {
        isSpawning = true;
        while (pendingSpawns > 0 && spawnedInstances.Count < maxInstances)
        {
            yield return new WaitForSeconds(spawnDelay);
            SpawnNew();
            pendingSpawns--;
        }
        isSpawning = false;
    }
}