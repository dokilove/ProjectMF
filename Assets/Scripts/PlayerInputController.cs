using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, Player_Actions.IPlayerActions
{
    private Player_Actions playerActions;

    public Vector2 MoveDirection { get; private set; }
    public Vector2 LookDirection { get; private set; }

    public event System.Action OnJumpAction;
    public event System.Action<string> OnActionTriggered;

    private void Awake()
    {
        playerActions = new Player_Actions();
        playerActions.Player.SetCallbacks(this);
    }

    private void OnEnable()
    {
        playerActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerActions.Player.Disable();
    }

    private void LateUpdate()
    {
        MoveDirection = playerActions.Player.Walk.ReadValue<Vector2>();
        LookDirection = playerActions.Player.Camera.ReadValue<Vector2>();
    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnActionTriggered?.Invoke(context.control.displayName);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpAction?.Invoke();
            OnActionTriggered?.Invoke(context.control.displayName);
        }
    }

    public void OnWalk(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnActionTriggered?.Invoke(context.control.displayName);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnActionTriggered?.Invoke(context.control.displayName);
            Debug.Log("Attack button pressed!");

            // 플레이어 주변의 적을 감지하고 가장 가까운 적과 전투를 시작합니다.
            float detectionRadius = 5f; // 감지 반경
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
            EnemyAI closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                EnemyAI enemy = hitCollider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }

            // 가장 가까운 적을 찾았고, 해당 적의 ID가 있다면 전투 시작
            if (closestEnemy != null && !string.IsNullOrEmpty(closestEnemy.enemyId))
            {
                Debug.Log($"Starting battle with closest enemy: {closestEnemy.enemyId}");
                BattleManager.Instance.StartBattle(closestEnemy.enemyId);
            }
        }
    }

    // Zoom 관련 코드가 모두 제거되었습니다.
    public void OnZoom(InputAction.CallbackContext context) {}
}
