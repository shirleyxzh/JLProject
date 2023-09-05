using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Agent : MonoBehaviour
{
    public UnityEvent<int, int> OnAttacked;

    public int StartingHP;
    public string CanAttackByType;
    public Transform AttackPoint;

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
            agentMover.MovementInput = movementInput;
            weaponParent.PointerPosition = pointerInput;

            agentAnimations.RotateToPointer(pointerInput);
            agentAnimations.PlayAnimation(movementInput);
        }
    }

    public void HitByObject(GameObject obj)
    {
        var bullet = obj.GetComponentInParent<Bullet>();
        if (bullet && bullet.type == CanAttackByType && playerHP > 0)
        {
            playerHits++;
            playerHP = Mathf.Max(playerHP - bullet.damage, 0);
            OnAttacked?.Invoke(playerHits, playerHP);
            bullet.gameObject.SetActive(false);
            if (playerHP == 0)
            {
                agentMover.MovementInput = Vector2.zero;
                weaponParent.PerformAnAttack(false);
                agentAnimations.PlayDead();
            }
        }
    }
}
