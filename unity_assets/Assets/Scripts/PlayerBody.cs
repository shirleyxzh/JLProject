using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    public PlayerMove player;

    private void OnTriggerEnter(Collider other)
    {
        player.HitByBullet();
    }
}
