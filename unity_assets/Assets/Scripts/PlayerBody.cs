using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    public Agent player;

    private void OnTriggerEnter(Collider other)
    {
        //player.HitByBullet();
        return;
    }
}
