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
    private TextMeshProUGUI hudKills;

    [SerializeField]
    private Vector3[] spawnPoints;

    private int kills;
    private int spawned;
    private float spawnTimer;
    private int spawnPointIdx;
    private PlayerSpawner playerSpawner;

    private void Awake()
    {
        playerSpawner = GetComponent<PlayerSpawner>();
    }
    private void Start()
    {
        kills = 0;
        spawned = 0;
        spawnPointIdx = Random.Range(0, spawnPoints.Length);
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
        var mask = LayerMask.GetMask("enemy") | LayerMask.GetMask("player");
        var pos = spawnPoints[spawnPointIdx++ % spawnPoints.Length];
        var spaceTaken = Physics.CheckSphere(pos, 1f, mask);
        if (!spaceTaken)
        {
            var obj = Instantiate(EnemyPrefab);
            obj.transform.position = pos;

            var spawnedEnemy = obj.GetComponent<EnemyAI>();
            spawnedEnemy.explosionParticleSystem = explosionParticleSystem;
            spawnedEnemy.player = playerSpawner.player;
            spawnedEnemy.DeathCB.AddListener(EnemyDied);
            spawnedEnemy.HitCB.AddListener(EnemyHit);
            spawned++;
        }
    }

    void EnemyDied()
    {
        kills++;
        spawned--;
        hudKills.text = $"Kills: {kills}";
    }

    void EnemyHit(int hits, int hp)
    {
    }
}
