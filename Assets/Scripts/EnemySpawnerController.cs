using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerController : MonoBehaviour
{
    [Header("Trigger Zone Settings")]
    [SerializeField] private Transform _triggerZone;
    [SerializeField] private float _triggerZoneRadius;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Spawn Object Settings")]
    [SerializeField] private GameObject _spawnObjectPrefab;
    private List<GameObject> listOfEnemies = new();

    private bool _hasPlayerEntered = false;
    private bool _hasEnemySpawned = false;

    public event Action OnEnemyWaveComplete = delegate { };

    


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
            GameObject enemyObj = Instantiate(_spawnObjectPrefab, GenerateSpawnCoordinates(), Quaternion.identity);
            listOfEnemies.Add(enemyObj);
        }

    }

    bool CheckIfPlayerEntered() => Physics.CheckSphere(transform.position, _triggerZoneRadius, _playerLayer);

    Vector3 GenerateSpawnCoordinates()
    {
        const float ABOVE_THE_FLOOR = 1f;

        Vector3 randomPos = UnityEngine.Random.insideUnitSphere * _triggerZoneRadius;
        Vector3 spawnPoint = new Vector3(transform.position.x + randomPos.x, ABOVE_THE_FLOOR, transform.position.z + randomPos.z);
        return spawnPoint;
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_triggerZone.position, _triggerZoneRadius);
    }
}
