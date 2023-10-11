using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponParent : MonoBehaviour
{
    public Vector2 PointerPosition { get; set; }
    private BulletSpawner spawner;
    private bool shooting;

    private void Awake()
    {
        spawner = GetComponentInChildren<BulletSpawner>();
        shooting = false;
    }
    private void Update() 
    {
        if (shooting)
            spawner.ShootAt(PointerPosition);
        else
            spawner.StopShooting();
    }

    public void PerformAnAttack(bool AttackStarted)
    {
        shooting = AttackStarted;
    }
}
