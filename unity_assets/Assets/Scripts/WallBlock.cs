using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlock : MonoBehaviour
{
    private int bulletLayer;

    private void Awake()
    {
        bulletLayer = LayerMask.NameToLayer("bullet");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == bulletLayer)
        {
            var bullet = other.GetComponent<Bullet>();
            bullet?.RemoveWithVFX(other.transform.position);
        }
    }
}
