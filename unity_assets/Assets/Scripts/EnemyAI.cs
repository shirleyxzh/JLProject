using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent<bool> OnAttack;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private float chaseDist = 3, attackDist = 0.8f;

    [SerializeField]
    private float attackDelay = 1;
    private float passedTime = 1;

    private void Update()
    {
        if (player == null)
            return;

        float dist = Vector2.Distance(player.position, transform.position);
        if (dist < chaseDist)
        {
            OnPointerInput?.Invoke(player.position);
            if (dist <= attackDist)
            {
                // attack player
                OnMovementInput?.Invoke(Vector2.zero);
                if (passedTime >= attackDelay)
                {
                    passedTime = 0;
                    OnAttack?.Invoke(true);
                }
            }
            else
            {
                // chasing player
                OnAttack?.Invoke(false);
                Vector2 dir = player.position - transform.position;
                OnMovementInput?.Invoke(dir.normalized);
            }
        }
        else
        {
            OnMovementInput?.Invoke(Vector2.zero);
        }

        if (passedTime < attackDelay)
            passedTime += Time.deltaTime;
    }
}
