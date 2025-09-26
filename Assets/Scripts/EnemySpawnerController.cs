using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnerController : MonoBehaviour
{
    [Header("Trigger Zone Settings")]
    [SerializeField] private Transform _triggerZone;
    [SerializeField] private float _triggerZoneSize;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Spawn Object Settings")]
    [SerializeField] private GameObject _spawnObjectPrefab;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Game Settings")]
    [SerializeField] private float _maxEnemiesOnFieldAtATime;
    [SerializeField] private float _totalNumberOfEnemies;

    [Header("Win Settings")]
    [SerializeField] GameObject _entranceObjectPrefab;

    private float _killCounter = 0;
    private float _currentEnemiesOnField = 0;
    private float _intitalSpawnCounter = 0;
    private bool _hasPlayerEntered = false;

    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _entranceObjectPrefab.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(CheckIfPlayerEntered() && !_hasPlayerEntered)
        {
            IntialSpawnEnemies();
            _hasPlayerEntered = true;
        }

        if (_currentEnemiesOnField >= _maxEnemiesOnFieldAtATime) CancelInvoke("SpawnEnemy");

        if (_killCounter >= _totalNumberOfEnemies)
        {
            CancelInvoke("SpawnEnemy");
            SetEntranceObjectDeactive();
        }        
    }

    public void HandleDeathLogic()
    {
        _killCounter++;
        _currentEnemiesOnField--;

        if (_killCounter < _totalNumberOfEnemies) Invoke("SpawnEnemy", _spawnDelay); 
    }

    void IntialSpawnEnemies()
    {
        float INTIAL_SPAWN_TIME = 0f;
        InvokeRepeating("SpawnEnemy", INTIAL_SPAWN_TIME, _spawnDelay);
    }

    void SpawnEnemy()
    {
        if (_intitalSpawnCounter != _maxEnemiesOnFieldAtATime) _intitalSpawnCounter++;

        float rayDist = 10f;
        Vector3 spawnCoordinates = GenerateSpawnCoordinates();
        RaycastHit hit;

        if(Physics.Raycast(spawnCoordinates, Vector3.down, out hit, rayDist, _groundLayer))
        {
            NavMeshHit navHit;

            if(NavMesh.SamplePosition(hit.collider.transform.position, out navHit, rayDist, NavMesh.AllAreas))
            {
                int walkableAreaID = NavMesh.GetAreaFromName("Walkable");
                int areaMask = navHit.mask;

                if (CheckIfObjectIsInWalkableArea(navHit))
                {
                    Debug.LogError("This area is good to walk");
                } else
                {
                    Debug.LogError("This area is not good to walk");
                }

            } 
        }



        GameObject enemyObj = Instantiate(_spawnObjectPrefab, spawnCoordinates, Quaternion.identity);
        enemyObj.GetComponent<HealthEntity>().OnDied += HandleDeathLogic;
        _currentEnemiesOnField++;
    }

    void SetEntranceObjectDeactive() => _entranceObjectPrefab.SetActive(false);


    bool CheckIfObjectIsInWalkableArea(NavMeshHit navHit)
    {
        int walkableAreaID = NavMesh.GetAreaFromName("Walkable");
        int areaMask = navHit.mask;

        return (areaMask & (1 << walkableAreaID)) != 0;
    }

    bool CheckIfPlayerEntered() => Physics.CheckSphere(transform.position, _triggerZoneSize, _playerLayer);

    Vector3 GenerateSpawnCoordinates()
    {
        const float ABOVE_THE_FLOOR = 1f;

        Vector3 randomPos = Random.insideUnitSphere * _triggerZoneSize;
        Vector3 spawnPoint = new Vector3(transform.position.x + randomPos.x, ABOVE_THE_FLOOR, transform.position.z + randomPos.z);
        return spawnPoint;
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_triggerZone.position, _triggerZoneSize);
    }
}
