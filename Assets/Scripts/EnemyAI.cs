using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// AI skript pro nepřítele – patroluje, detekuje hráče, pronásleduje a útočí.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 5f;      // Vzdálenost, na kterou nepřítel detekuje hráče
    public float chaseSpeed = 4f;          // Rychlost při pronásledování hráče
    public float patrolSpeed = 3f;         // Rychlost při hlídkování
    public float patrolWaitTime = 2f;      // Čas čekání na patrolovacím bodě
    public float attackRange = 1f;         // Vzdálenost, na kterou může nepřítel zaútočit
    public float attackDamage = 10f;       // Poškození způsobené útokem
    public float attackCooldown = 1f;      // Časový odstup mezi útoky

    private Transform player;              // Reference na hráče
    private NavMeshAgent agent;            // NavMesh agent pro pohyb
    private PlayerController playerController; // Reference na skript hráče
    private Vector3 targetPosition;        // Aktuální cíl pro pohyb
    private float waitTimer;               // Časovač čekání na patrolovacím bodě
    private bool isChasing;                // Zda právě pronásleduje hráče
    private bool wasChasingLastFrame;      // Zda pronásledoval hráče v minulém snímku
    private float lastAttackTime;          // Čas posledního útoku
    private List<Vector2Int> patrolPoints; // Seznam bodů pro hlídkování

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

        SetNewPatrolTarget(); // Nastaví první patrolovací bod
    }

    void Update()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        bool canSeePlayer = CanSeePlayer(); // Zjistí, zda vidí hráče
        if (canSeePlayer)
        {
            isChasing = true;
            agent.speed = chaseSpeed;
        }
        else
        {
            if (isChasing && wasChasingLastFrame)
            {
                SetNewPatrolTarget(); // Pokud přestal pronásledovat, nastaví nový patrolovací bod
            }
            isChasing = false;
            agent.speed = patrolSpeed;
        }

        if (isChasing)
        {
            ChasePlayer(); // Pronásleduje hráče
        }
        else
        {
            Patrol(); // Hlídkuje
        }

        wasChasingLastFrame = isChasing;
    }

    /// <summary>
    /// Zjistí, zda nepřítel vidí hráče (není schovaný a je v dosahu + není za zdí).
    /// </summary>
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

    /// <summary>
    /// Pronásleduje hráče a útočí, pokud je dostatečně blízko.
    /// </summary>
    void ChasePlayer()
    {
        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
        }
    }

    /// <summary>
    /// Hlídkuje mezi body v bludišti.
    /// </summary>
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

    /// <summary>
    /// Nastaví nový patrolovací bod.
    /// </summary>
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
                SetNewPatrolTarget(); // Pokud není bod na NavMesh, zkusí jiný
            }
        }
    }

    /// <summary>
    /// Útok na hráče.
    /// </summary>
    void Attack()
    {
        lastAttackTime = Time.time;
        Debug.Log("Nepřítel útočí na hráče! Poškození: " + attackDamage);
        playerController.TakeDamage(attackDamage); 
    }
}