using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(PlayerSpawner))]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject EnemyPrefab;

    public int TotolToSpawn { get; set; }
    public bool EnemiesLeft => killed < TotolToSpawn;
    public List<GameObject> spawnPoints { get; set; } = new List<GameObject>();

    [SerializeField]
    ParticleSystem explosionParticleSystem;

    [SerializeField]
    private GridMgr gridMgr;

    private Agent agent;
    private int killed;
    private int spawned;
    private float spawnTimer;
    private int spawnPointIdx;
    private Agent playerAgent;
    private PlayerInput playerInput;

    private bool disbaleSpawner = true;

    public void StartLevel(GameObject player)
    {
        killed = 0;
        spawned = 0;
        spawnTimer = Random.Range(1f, 2f);
        spawnPointIdx = Random.Range(0, spawnPoints.Count);
        playerAgent = player.GetComponent<Agent>();
        playerInput = playerAgent.GetComponent<PlayerInput>();

        disbaleSpawner = spawnPoints.Count == 0;
    }

    public void EndLevel()
    {
        disbaleSpawner = true;
    }

    private void Update()
    {
        if (disbaleSpawner)
            return;

        if (spawned < TotolToSpawn)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = Random.Range(2f, 5f);
            }
        }
    }

    private void SpawnEnemy()
    {
        var mask = LayerMask.GetMask("enemy", "player");
        var pos = spawnPoints[spawnPointIdx++ % spawnPoints.Count].transform.position + Vector3.back;
        var spaceTaken = Physics.CheckSphere(pos, 1f, mask);
        if (!spaceTaken)
        {
            var obj = Instantiate(EnemyPrefab, pos, Quaternion.identity);
            agent = obj.GetComponent<Agent>();
            var enemy = obj.GetComponent<EnemyAI>();
            enemy.explosionParticleSystem = explosionParticleSystem;
            enemy.player = playerAgent;
            enemy.DeathCB.AddListener(EnemyDied);
            enemy.HitCB.AddListener(EnemyHit);

            enemy.gridProxy = gridMgr.CreateProxy(pos);     // for enemy movement
            agent.destProxy = gridMgr.CreateProxy(pos);     // for pushback collisions

            agent.StartLevel();
            spawned++;
        }
    }

    void EnemyDied()
    {
        killed++;
        playerInput.EnemyKilled();
    }

    void EnemyHit(int hits, int hp)
    {
    }
}
