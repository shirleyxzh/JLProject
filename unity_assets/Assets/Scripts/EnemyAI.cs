using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent<bool> OnAttack;

    public bool isDead { get; private set; } = false;

    public Agent player;
    public ParticleSystem explosionParticleSystem;

    [SerializeField, Min(0)]
    int explosionParticleCount = 50;

    [SerializeField]
    private float chaseDist = 3, attackDist = 0.8f;

    [SerializeField]
    private float attackDelay = 1;
    private float passedTime = 1;
    private float destoryTimer = 2;

    private void Update()
    {
        if (isDead)    // this is where the code determain if there is an enemy on the ground... so if i have a count here... 
        {
            if (destoryTimer > 0)
            {
                destoryTimer -= Time.deltaTime;
                if (destoryTimer <= 0)
                {
                    explosionParticleSystem.Emit(
                        new ParticleSystem.EmitParams
                        {
                            position = new Vector3(transform.position.x, transform.position.y),
                            applyShapeToPosition = true
                        },
                        explosionParticleCount
                    );
                    Destroy(gameObject);
                }
            }
            return;
        }

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
        }
    }
}
