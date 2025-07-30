using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    public PlayerInputController inputController;
    public Transform mainCamera;

    [Header("움직임 설정")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;

    [Header("점프 설정")]
    [Tooltip("캐릭터의 점프 힘입니다.")]
    public float jumpForce = 5f;

    [Tooltip("바닥을 감지할 위치입니다. 보통 캐릭터 발 아래에 둡니다.")]
    public Transform groundCheck;
    [Tooltip("바닥 감지 원의 반지름입니다.")]
    public float groundDistance = 0.2f;
    [Tooltip("바닥으로 인식할 레이어입니다.")]
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputController = GetComponent<PlayerInputController>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        inputController.OnJumpAction += HandleJump;
    }

    private void OnDisable()
    {
        inputController.OnJumpAction -= HandleJump;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 이동 및 회전 로직
        Vector2 moveInput = inputController.MoveDirection;
        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        
        Vector3 moveVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // --- 점프를 처리하는 메소드 ---
    private void HandleJump()
    {
        // 바닥에 있을 때만 점프가 가능합니다.
        if (isGrounded)
        {
            Jump();
        }
    }

    // --- 점프 동작 ---
    private void Jump()
    {
        // y축 방향으로 힘을 가해 점프합니다.
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
