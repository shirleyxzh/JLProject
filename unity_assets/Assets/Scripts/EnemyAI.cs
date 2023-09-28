using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent<bool> OnAttack;
    public UnityEvent DeathCB;

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
    
    private Agent thisEnemy;
    private Vector3 lastSeenPosition;
    private bool lastSeenValid = false;

    private void Start()
    {
        thisEnemy = GetComponent<Agent>();
    }
    private void Update()
    {
        var enemyPos = thisEnemy.AttackPoint.position;
        var playerPos = player.AttackPoint.position;

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
                            position = new Vector3(enemyPos.x, enemyPos.y),
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
            // player is dead
            OnAttack?.Invoke(false);
            OnMovementInput?.Invoke(Vector2.zero);
            return;
        }

        bool canSeePlayer = false;
        float dist = Vector2.Distance(playerPos, enemyPos);
        if (dist < chaseDist)
        {
            // check line of site
            var mask = LayerMask.GetMask("wall");
            canSeePlayer = !Physics.Linecast(enemyPos, playerPos, mask);
        }

        if (canSeePlayer)
        {
            lastSeenValid = true;
            lastSeenPosition = playerPos;

            OnPointerInput?.Invoke(playerPos);
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
                Vector2 dir = playerPos - enemyPos;
                OnMovementInput?.Invoke(dir.normalized);
            }
        }
        else if (lastSeenValid)
        {
            // move close to last known pos
            OnAttack?.Invoke(false);
            Vector2 dir = lastSeenPosition - enemyPos;
            OnMovementInput?.Invoke(dir.normalized);

            dist = Vector2.Distance(lastSeenPosition, enemyPos);
            lastSeenValid = dist > attackDist / 3;
        }
        else
        {
            OnAttack?.Invoke(false);
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
            DeathCB?.Invoke();
            OnAttack?.Invoke(false);
            OnMovementInput?.Invoke(Vector2.zero);
        }
    }
}
