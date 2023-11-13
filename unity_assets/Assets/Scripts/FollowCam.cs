using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public GameObject Target { get; set; }
    public bool SmoothFollow { get; set; } = false;

    float smoothTime = 0.3f;
    Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (Target)
        {
            var newPos = Target.transform.position;
            newPos.z = transform.position.z;
            if (SmoothFollow)
                transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);
            else
                transform.position = newPos;
        }
    }
}
