using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AgentMover : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed = 2;
    
    public Vector3 MovementInput { get;set; }

    private NavMeshAgent navMesh;
    private bool fixupForNav;

    private void Awake()
    {
        navMesh = GetComponent<NavMeshAgent>();
        fixupForNav = true;
    }

    private void Update()
    {
        var step = MovementInput * maxSpeed * Time.deltaTime;
        var newPos = navMesh.transform.position + step;
        if (navMesh.isOnNavMesh)
        {
            if (fixupForNav) FixupForNav();
            navMesh.SetDestination(newPos);
        }
        else
        {
            transform.position = newPos;
        }
    }

    private void FixupForNav()
    {
        fixupForNav = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.z + 1f, child.localPosition.y);
            child.localRotation = Quaternion.Euler(-90f, child.localRotation.eulerAngles.y, child.localRotation.eulerAngles.z);
        }

        var cbox = GetComponent<BoxCollider>();
        cbox.center = new Vector3(cbox.center.x, cbox.center.z + 1f, cbox.center.y);
        cbox.size = new Vector3(cbox.size.x, cbox.size.z, cbox.size.y);
    }
}
