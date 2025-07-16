using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, Player_Actions.IPlayerActions
{
    private Player_Actions playerActions;

    // ���� ������Ƽ: �ٸ� ��ũ��Ʈ���� ���������� ó���� ���� �о�ϴ�.
    public Vector2 MoveDirection { get; private set; }
    public Vector2 LookDirection { get; private set; }
    public float ZoomValue { get; private set; }

    public event System.Action OnJumpAction;
    public event System.Action<string> OnActionTriggered;

    // --- ���� �߰��� �κ� ---
    // �ݹ鿡�� ���� ���� �Է� ���� �ӽ÷� ������ ���� �����Դϴ�.
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

    // LateUpdate���� �� �����Ӹ��� ���� �Է� ���¸� ���� �о�� ó���մϴ�.
    private void LateUpdate()
    {
        // 1. �� �����Ӹ��� �� �׼��� ���� ���� ���� �о�ɴϴ�. (����)
        Vector2 currentLookInput = playerActions.Player.Camera.ReadValue<Vector2>();
        float currentZoomInput = playerActions.Player.Zoom.ReadValue<float>();

        // Walk �׼ǵ� ���⼭ ���� �о�ͼ� MoveDirection�� �����մϴ�.
        MoveDirection = playerActions.Player.Walk.ReadValue<Vector2>();

        // 2. �� �Է��� �ִ��� Ȯ���մϴ�.
        bool isZooming = currentZoomInput != 0;

        // 3. �� �Է� ���ο� ���� ���� ������Ƽ�� ���� ���� �����մϴ�.
        if (isZooming)
        {
            // ���� �ϴ� ���̶��, ī�޶� ������ ���� 0���� ����� ȸ���� �����ϴ�.
            LookDirection = Vector2.zero;
        }
        else
        {
            // ���� ���� ���� ����, ���� ���콺/��ƽ ������ ���� �����մϴ�.
            LookDirection = currentLookInput;
        }

        // 4. ���� �� ���� ���� ������Ƽ�� �Ҵ��մϴ�.
        ZoomValue = currentZoomInput;
    }

    // --- �Ʒ� �ݹ� �޼ҵ���� �����Ǿ����ϴ� ---
    // ���� �ݹ鿡���� ���� ������Ƽ�� ���� �������� �ʰ�,
    // ���� ������ ���� ���� ���常 �մϴ�.

    public void OnCamera(InputAction.CallbackContext context)
    {
        _rawLookInput = context.ReadValue<Vector2>(); // ���� ������ ����
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
        MoveDirection = context.ReadValue<Vector2>(); // Move�� �ٸ� ������ �浹 �����Ƿ� ���� �Ҵ�
        if (context.performed)
        {
            OnActionTriggered?.Invoke(context.control.displayName);
        }
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        _rawZoomInput = context.ReadValue<float>(); // ���� ������ ����
        if (context.performed)
        {
            OnActionTriggered?.Invoke(context.control.displayName);
        }
    }
}