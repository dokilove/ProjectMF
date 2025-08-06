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

            // 플레이어 주변의 적을 감지합니다.
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f); // 5f는 감지 반경, 조절 가능
            foreach (var hitCollider in hitColliders)
            {
                EnemyAI enemy = hitCollider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    // 적이 추격 중이 아닐 때 (시야각 밖에서 공격)
                    if (!enemy.IsChasing())
                    {
                        BattleManager.Instance.StartBattle();
                        // 여기에 전투 화면 전환 로직을 추가합니다.
                        break; // 첫 번째 감지된 적에 대해서만 처리
                    }
                }
            }
        }
    }

    // Zoom 관련 코드가 모두 제거되었습니다.
    public void OnZoom(InputAction.CallbackContext context) {}
}
