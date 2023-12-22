using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    [SerializeField, Min(0)]
    int explosionParticleCount = 50;

    [SerializeField]
    private float chaseDist = 3, attackDist = 0.8f;

    [SerializeField]
    private float attackDelay = 1;

    // callbacks to update HUD - - set by EnemySpawner
    public UnityEvent<int, int> HitCB { get;  set; } = new UnityEvent<int, int>();
    public UnityEvent DeathCB { get; set; } = new UnityEvent();


    public Agent player { get; set; }
    public Transform gridProxy { get; set; }
    public ParticleSystem explosionParticleSystem { get; set; }

    private bool isDead = false;
    private float passedTime = 1;
    private float destoryTimer = 0.5f;

    private Agent thisEnemy;
    private Vector3 lastSeenPosition;
    private bool lastSeenValid = false;
    private bool onPatrol = false;
    private float patrolTimer;

    private int losMask;
    private int patrolMask;

    private void Start()
    {
        thisEnemy = GetComponent<Agent>();
        thisEnemy.OnAttacked.AddListener(WasAttacked);
        losMask = LayerMask.GetMask("blocker");
        patrolMask = LayerMask.GetMask("enemy", "blocker");
    }
    private void Update()
    {
        transform.position = gridProxy.position;

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
                            position = enemyPos,
                            applyShapeToPosition = true
                        },
                        explosionParticleCount
                    );
                    Destroy(thisEnemy.destProxy.gameObject);
                    Destroy(gridProxy.gameObject);
                    Destroy(gameObject);
                }
            }
            return;
        }

        if (player.GetHP == 0)
        {
            // player is dead
            thisEnemy.PerformAttack(false);
            thisEnemy.OnMovementInput(Vector2.zero);
            return;
        }

        bool canSeePlayer = false;
        float dist = Vector2.Distance(playerPos, enemyPos);
        if (dist < chaseDist)
        {
            // check line of site
            canSeePlayer = !Physics.Linecast(thisEnemy.EyeLevel.position, playerPos, losMask);
        }

        // TODO: don't collide with other enemies
        if (canSeePlayer)
        {
            onPatrol = false;
            lastSeenValid = true;
            lastSeenPosition = playerPos;
            thisEnemy.PointerInput = lastSeenPosition;
            var canAttack = dist <= attackDist && passedTime >= attackDelay;
            thisEnemy.PerformAttack(canAttack);
            if (canAttack)
                passedTime = 0;

            if (dist > attackDist / 2f)
            {
                // chasing player
                Vector2 dir = playerPos - enemyPos;
                thisEnemy.OnMovementInput(dir.normalized);
            }
        }
        else if (lastSeenValid || onPatrol)
        {
            // move close to last known pos
            thisEnemy.PerformAttack(false);
            var dir = lastSeenPosition - enemyPos;
            thisEnemy.OnMovementInput(dir.normalized);
            //Debug.DrawRay(enemyPos, dir, Color.white);

            dist = Vector2.Distance(lastSeenPosition, enemyPos);
            if (lastSeenValid) lastSeenValid = dist > attackDist / 3;
            if (onPatrol)
            {
                patrolTimer -= Time.deltaTime;
                onPatrol = patrolTimer > 0;
            }
        }
        else
        {
            // pick a random point in the room and walk to it
            thisEnemy.PerformAttack(false);
            thisEnemy.OnMovementInput(Vector2.zero);

            lastSeenPosition = enemyPos + Vector3.left * Random.Range(-1f, 1f) + Vector3.up * Random.Range(-1f, 1f);
            onPatrol = !Physics.Linecast(enemyPos, lastSeenPosition, patrolMask);
            patrolTimer = 2f;
        }

        if (passedTime < attackDelay)
            passedTime += Time.deltaTime;

        gridProxy.position = transform.position;
    }

    public void WasAttacked(int hits, int hp)
    {
        HitCB?.Invoke(hits, hp);
        if (hp == 0 && !isDead)
        {
            isDead = true;
            DeathCB?.Invoke();
            thisEnemy.PerformAttack(false);
            thisEnemy.OnMovementInput(Vector2.zero);
        }
    }

    public void DebugRemove()
    {
        if (!isDead)
        {
            isDead = true;
            thisEnemy.PerformAttack(false);
            thisEnemy.OnMovementInput(Vector2.zero);
        }
    }
}
