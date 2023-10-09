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

    private int savedLayer;
    private int defaultLayer;
    private int testMask;

    private BoxCollider boxCol;
    private Vector3 xStep = Vector3.right * 0.16f;
    private Vector3 yStep = Vector3.up * 0.05f;

    private NavMeshAgent navMesh;
    private bool fixupForNav;

    private void Awake()
    {
        savedLayer = gameObject.layer;
        defaultLayer = LayerMask.NameToLayer("Default");
        testMask = LayerMask.GetMask("enemy") | LayerMask.GetMask("wall");  // | LayerMask.GetMask("floor");

        navMesh = GetComponent<NavMeshAgent>();
        fixupForNav = true;
    }

    private void LateUpdate()
    {
        if (fixupForNav) 
            FixupForNav();

        var step = MovementInput * maxSpeed * Time.deltaTime;
        var pos = transform.position;
        var newPos = pos + step;
        //if (navMesh.isOnNavMesh)
        //{
        //    navMesh.SetDestination(newPos);
        //}
        //else
        {
            gameObject.layer = defaultLayer;

            //if (step.x < 0 && Physics.Linecast(pos, newPos - xStep, testMask))
            //    newPos.x = pos.x;
            //else if (step.x > 0 && Physics.Linecast(pos, newPos + xStep, testMask))
            //    newPos.x = pos.x;
            //if (step.y < 0 && Physics.Linecast(pos, newPos - yStep, testMask))
            //    newPos.y = pos.y;
            //else if (step.y > 0 && Physics.Linecast(pos, newPos + yStep, testMask))
            //    newPos.y = pos.y;

            var dx = Mathf.Abs(step.x) + 0.05f;
            var dir = step.x < 0 ? Vector3.left : Vector3.right;
            var off1 = dir * dx;
            var off2 = dir * (boxCol.bounds.size.x + dx);
            var p1 = step.x < 0 ? boxCol.bounds.min : boxCol.bounds.max;
            var p2 = step.x < 0 ? boxCol.bounds.max : boxCol.bounds.min;
            if (testEdge(p1, p2, off1, off2))
                newPos.x = pos.x;

            var dy = Mathf.Abs(step.y) + 0.05f;
            dir = step.y < 0 ? Vector3.down : Vector3.up;
            off1 = dir * dy;
            off2 = dir * (boxCol.bounds.size.y + dy);
            p1 = step.y < 0 ? boxCol.bounds.min : boxCol.bounds.max;
            p2 = step.y < 0 ? boxCol.bounds.max : boxCol.bounds.min;
            if (testEdge(p1, p2, off1, off2))
                newPos.y = pos.y;

            gameObject.layer = savedLayer;

            if (navMesh.isOnNavMesh)
                navMesh.SetDestination(newPos);
            else
                transform.position = newPos;
        }
    }

    private bool testEdge(Vector3 p1, Vector3 p2, Vector3 off1, Vector3 off2)
    {
        var hit = Physics.Linecast(p1, p1 + off1, testMask) || Physics.Linecast(p2, p2 + off2, testMask);
        return hit;
    }

    private void FixupForNav()
    {
        fixupForNav = false;
        if (navMesh.isOnNavMesh)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.z + 1f, child.localPosition.y);
                child.localRotation = Quaternion.Euler(-90f, child.localRotation.eulerAngles.y, child.localRotation.eulerAngles.z);
            }

            boxCol = GetComponent<BoxCollider>();
            boxCol.center = new Vector3(boxCol.center.x, boxCol.center.z + 1f, boxCol.center.y);
            boxCol.size = new Vector3(boxCol.size.x, boxCol.size.z, boxCol.size.y);
        }
        else
        {
            transform.Translate(Vector3.back);
            boxCol = GetComponent<BoxCollider>();
        }
    }
}
