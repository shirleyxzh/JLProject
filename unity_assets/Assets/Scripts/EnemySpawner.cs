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

    private int killed;
    private float spawnTimer;
    private int spawnPointIdx;
    private Agent playerAgent;
    private PlayerInput playerInput;
    private PlayerSpawner playerSpawner;
    public List<EnemyAI> enemiesSpawned = new List<EnemyAI>();

    private bool disableSpawner = true;

    // debug
    private bool oneshotModeOn = false;

    public void StartLevel(GameObject player)
    {
        killed = 0;
        spawnTimer = 1f;
        enemiesSpawned.Clear();
        spawnPointIdx = Random.Range(0, spawnPoints.Count);
        playerAgent = player.GetComponent<Agent>();
        playerInput = playerAgent.GetComponent<PlayerInput>();
        playerSpawner = gameObject.GetComponent<PlayerSpawner>();

        disableSpawner = spawnPoints.Count == 0;
    }

    public void EndLevel()
    {
        disableSpawner = true;
    }

    private void Update()
    {
        if (disableSpawner)
            return;

        if (enemiesSpawned.Count < TotolToSpawn)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = Random.Range(0.25f, 0.5f);
            }
        }
    }

    private void SpawnEnemy()
    {
        var mask = LayerMask.GetMask("enemy", "player");
        var pos = spawnPoints[spawnPointIdx++ % spawnPoints.Count].transform.position + Vector3.back;
        var spaceTaken = Physics.CheckSphere(pos, 0.25f, mask);
        if (!spaceTaken)
        {
            var obj = Instantiate(EnemyPrefab, pos, Quaternion.identity);
            var enemy = obj.GetComponent<EnemyAI>();
            var agent = obj.GetComponent<Agent>();

            agent.OneShotKill = oneshotModeOn;
            enemy.explosionParticleSystem = explosionParticleSystem;
            enemy.player = playerAgent;
            enemy.DeathCB.AddListener(EnemyDied);
            enemy.HitCB.AddListener(EnemyHit);

            enemy.gridProxy = playerSpawner.GridMgr.CreateProxy(pos);     // for enemy movement
            agent.destProxy = playerSpawner.GridMgr.CreateProxy(pos);     // for pushback collisions

            agent.StartLevel();
            enemiesSpawned.Add(enemy);
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

    public void ForceEnd()
    {
        if (disableSpawner)
            return;

        // remove all the enemies to force a level end
        foreach (var enemy in enemiesSpawned)
        {
            if (enemy != null)
                enemy.DebugRemove();
        }
        TotolToSpawn = 0;
    }

    public void ToggleOneShotKill()
    {
        if (disableSpawner)
            return;

        oneshotModeOn = !oneshotModeOn;
        foreach (var enemy in enemiesSpawned)
        {
            if (enemy != null)
                enemy.GetComponent<Agent>().OneShotKill = oneshotModeOn;
        }
        playerSpawner.SetOneShot(oneshotModeOn); 
    }
}
