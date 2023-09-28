using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var layer = LayerMask.NameToLayer("bullet");
        if (other.gameObject.layer == layer)
            other.gameObject.SetActive(false);
    }
}
