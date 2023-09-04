using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public GameObject Player;
    
    private void LateUpdate()
    {
        var newPos = Player.transform.position;
        newPos.z = transform.position.z;
        transform.position = newPos;
    }
}
