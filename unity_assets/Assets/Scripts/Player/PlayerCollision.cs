using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public Player player;

    private void OnTriggerEnter(Collider collision)  // OnTriggerEnter2D <- ref video class. we need 3D version.
    {

       // if (collision.tag == "bullet")
       // {
            player.bulletHit();
        //}
    }
}
