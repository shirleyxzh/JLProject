using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using WSWhitehouse.TagSelector;

public class Agent : MonoBehaviour
{
    public int StartingHP;
    [TagSelector]
    public string CanAttackByType;
    public Transform AttackPoint;

    public UnityEvent<int, int> OnAttacked { get; set; } = new UnityEvent<int, int>();

    private AgentAnimations agentAnimations;
    private AgentMover agentMover;

    private Vector2 pointerInput, movementInput;

    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }
    public Vector2 MovementInput { get => movementInput; set => movementInput = value; }

    private WeaponParent weaponParent;

    private int playerHits;
    private int playerHP;
    public int GetHP { get => playerHP; }

    public void PeformAttack(bool AttackStarted) 
    {
        if (playerHP > 0)
        {
            var canAttack = AttackStarted && !agentAnimations.isRolling;
            agentAnimations.PlayAttack(canAttack);
            weaponParent.PerformAnAttack(canAttack);
        }
    }
    public void PeformRoll()
    {
        if (playerHP > 0)
            agentAnimations.PlayRoll();
    }
    private void Awake()
    {
        agentMover = GetComponent<AgentMover>();
        agentAnimations = GetComponent<AgentAnimations>();
        weaponParent = GetComponentInChildren<WeaponParent>();
    }

    private void Start()
    {
        playerHits = 0;
        playerHP = StartingHP;
        OnAttacked?.Invoke(playerHits, playerHP);           // starting hits and HP
        agentAnimations.PlayAnimation(Vector2.zero);        // start idle
        agentAnimations.RotateToPointer(Vector2.right);     // start looking right
    }

    private void Update()
    {
        if (playerHP > 0)
        {
            var moveDir = movementInput;
            if (agentAnimations.isRolling)
                moveDir.x = pointerInput.x > transform.position.x ? 1 : -1;
            agentMover.MovementInput = moveDir;
            weaponParent.PointerPosition = pointerInput;

            agentAnimations.RotateToPointer(pointerInput);
            agentAnimations.PlayAnimation(moveDir);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == CanAttackByType && playerHP > 0)
        {
            playerHits++;
            var bullet = other.GetComponent<Bullet>();
            playerHP = Mathf.Max(playerHP - bullet.damage, 0);
            OnAttacked?.Invoke(playerHits, playerHP);
            bullet.RemoveWithVFX(AttackPoint.position);
            if (playerHP == 0)
            {
                agentMover.MovementInput = Vector3.zero;
                weaponParent.PerformAnAttack(false);
                agentAnimations.PlayDead();
            }
        }
    }
}
