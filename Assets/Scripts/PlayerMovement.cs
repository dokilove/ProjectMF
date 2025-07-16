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

    // --- 점프 관련 설정이 추가되었습니다 ---
    [Header("점프 설정")]
    [Tooltip("캐릭터의 점프 힘입니다.")]
    public float jumpForce = 7f;

    [Tooltip("바닥을 감지할 위치입니다. 보통 캐릭터 발 아래에 둡니다.")]
    public Transform groundCheck;

    [Tooltip("바닥 감지 원의 반지름입니다.")]
    public float groundDistance = 0.2f;

    [Tooltip("바닥으로 인식할 레이어입니다.")]
    public LayerMask groundMask;
    // ------------------------------------

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

    // --- 이벤트 구독/해제 부분이 추가되었습니다 ---
    private void OnEnable()
    {
        // PlayerInputController의 점프 이벤트가 발생하면 HandleJump 메소드를 호출하도록 등록합니다.
        inputController.OnJumpAction += HandleJump;
    }

    private void OnDisable()
    {
        // 스크립트가 비활성화될 때 이벤트 등록을 해제합니다. (메모리 누수 방지)
        inputController.OnJumpAction -= HandleJump;
    }
    // ------------------------------------------

    private void FixedUpdate()
    {
        // 바닥 감지
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 이동 및 회전 로직 (이전과 동일)
        Vector2 moveInput = inputController.MoveDirection;
        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        // Rigidbody를 사용할 때는 transform.forward 대신 moveDirection으로 속도를 계산하는 것이 더 정확할 수 있습니다.
        Vector3 moveVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // --- 점프를 처리하는 메소드가 추가되었습니다 ---
    private void HandleJump()
    {
        // 바닥에 있을 때만 점프가 가능합니다.
        if (isGrounded)
        {
            // y축 속도를 0으로 만들어, 내려오던 힘이 점프에 영향을 주지 않게 합니다.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            // 위쪽으로 순간적인 힘을 가합니다.
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    // ------------------------------------------
}