using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }
    public State currentState = State.Patrol;

    public enum PatrolMode { Waypoints, RandomNavMesh }

    [Header("Patrol Mode")]
    public PatrolMode patrolMode = PatrolMode.Waypoints;
    public bool randomWaypointOrder = false;
    public float randomPatrolRadius = 10f;

    [Header("References")]
    public Transform[] patrolPoints;
    public Transform player;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Attack Settings")]
    public float attackDistance = 1.5f;
    public float attackCooldown = 1f;

    [Header("Detection Settings")]
    public float chaseDistance = 8f;
    public float viewDistance = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("State Materials")]
    public Material patrolMaterial;
    public Material chaseMaterial;
    public Material attackMaterial;

    private Renderer rend;
    private NavMeshAgent agent;
    private int patrolIndex = 0;
    private float attackTimer = 0f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();

        rend.material = patrolMaterial;
        agent.speed = patrolSpeed;
    }

    void Update()
    {
        if (!player.IsDestroyed())
        {
            switch (currentState)
            {
                case State.Patrol:
                    Patrol();
                    break;

                case State.Chase:
                    Chase();
                    break;

                case State.Attack:
                    Attack();
                    break;
            }

            attackTimer -= Time.deltaTime;
        }
    }

    // ---------------- PATROL ---------------- //
    void Patrol()
    {
        rend.material = patrolMaterial;
        agent.speed = patrolSpeed;

        if (!agent.hasPath || agent.remainingDistance < 0.3f)
        {
            if (patrolMode == PatrolMode.Waypoints && patrolPoints.Length > 0)
            {
                if (randomWaypointOrder)
                {
                    patrolIndex = Random.Range(0, patrolPoints.Length);
                }
                else
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                }

                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            else if (patrolMode == PatrolMode.RandomNavMesh)
            {
                agent.SetDestination(GetRandomNavMeshPosition());
            }
        }

        if (ShouldStartChasing())
            currentState = State.Chase;
    }

    // ---------------- CHASE ---------------- //
    void Chase()
    {
        rend.material = chaseMaterial;
        agent.speed = chaseSpeed;

        if (!PlayerInChaseRange() && !PlayerInViewRange())
        {
            currentState = State.Patrol;
            return;
        }

        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackDistance && PlayerInViewRange())
            currentState = State.Attack;
    }

    // ---------------- ATTACK ---------------- //
    void Attack()
    {
        rend.material = attackMaterial;
        agent.ResetPath();

        if (!PlayerInChaseRange() && !PlayerInViewRange())
        {
            currentState = State.Patrol;
            return;
        }

        if (!PlayerInViewRange())
        {
            currentState = State.Chase;
            return;
        }

        transform.LookAt(player);

        if (attackTimer <= 0f)
        {
            Debug.Log("Enemy attacked the player!");
            attackTimer = attackCooldown;
        }
    }

    // ---------------- DETECTION ---------------- //
    bool PlayerInChaseRange()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        if (dist > chaseDistance)
            return false;

        if (Physics.Raycast(transform.position, dirToPlayer.normalized, dist, obstacleMask))
            return false;

        return true;
    }

    bool PlayerInViewRange()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        if (dist > viewDistance)
            return false;

        dirToPlayer.Normalize();

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f)
            return false;

        if (Physics.Raycast(transform.position, dirToPlayer, dist, obstacleMask))
            return false;

        return true;
    }

    bool ShouldStartChasing()
    {
        return PlayerInChaseRange() || PlayerInViewRange();
    }

    // ---------------- RANDOM NAVMESH PATROL ---------------- //
    Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * randomPatrolRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, randomPatrolRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    // ---------------- GIZMOS ---------------- //
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.cyan;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + left * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + right * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}