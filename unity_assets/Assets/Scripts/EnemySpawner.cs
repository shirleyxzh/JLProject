using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(PlayerSpawner))]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject EnemyPrefab;

    [SerializeField]
    private int TotolToSpawn = 3;

    [SerializeField]
    ParticleSystem explosionParticleSystem;

    [SerializeField]
    private GridMgr gridMgr;

    [SerializeField]
    private Transform[] spawnPoints;

    private int spawned;
    private float spawnTimer;
    private int spawnPointIdx;
    private PlayerInput playerInput;
    private PlayerSpawner playerSpawner;
    private Agent agent;

    private void Awake()
    {
        playerSpawner = GetComponent<PlayerSpawner>();
    }
    private void Start()
    {
        spawned = 0;
        spawnPointIdx = Random.Range(0, spawnPoints.Length);
        playerInput = playerSpawner.agent.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (spawned < TotolToSpawn)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = Random.Range(2, 5);
            }
        }
    }

    private void SpawnEnemy()
    {
        var mask = LayerMask.GetMask("enemy", "player");
        var pos = spawnPoints[spawnPointIdx++ % spawnPoints.Length].position + Vector3.back;
        var spaceTaken = Physics.CheckSphere(pos, 1f, mask);
        if (!spaceTaken)
        {
            var obj = Instantiate(EnemyPrefab);
            obj.transform.position = pos;

            agent = obj.GetComponent<Agent>();
            var enemy = obj.GetComponent<EnemyAI>();
            enemy.explosionParticleSystem = explosionParticleSystem;
            enemy.player = playerSpawner.agent;
            enemy.DeathCB.AddListener(EnemyDied);
            enemy.HitCB.AddListener(EnemyHit);

            enemy.gridProxy = gridMgr.CreateProxy(pos);
            agent.destProxy = gridMgr.CreateProxy(pos);
            
            spawned++;
        }
    }

    void EnemyDied()
    {
        spawned--;
        playerInput.EnemyKilled();
    }

    void EnemyHit(int hits, int hp)
    {
    }
}
