using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, Player_Actions.IPlayerActions
{
    private Player_Actions playerActions;

    // 공개 프로퍼티: 다른 스크립트들이 최종적으로 처리된 값을 읽어갑니다.
    public Vector2 MoveDirection { get; private set; }
    public Vector2 LookDirection { get; private set; }
    public float ZoomValue { get; private set; }

    public event System.Action OnJumpAction;
    public event System.Action<string> OnActionTriggered;

    // --- 새로 추가된 부분 ---
    // 콜백에서 받은 원본 입력 값을 임시로 저장할 내부 변수입니다.
    private Vector2 _rawLookInput;
    private float _rawZoomInput;
    // ----------------------

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

    // LateUpdate에서 매 프레임마다 현재 입력 상태를 직접 읽어와 처리합니다.
    private void LateUpdate()
    {
        // 1. 매 프레임마다 각 액션의 현재 값을 직접 읽어옵니다. (폴링)
        Vector2 currentLookInput = playerActions.Player.Camera.ReadValue<Vector2>();
        float currentZoomInput = playerActions.Player.Zoom.ReadValue<float>();

        // Walk 액션도 여기서 직접 읽어와서 MoveDirection을 갱신합니다.
        MoveDirection = playerActions.Player.Walk.ReadValue<Vector2>();

        // 2. 줌 입력이 있는지 확인합니다.
        bool isZooming = currentZoomInput != 0;

        // 3. 줌 입력 여부에 따라 공개 프로퍼티의 값을 최종 결정합니다.
        if (isZooming)
        {
            // 줌을 하는 중이라면, 카메라 움직임 값을 0으로 만들어 회전을 막습니다.
            LookDirection = Vector2.zero;
        }
        else
        {
            // 줌을 하지 않을 때만, 실제 마우스/스틱 움직임 값을 전달합니다.
            LookDirection = currentLookInput;
        }

        // 4. 최종 줌 값을 공개 프로퍼티에 할당합니다.
        ZoomValue = currentZoomInput;
    }

    // --- 아래 콜백 메소드들이 수정되었습니다 ---
    // 이제 콜백에서는 공개 프로퍼티를 직접 수정하지 않고,
    // 내부 변수에 원본 값을 저장만 합니다.

    public void OnCamera(InputAction.CallbackContext context)
    {
        _rawLookInput = context.ReadValue<Vector2>(); // 내부 변수에 저장
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
        MoveDirection = context.ReadValue<Vector2>(); // Move는 다른 로직과 충돌 없으므로 직접 할당
        if (context.performed)
        {
            OnActionTriggered?.Invoke(context.control.displayName);
        }
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        _rawZoomInput = context.ReadValue<float>(); // 내부 변수에 저장
        if (context.performed)
        {
            OnActionTriggered?.Invoke(context.control.displayName);
        }
    }
}