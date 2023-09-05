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
    private TextMeshProUGUI hudKills;

    [SerializeField]
    private Vector3[] spawnPoints;

    private int kills;
    private GameObject spawnedEnemy;

    private void Start()
    {
        kills = 0;
        SpawnEnemy();
    }

    private void Update()
    {
        if (spawnedEnemy == null)
        {
            kills++;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        spawnedEnemy = Instantiate(EnemyPrefab);
        spawnedEnemy.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)];
        spawnedEnemy.GetComponent<EnemyAI>().player = player;

        hudKills.text = $"Kills: {kills}";
    }
}
