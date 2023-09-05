using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent<bool> OnAttack;

    public Agent player;

    [SerializeField]
    private float chaseDist = 3, attackDist = 0.8f;

    [SerializeField]
    private float attackDelay = 1;
    private float passedTime = 1;
    private bool isDead = false;

    private void Update()
    {
        if (isDead)
            return;

        if (player.GetHP == 0)
        {
            OnAttack?.Invoke(false);
            OnMovementInput?.Invoke(Vector2.zero);
            return;
        }

        float dist = Vector2.Distance(player.transform.position, transform.position);
        if (dist < chaseDist)
        {
            OnPointerInput?.Invoke(player.AttackPoint.position);
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
                Vector2 dir = player.transform.position - transform.position;
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

    public void WasAttacked(int hits, int hp)
    {
        if (hp == 0 && !isDead)
        {
            isDead = true;
            OnAttack?.Invoke(false);
            OnMovementInput?.Invoke(Vector2.zero);
            Destroy(gameObject, 2);
        }
    }
}
