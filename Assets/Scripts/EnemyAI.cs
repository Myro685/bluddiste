using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 5f;
    public float chaseSpeed = 4f;
    public float patrolSpeed = 3f;
    public float patrolWaitTime = 2f;
    public float attackRange = 1f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    private Transform player;
    private NavMeshAgent agent;
    private PlayerController playerController;
    private Vector3 targetPosition;
    private float waitTimer;
    private bool isChasing;
    private bool wasChasingLastFrame;
    private float lastAttackTime;
    private List<Vector2Int> patrolPoints;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
        patrolPoints = GameObject.Find("Maze").GetComponent<MazeGenerator>().GetFreeCells();

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

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }
            else
            {
                SetNewPatrolTarget();
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        Debug.Log("Nepřítel útočí na hráče! Poškození: " + attackDamage);
        playerController.TakeDamage(attackDamage); // Poškodíme hráče
    }
}