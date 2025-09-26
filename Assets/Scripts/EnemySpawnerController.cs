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

    [Header("Game Settings")]
    [SerializeField] private float _maxEnemiesOnFieldAtATime;
    [SerializeField] private float _totalNumberOfEnemies;
    [SerializeField] private float _killCounter = 0;
    private float _currentEnemiesOnField = 0;
    private float _intitalSpawnCounter = 0;
    private bool _hasPlayerEntered = false;

    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            Debug.LogWarning("You Win");
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

        Vector3 spawnCoordinates = GenerateSpawnCoordinates();

        Ray ray = new Ray(spawnCoordinates, Vector3.down);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            //if (hit.collider != null)
            //{
            //    Debug.LogWarning(hit.collider.gameObject.name);
            //}
            NavMeshHit navHit;

            // 2 is suppose to what I assume is the walkable area index
            if (NavMesh.SamplePosition(hit.transform.position, out navHit, 1.0f, 1))
            {
                Debug.LogWarning("Valid Spawn");
                
            }
            else
            {
                Debug.LogWarning("Invalid Spawn");
            }
        }


        GameObject enemyObj = Instantiate(_spawnObjectPrefab, spawnCoordinates, Quaternion.identity);
        enemyObj.GetComponent<HealthEntity>().OnDied += HandleDeathLogic;
        _currentEnemiesOnField++;
    }

    void IsSpawnValid()
    {
        Vector3 spawnCoordinates = GenerateSpawnCoordinates();
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
