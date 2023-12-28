using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AgentMover : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed = 2;
    
    private int savedLayer;
    private int defaultLayer;
    private int testMask;

    private BoxCollider boxCol;
    private float wallGap = 0.1f;

    private void Awake()
    {
        savedLayer = gameObject.layer;
        boxCol = GetComponent<BoxCollider>();
        defaultLayer = LayerMask.NameToLayer("Default");
        testMask = LayerMask.GetMask("enemy", "blocker");
    }

    public void MovementInput(Vector3 direction, bool forcedMove = false)
    {
        var step = direction * maxSpeed * Time.deltaTime;
        var pos = transform.position;
        var newPos = pos + step;
        gameObject.layer = defaultLayer;

        var dx = Mathf.Abs(step.x) + wallGap;
        var dir = step.x < 0 ? Vector3.left : Vector3.right;
        var off1 = dir * dx;
        var off2 = dir * (boxCol.bounds.size.x + dx);
        var p1 = step.x < 0 ? boxCol.bounds.min : boxCol.bounds.max;
        var p2 = step.x < 0 ? boxCol.bounds.max : boxCol.bounds.min;
        if (!forcedMove && testEdge(p1, p2, off1, off2))
            newPos.x = pos.x;

        var dy = Mathf.Abs(step.y) + wallGap;
        dir = step.y < 0 ? Vector3.down : Vector3.up;
        off1 = dir * dy;
        off2 = dir * (boxCol.bounds.size.y + dy);
        p1 = step.y < 0 ? boxCol.bounds.min : boxCol.bounds.max;
        p2 = step.y < 0 ? boxCol.bounds.max : boxCol.bounds.min;
        if (!forcedMove && testEdge(p1, p2, off1, off2))
            newPos.y = pos.y;

        gameObject.layer = savedLayer;

        transform.position = newPos;
    }

    private bool testEdge(Vector3 p1, Vector3 p2, Vector3 off1, Vector3 off2)
    {
        var hit = Physics.Linecast(p1, p1 + off1, testMask) || Physics.Linecast(p2, p2 + off2, testMask);
        return hit;
    }
}
