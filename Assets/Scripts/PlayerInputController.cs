using UnityEngine;
using UnityEngine.InputSystem;
using System;

// 3개의 액션맵 인터페이스를 모두 상속받습니다.
public class PlayerInputController : MonoBehaviour, Player_Actions.IDungeonActions, Player_Actions.IBattle_CommandActions, Player_Actions.IBattle_ActionActions
{
    private Player_Actions playerActions;

    // Dungeon Actions
    public Vector2 MoveDirection { get; private set; }
    public Vector2 LookDirection { get; private set; }
    public event Action OnJumpAction;
    
    // Battle Actions - 메소드와의 이름 충돌을 피하기 위해 이벤트 이름 변경
    public event Action OnSubmitEvent;
    public event Action OnCancelEvent;
    public event Action OnBattleAttackEvent;
    public event Action OnDodgeEvent;
    public event Action OnPullEvent;

    // UI 표시용 통합 이벤트
    public event Action<string> OnActionForDisplay;

    private void Awake()
    {
        playerActions = new Player_Actions();
        // 각 액션맵에 대한 콜백을 설정합니다.
        playerActions.Dungeon.SetCallbacks(this);
        playerActions.Battle_Command.SetCallbacks(this);
        playerActions.Battle_Action.SetCallbacks(this);
    }

    private void OnEnable()
    {
        // 시작 시 Dungeon 컨트롤 활성화
        EnableDungeonControls();
    }

    private void OnDisable()
    {
        // 비활성화 시 모든 액션맵을 끕니다.
        playerActions.Dungeon.Disable();
        playerActions.Battle_Command.Disable();
        playerActions.Battle_Action.Disable();
    }

    private void LateUpdate()
    {
        // Dungeon 액션맵이 활성화 되어 있을 때만 값을 읽습니다.
        if (playerActions.Dungeon.enabled)
        {
            MoveDirection = playerActions.Dungeon.Walk.ReadValue<Vector2>();
            LookDirection = playerActions.Dungeon.Camera.ReadValue<Vector2>();
        }
    }

    // --- 컨트롤 스위칭 함수 ---
    public void EnableDungeonControls()
    {
        playerActions.Battle_Command.Disable();
        playerActions.Battle_Action.Disable();
        playerActions.Dungeon.Enable();
        Debug.Log("Input: Dungeon Controls Enabled");
    }

    public void EnableBattleCommandControls()
    {
        playerActions.Dungeon.Disable();
        playerActions.Battle_Action.Disable();
        playerActions.Battle_Command.Enable();
        Debug.Log("Input: Battle Command Controls Enabled");
    }

    public void EnableBattleActionControls()
    {
        playerActions.Dungeon.Disable();
        playerActions.Battle_Command.Disable();
        playerActions.Battle_Action.Enable();
        Debug.Log("Input: Battle Action Controls Enabled");
    }

    // --- Dungeon Actions 콜백 ---
    public void OnCamera(InputAction.CallbackContext context) { 
        if(context.performed) OnActionForDisplay?.Invoke("Camera");
    }
    public void OnWalk(InputAction.CallbackContext context) { 
        if(context.performed) OnActionForDisplay?.Invoke("Walk");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpAction?.Invoke();
            OnActionForDisplay?.Invoke("Jump");
        }
    }

    // Dungeon의 공격 (전투 시작)
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnActionForDisplay?.Invoke("Dungeon Attack");
            Debug.Log("Dungeon Attack button pressed!");
            float detectionRadius = 5f;
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

            if (closestEnemy != null)
            {
                // 그룹 ID가 있으면 그룹 전투, 없으면 단일 전투 시작
                if (!string.IsNullOrEmpty(closestEnemy.enemyGroupId))
                {
                    Debug.Log($"Starting group battle triggered by {closestEnemy.name}: {closestEnemy.enemyGroupId}");
                    BattleManager.Instance.StartBattleByGroup(closestEnemy.enemyGroupId, this);
                }
                else if (!string.IsNullOrEmpty(closestEnemy.enemyId))
                {
                    Debug.Log($"Starting battle with closest enemy: {closestEnemy.enemyId}");
                    BattleManager.Instance.StartBattle(closestEnemy.enemyId, this);
                }
            }
        }
    }

    // --- Battle_Command Actions 콜백 ---
    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnSubmitEvent?.Invoke();
            OnActionForDisplay?.Invoke("Submit");
            Debug.Log("Submit Action Triggered");
        }
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnCancelEvent?.Invoke();
            OnActionForDisplay?.Invoke("Cancel");
            Debug.Log("Cancel Action Triggered");
        }
    }
    
    // --- Battle_Action Actions 콜백 ---
    // 이름이 같은 OnAttack을 명시적으로 구현
    void Player_Actions.IBattle_ActionActions.OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnBattleAttackEvent?.Invoke();
            OnActionForDisplay?.Invoke("Battle Attack");
            Debug.Log("Battle Action: Attack Triggered");
        }
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnDodgeEvent?.Invoke();
            OnActionForDisplay?.Invoke("Dodge");
            Debug.Log("Battle Action: Dodge Triggered");
        }
    }

    public void OnPull(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPullEvent?.Invoke();
            OnActionForDisplay?.Invoke("Pull");
            Debug.Log("Battle Action: Pull Triggered");
        }
    }
}
