using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private Agent player;

    [SerializeField]
    private GameObject EnemyPrefab;

    [SerializeField]
    private int enemyMax;

    [SerializeField]
    ParticleSystem explosionParticleSystem;

    [SerializeField]
    private TextMeshProUGUI hudKills;

    [SerializeField]
    private Vector3[] spawnPoints;

    private int kills;
    private EnemyAI spawnedEnemy;

    private void Start()
    {
        kills = 0;
        SpawnEnemy();
    }

    private void Update()
    {
        if (spawnedEnemy.isDead)  //<- this is calling spawn enemy.. so the emeny on screen count check should be here..?? 
        {
            kills++;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()  
    {
        var obj = Instantiate(EnemyPrefab);
        obj.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)];

        spawnedEnemy = obj.GetComponent<EnemyAI>();
        spawnedEnemy.explosionParticleSystem = explosionParticleSystem;
        spawnedEnemy.player = player;

        hudKills.text = $"Kills: {kills}";
    }
}
