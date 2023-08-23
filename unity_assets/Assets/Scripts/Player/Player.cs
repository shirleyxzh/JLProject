using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int startHp;
    int hp;
    public float bulletCooldown;
    float bulletTimer;
    void Start()
    {
        hp = startHp;
    }
    void Update()
    {
        bulletTimer -= Time.deltaTime;
    }
    public void bulletHit()  // OnTriggerEnter2D <- ref video class. we need 3D version.
    {
        if (bulletTimer <= 0)
        {
            hp -= 1;
            Debug.Log($"HP {hp}");
            bulletTimer = bulletCooldown;
        }
    }
}
