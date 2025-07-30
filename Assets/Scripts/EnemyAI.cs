using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum AIState { GoToStartPoint, Patrolling, Chasing }
    private AIState currentState;

    private NavMeshAgent navMeshAgent;

    [Header("플레이어 감지")]
    public Transform player;
    public float detectionRadius = 10f;
    public float loseSightRadius = 15f;

    [Header("순찰 설정")]
    [Tooltip("순찰할 지점들의 배열입니다.")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Start()
    {
        GoToStartPosition();
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.GoToStartPoint:
                HandleGoToStart();
                CheckForPlayer();
                break;
            case AIState.Patrolling:
                Patrol();
                CheckForPlayer();
                break;
            case AIState.Chasing:
                Chase();
                CheckIfPlayerLost();
                break;
        }
    }

    private void GoToStartPosition()
    {
        currentState = AIState.GoToStartPoint;
        navMeshAgent.autoBraking = true; // 시작점에는 정확히 멈추도록 설정
        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            navMeshAgent.SetDestination(waypoints[0].position);
        }
    }

    private void HandleGoToStart()
    {
        // 시작점에 가까워지면 순찰 시작
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            currentState = AIState.Patrolling;
            navMeshAgent.autoBraking = false; // 순찰 중에는 부드럽게 돌도록 설정
            GoToNextWaypoint();
        }
    }

    private void CheckForPlayer()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            currentState = AIState.Chasing;
            navMeshAgent.autoBraking = true; // 추격 시에는 플레이어 근처에서 멈추도록 설정
        }
    }

    private void CheckIfPlayerLost()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) > loseSightRadius)
        {
            GoToStartPosition(); // 플레이어를 놓치면 다시 시작점으로 복귀
        }
    }

    private void Patrol()
    {
        // 현재 웨이포인트에 가까워지면 다음 웨이포인트로 목적지 변경
        // autoBraking이 false이므로 멈추지 않고 부드럽게 지나갑니다.
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        navMeshAgent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    private void Chase()
    {
        if (player != null)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 감지 범위 그리기
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseSightRadius);

        // 웨이포인트 경로 그리기
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            
            Vector3 currentPoint = waypoints[i].position;
            Gizmos.DrawSphere(currentPoint, 0.3f);

            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex] != null)
            {
                Vector3 nextPoint = waypoints[nextIndex].position;
                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }
    }
}
