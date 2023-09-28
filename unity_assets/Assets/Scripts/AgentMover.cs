using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AgentMover : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed = 2;
    [SerializeField]
    public Vector3 MovementInput { get;set; }

    private NavMeshAgent navMesh;

    private void Awake()
    {
        navMesh = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        var step = MovementInput * maxSpeed * Time.deltaTime;
        var newPos = transform.position + step;
        navMesh.SetDestination(newPos);
    }
}
