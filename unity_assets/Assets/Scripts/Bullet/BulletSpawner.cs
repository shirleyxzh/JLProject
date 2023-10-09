using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public BulletSpawnData[] spawnDatas;
    public int index = 0;
    public bool isSequenceRandom;
    public bool isSequenceInOrder;
    public bool spawnAutomatically;

    BulletSpawnData GetSpawnData()
    {
        return spawnDatas[index];
    }
    float timer;
    Vector3 targetDirection;

    float[] rotations;
    void Start()
    {
        timer = GetSpawnData().cooldown;
        spawnAutomatically = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnAutomatically)
        {
            if (timer <= 0)
            {
                SpawnBullets();
                timer = GetSpawnData().cooldown;
                if (isSequenceRandom)
                {
                    index = Random.Range(0, spawnDatas.Length);
                }
                else if (isSequenceInOrder)
                {
                    index += 1;
                    if (index >= spawnDatas.Length) index = 0;
                }
                rotations = new float[GetSpawnData().numberOfBullets];
            }
            timer -= Time.deltaTime;
        }
    }

    // Select a random rotation from min to max for each bullet
    public float[] RandomRotations()
    {
        var difference = GetSpawnData().maxRotation - GetSpawnData().minRotation;
        var halfTurn = difference / 2f;
        for (int i = 0; i < GetSpawnData().numberOfBullets; i++)
        {
            rotations[i] = Random.Range(GetSpawnData().minRotation, GetSpawnData().maxRotation);
            rotations[i] -= halfTurn;
        }
        return rotations;

    }
    
    // This will set random rotations evenly distributed between the min and max Rotation.
    public float[] DistributedRotations()
    {
        var difference = GetSpawnData().maxRotation - GetSpawnData().minRotation;
        var halfTurn = difference / 2f;
        for (int i = 0; i < GetSpawnData().numberOfBullets; i++)
        {
            if (GetSpawnData().numberOfBullets > 1)
            {
                var fraction = (float)i / ((float)GetSpawnData().numberOfBullets - 1);
                var fractionOfDifference = fraction * difference;
                rotations[i] = fractionOfDifference + GetSpawnData().minRotation; // We add minRotation to undo Difference
            }
            else
            {
                rotations[i] = GetSpawnData().minRotation;
            }
            rotations[i] -= halfTurn;
        }
        return rotations;
    }
    public GameObject[] SpawnBullets()
    {
        rotations = new float[GetSpawnData().numberOfBullets];
        if (GetSpawnData().isRandom)
        {
            RandomRotations();
        } else
        {
            DistributedRotations();
        }

        // Spawn Bullets
        GameObject[] spawnedBullets = new GameObject[GetSpawnData().numberOfBullets];
        for (int i = 0; i < GetSpawnData().numberOfBullets; i++)
        {
            if (GetSpawnData().bulletTag != "Untagged")
                spawnedBullets[i] = BulletManager.GetBulletFromPoolWithType(GetSpawnData().bulletTag);
            else
                spawnedBullets[i] = BulletManager.GetBulletFromPool();
            if (spawnedBullets[i] == null)
            {
                spawnedBullets[i] = Instantiate(GetSpawnData().bulletResource, transform);
                BulletManager.bullets.Add(spawnedBullets[i]);
            } else
            {
                spawnedBullets[i].transform.SetParent(transform);
            }
            spawnedBullets[i].transform.localPosition = Vector3.back * 0.01f;
            spawnedBullets[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg);

            var b = spawnedBullets[i].GetComponent<Bullet>();
            b.timer = b.lifeTime;
            b.rotation = rotations[i];
            b.direction = b.transform.right;
            b.speed = GetSpawnData().bulletSpeed;
            b.hitVFX = BulletManager.GetHitVFX();
            if (!GetSpawnData().isParent) spawnedBullets[i].transform.SetParent(BulletManager.bulletDump.transform);
        }
        return spawnedBullets;
    }

    public void ShootAt(Vector3 target)
    {
        targetDirection = (target - transform.position).normalized;
        spawnAutomatically = true;
    }

    public void StopShooting()
    {
        spawnAutomatically = false;
    }
}
