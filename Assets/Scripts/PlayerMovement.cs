using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

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

    [Header("회피 설정")]
    [Tooltip("캐릭터의 뒤로 회피하는 힘입니다.")]
    public float dashForce = 10f;
    [Tooltip("회피가 지속되는 시간입니다.")]
    public float dashTime = 0.2f;

    [Tooltip("바닥을 감지할 위치입니다. 보통 캐릭터 발 아래에 둡니다.")]
    public Transform groundCheck;
    [Tooltip("바닥 감지 원의 반지름입니다.")]
    public float groundDistance = 0.2f;
    [Tooltip("바닥으로 인식할 레이어입니다.")]
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isDashing = false; // 회피 상태를 저장할 변수

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
        inputController.OnJumpAction += HandleDash;
    }

    private void OnDisable()
    {
        inputController.OnJumpAction -= HandleDash;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 회피 중일 때는 이동 및 회전 입력을 무시합니다.
        if (isDashing)
        {
            return;
        }

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

    // --- 회피를 처리하는 메소드 ---
    private void HandleDash()
    {
        // 바닥에 있고, 현재 다른 회피 동작 중이 아닐 때만 회피가 가능합니다.
        if (isGrounded && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    // --- 회피 동작을 위한 코루틴 ---
    private IEnumerator Dash()
    {
        isDashing = true;

        // Rigidbody의 속도를 직접 변경하여 미끄러지듯이 뒤로 이동시킵니다.
        Vector3 dashVelocity = -transform.forward * dashForce;
        rb.linearVelocity = new Vector3(dashVelocity.x, rb.linearVelocity.y, dashVelocity.z);

        // dashTime 만큼 대기합니다.
        yield return new WaitForSeconds(dashTime);

        // 회피가 끝난 후 속도를 초기화하여 미끄러짐을 멈춥니다.
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        isDashing = false;
    }
}
