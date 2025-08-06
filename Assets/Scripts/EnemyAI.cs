using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum AIState { GoToStartPoint, Patrolling, Chasing }
    private AIState currentState;

    private NavMeshAgent navMeshAgent;
    private Collider enemyCollider;

    [Header("플레이어 감지")]
    public Transform player;
    public float detectionRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 60f;
    public float loseSightRadius = 15f;

    [Header("순찰 설정")]
    [Tooltip("순찰할 지점들의 배열입니다.")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    [Header("시야각 시각화")]
    public Material viewVisualizerMaterial;
    public int viewMeshResolution = 10;
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    [Header("전투 복귀 설정")]
    public float colliderEnableDelay = 1.0f; // 전투 종료 후 Collider 활성화 지연 시간

    private bool isInBattle = false;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>(); // 적 오브젝트의 Collider 컴포넌트 가져오기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // 시야각 시각화용 게임오브젝트 및 컴포넌트 설정
        GameObject viewVisualizerObject = new GameObject("ViewVisualizer");
        viewVisualizerObject.transform.SetParent(transform, false);
        viewVisualizerObject.transform.localPosition = new Vector3(0, 0.01f, 0); // 바닥에 겹치지 않게 살짝 띄움
        viewVisualizerObject.transform.localRotation = Quaternion.identity;

        viewMeshFilter = viewVisualizerObject.AddComponent<MeshFilter>();
        MeshRenderer viewMeshRenderer = viewVisualizerObject.AddComponent<MeshRenderer>();
        viewMeshRenderer.material = viewVisualizerMaterial; 

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    void Start()
    {
        GoToStartPosition();
    }

    void Update()
    {
        if (isInBattle) return; // 전투 중에는 AI 로직을 실행하지 않음

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

    void LateUpdate()
    {
        DrawFieldOfView();
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
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2f)
            {
                RaycastHit hit;
                // 적의 눈 높이에서 플레이어의 위치로 레이캐스트를 실행합니다.
                Vector3 eyePosition = transform.position + Vector3.up * 1.5f; // 예시 눈 높이
                if (Physics.Raycast(eyePosition, player.position - eyePosition, out hit, detectionRadius))
                {
                    if (hit.transform == player)
                    {
                        currentState = AIState.Chasing;
                        navMeshAgent.autoBraking = true;
                    }
                }
            }
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

    public bool IsChasing()
    {
        return currentState == AIState.Chasing;
    }

    public void SetBattleMode(bool inBattle)
    {
        isInBattle = inBattle;
        if (navMeshAgent != null)
        {
            if (inBattle) // 전투 진입 시 즉시 비활성화
            {
                navMeshAgent.enabled = false;
            }
            else // 전투 종료 시 지연 후 활성화
            {
                StartCoroutine(EnableNavMeshAgentAfterDelay(colliderEnableDelay));
            }
        }
        if (enemyCollider != null)
        {
            if (inBattle) // 전투 진입 시 즉시 비활성화
            {
                enemyCollider.enabled = false;
            }
            else // 전투 종료 시 지연 후 활성화
            {
                StartCoroutine(EnableColliderAfterDelay(colliderEnableDelay));
            }
        }
        // 전투 모드 진입 시 현재 상태를 초기화하거나 특정 상태로 변경할 수 있습니다.
        if (!inBattle) // 전투 종료 시
        {
            GoToStartPosition(); // 다시 순찰 시작점으로 돌아가도록 설정
        }
    }

    private IEnumerator EnableNavMeshAgentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
        }
    }

    private IEnumerator EnableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * viewMeshResolution / 60f);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            viewPoints.Add(DirFromAngle(angle, true));
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(transform.position + viewPoints[i] * detectionRadius);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentState == AIState.Chasing && !isInBattle)
            {
                BattleManager.Instance.StartBattle(); // 현재 EnemyAI 인스턴스를 전달하지 않음
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 감지 범위 그리기
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 시야각 그리기
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRadius);

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
