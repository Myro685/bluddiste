using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 5f; // Dosah detekce hráče
    public float chaseSpeed = 4f; // Rychlost při pronásledování
    public float patrolSpeed = 3f; // Rychlost při hlídkování
    public float patrolWaitTime = 2f; // Čas čekání na hlídce
    public float attackRange = 1f; // Vzdálenost pro útok
    public float attackDamage = 10f; // Poškození při útoku
    public float attackCooldown = 1f; // Čas mezi útoky

    private Transform player; // Reference na hráče
    private NavMeshAgent agent; // Reference na NavMeshAgent
    private PlayerController playerController; // Reference na skript hráče
    private Vector3 targetPosition; // Cílová pozice pro hlídkování
    private float waitTimer; // Časovač pro čekání na hlídce
    private bool isChasing; // Zda nepřítel pronásleduje hráče
    private bool wasChasingLastFrame; // Sledování, zda nepřítel pronásledoval v posledním snímku
    private float lastAttackTime; // Čas posledního útoku
    private List<Vector2Int> patrolPoints; // Seznam bodů pro hlídkování

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        patrolPoints = GameObject.Find("Maze").GetComponent<MazeGenerator>().GetFreeCells();

        // Kontrola, zda máme patrolPoints
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            Debug.LogWarning("Nepřítel nemá žádné body pro hlídkování!");
            return;
        }

        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.5f;

        SetNewPatrolTarget();
    }

    void Update()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        // Detekce hráče
        bool canSeePlayer = CanSeePlayer();
        if (canSeePlayer)
        {
            isChasing = true;
            agent.speed = chaseSpeed;
        }
        else
        {
            if (isChasing && wasChasingLastFrame)
            {
                SetNewPatrolTarget();
            }
            isChasing = false;
            agent.speed = patrolSpeed;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        wasChasingLastFrame = isChasing;
    }

    bool CanSeePlayer()
    {
        if (playerController.IsHiding())
        {
            return false;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
        }
    }

    void Patrol()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget < 0.5f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                SetNewPatrolTarget();
            }
        }

        agent.SetDestination(targetPosition);
    }

    void SetNewPatrolTarget()
    {
        if (patrolPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, patrolPoints.Count);
            Vector2Int patrolPoint = patrolPoints[randomIndex];
            targetPosition = new Vector3(patrolPoint.x, transform.position.y, patrolPoint.y);
            waitTimer = patrolWaitTime;

            // Zajistíme, že cíl je na NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }
            else
            {
                // Pokud cíl není na NavMesh, vybereme jiný
                SetNewPatrolTarget();
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        Debug.Log("Nepřítel útočí na hráče! Poškození: " + attackDamage);
    }
}