using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerBody : MonoBehaviour
{
    private Agent player;

    private void Start()
    {
        player = GetComponentInParent<Agent>();
    }
    private void OnTriggerEnter(Collider other)
    {
        player.HitByObject(other.gameObject);
    }
}
