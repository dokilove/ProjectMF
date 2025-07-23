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

    // Zoom 관련 코드가 모두 제거되었습니다.
    public void OnZoom(InputAction.CallbackContext context) {}
}
