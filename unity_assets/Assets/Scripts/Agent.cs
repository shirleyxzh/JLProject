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
    public Transform EyeLevel;

    // set by PlayerInput and EnemyAI
    public UnityEvent<int, int> OnAttacked { get; set; } = new UnityEvent<int, int>();
    public Transform destProxy { get; set; }

    private AgentAnimations agentAnimations;
    private AgentMover agentMover;

    private Vector2 pointerInput;
    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }

    private WeaponParent weaponParent;

    private int playerHits;
    private int playerHP;
    public int GetHP { get => playerHP; }
    public Vector3 GetPostion { get => transform.position; }

    private float pushBackTimer;
    private Vector3 pushBackDir;

    public void PerformAttack(bool AttackStarted) 
    {
        if (playerHP > 0)
        {
            var canAttack = AttackStarted && ActionsAllowed();
            agentAnimations.PlayAttack(canAttack);
            weaponParent.PerformAnAttack(canAttack);
        }
    }

    public void PeformRoll()
    {
        if (playerHP > 0 && ActionsAllowed())
            agentAnimations.PlayRoll();
    }
    private bool ActionsAllowed()
    {
        return !(agentAnimations.isRolling || pushBackTimer > 0);
    }
    private void Awake()
    {
        playerHits = 0;
        pushBackTimer = 0;
        playerHP = StartingHP;

        agentMover = GetComponent<AgentMover>();
        agentAnimations = GetComponent<AgentAnimations>();
        weaponParent = GetComponentInChildren<WeaponParent>();

        agentAnimations.eyeLevel = EyeLevel;
    }

    private void Start()
    {
        agentAnimations.PlayAnimation(Vector2.zero);        // start idle
        agentAnimations.RotateToPointer(Vector2.right);     // start looking right
    }

    public void OnMovementInput(Vector3 movementInput)
    {
        if (playerHP > 0)
        {
            var moveDir = movementInput;
            if (pushBackTimer > 0)
            {
                pushBackTimer = Mathf.Clamp01(pushBackTimer - Time.deltaTime);
                pushBackDir = (destProxy.position - transform.position).normalized;
                //Debug.DrawRay(transform.position, pushBackDir, Color.white);
                moveDir = pushBackDir;
            }
            else if (agentAnimations.isRolling)
            {
                moveDir.x = pointerInput.x > transform.position.x ? 1 : -1;
            }
            agentMover.MovementInput(moveDir);
            weaponParent.PointerPosition = pointerInput;

            agentAnimations.RotateToPointer(pointerInput);
            agentAnimations.PlayAnimation(moveDir);
        }
        else if (pushBackTimer > 0)
        {
            pushBackDir = (destProxy.position - transform.position).normalized;
            pushBackTimer = Mathf.Clamp01(pushBackTimer - Time.deltaTime);
            agentMover.MovementInput(pushBackDir);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == CanAttackByType)
        {
            if (playerHP > 0 && !agentAnimations.isRolling)
            {
                playerHits++;
                var bullet = other.GetComponent<Bullet>();
                playerHP = Mathf.Max(playerHP - bullet.damage, 0);
                OnAttacked?.Invoke(playerHits, playerHP);
                bullet.RemoveWithVFX(AttackPoint.position);
                if (playerHP == 0)
                {
                    agentMover.MovementInput(Vector3.zero);
                    weaponParent.PerformAnAttack(false);
                    agentAnimations.PlayDead();
                }
            }
        }
        else if (other.tag == "wall")
        {
            var collisionPoint = other.ClosestPoint(transform.position);
            var direction = (transform.position - collisionPoint).normalized;
            if (direction == Vector3.zero)
                direction = Vector3.up * 0.5f;

            if (pushBackTimer > 0)
                direction += pushBackDir;

            pushBackTimer = 0.5f;
            pushBackDir = direction;
            destProxy.position = transform.position + pushBackDir;
        }
    }
}
