using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var layer = LayerMask.NameToLayer("bullet");
        if (other.gameObject.layer == layer)
        {
            var bullet = other.GetComponent<Bullet>();
            bullet?.RemoveWithVFX(other.transform.position);
        }
    }
}
