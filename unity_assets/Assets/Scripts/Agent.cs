using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class Agent : MonoBehaviour
{
    private AgentAnimations agentAnimations;
    private AgentMover agentMover;

    private Vector2 pointerInput, movementInput;

    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }
    public Vector2 MovementInput { get => movementInput; set => movementInput = value; }

    private WeaponParent weaponParent;  // this is not calling the WeaponParent class :/ why?

    public void PeformAttack(bool AttackStarted) 
    {
        weaponParent.PerformAnAttack(AttackStarted);
    }
    private void Awake()
    {
        agentMover = GetComponent<AgentMover>();
        agentAnimations = GetComponent<AgentAnimations>();
        weaponParent = GetComponentInChildren<WeaponParent>();
    }

    private void Start()
    {
        agentAnimations.PlayAnimation(Vector2.zero);        // start idle
        agentAnimations.RotateToPointer(Vector2.right);     // start looking right
    }

    private void AnimateCharacter()
    {
        agentAnimations.RotateToPointer(pointerInput);
        agentAnimations.PlayAnimation(movementInput);    
    }

    private void Update()
    {
        agentMover.MovementInput = movementInput;
        weaponParent.PointerPosition = pointerInput;

        AnimateCharacter();
    }
}
