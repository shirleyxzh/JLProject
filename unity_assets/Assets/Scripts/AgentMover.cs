using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AgentMover : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed = 2;
    [SerializeField]
    public Vector2 MovementInput { get;set; }

    private void FixedUpdate()
    {
        var move = new Vector3(MovementInput.x, MovementInput.y, 0) * maxSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }
}
