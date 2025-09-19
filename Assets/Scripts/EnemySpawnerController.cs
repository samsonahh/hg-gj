using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerController : MonoBehaviour
{
    [Header("Trigger Zone Settings")]
    [SerializeField] private Transform _triggerZone;
    [SerializeField] private float _triggerZoneRadius;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Spawn Object Settings")]
    [SerializeField] private Transform _spawner;
    [SerializeField] private float _spawnerRange;
    [SerializeField] private GameObject _spawnObjectPrefab;
    private List<GameObject> listOfEnemies = new();

    private bool _hasPlayerEntered = false;
    private bool _hasEnemySpawned = false;

    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(CheckIfPlayerEntered() && !_hasPlayerEntered)
        {
            IntialSpawnEnemy();
            _hasPlayerEntered = true;
        }
    }

    void IntialSpawnEnemy()
    {
        _hasEnemySpawned = true;

        int num = 5;

        while (num-- > 0)
        {
            const float ABOVE_THE_FLOOR = 1f;

            Vector3 spawnPos = _spawner.position;
            spawnPos.x = Random.Range(-_spawnerRange - spawnPos.x, _spawnerRange + spawnPos.x);
            spawnPos.z = Random.Range(-_spawnerRange + spawnPos.z, _spawnerRange + spawnPos.z);
            spawnPos.y = ABOVE_THE_FLOOR;

            GameObject enemyObj = Instantiate(_spawnObjectPrefab, spawnPos, Quaternion.identity);
            listOfEnemies.Add(enemyObj);
        }

    }

    bool CheckIfPlayerEntered() => Physics.CheckSphere(transform.position, _triggerZoneRadius, _playerLayer);
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_triggerZone.position, _triggerZoneRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_spawner.position, 1f);
    }
}
