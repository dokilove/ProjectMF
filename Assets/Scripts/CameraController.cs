using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [Tooltip("카메라가 따라다닐 대상입니다. 보통 플레이어 캐릭터를 할당합니다.")]
    public Transform target;

    [Tooltip("입력을 받아오는 PlayerInputController 스크립트입니다.")]
    public PlayerInputController inputController;

    [Header("카메라 설정")]
    [Tooltip("카메라 회전 속도입니다.")]
    public float lookSpeed = 200f;

    [Tooltip("카메라 줌 속도입니다.")]
    public float zoomSpeed = 10f;

    [Tooltip("타겟과의 최소 거리 (줌 인 최대)")]
    public float minDistance = 1f;

    [Tooltip("타겟과의 최대 거리 (줌 아웃 최대)")]
    public float maxDistance = 10f;

    [Tooltip("카메라의 수직 회전(Pitch) 최소 각도입니다.")]
    public float minPitch = -45f;

    [Tooltip("카메라의 수직 회전(Pitch) 최대 각도입니다.")]
    public float maxPitch = 80f;

    [Tooltip("카메라가 바라볼 타겟의 오프셋입니다. (예: 캐릭터의 머리 위치)")]
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    // 내부 변수
    private float currentDistance;
    private float yaw = 0.0f; // 수평 회전 (Y축 기준)
    private float pitch = 0.0f; // 수직 회전 (X축 기준)

    void Start()
    {
        // 초기 거리 설정
        currentDistance = (minDistance + maxDistance) / 2;

        // 시작 시 카메라의 초기 회전 값을 현재 타겟을 바라보도록 설정
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }

        // 마우스 커서 숨기기 및 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update가 끝난 후 실행되어, 캐릭터의 움직임이 모두 끝난 후 카메라가 따라가도록 합니다.
    void LateUpdate()
    {
        if (target == null || inputController == null)
        {
            Debug.LogWarning("카메라 타겟 또는 입력 컨트롤러가 할당되지 않았습니다.");
            return;
        }

        // 1. 카메라 회전 (마우스 입력)
        Vector2 lookInput = inputController.LookDirection;
        yaw += lookInput.x * lookSpeed * Time.deltaTime;
        pitch -= lookInput.y * lookSpeed * Time.deltaTime; // Y축 입력은 반전시켜야 자연스럽습니다.
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // 수직 각도 제한

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 2. 카메라 줌 (마우스 스크롤)
        float zoomInput = inputController.ZoomValue;
        if (zoomInput != 0)
        {
            // 스크롤 값의 방향만 사용 (스크롤 값은 보통 120 단위로 들어오기 때문)
            currentDistance -= Mathf.Sign(zoomInput) * zoomSpeed * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        // 3. 카메라 위치 계산
        // 타겟 위치에서 회전 방향으로 거리만큼 떨어진 위치를 계산합니다.
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        Vector3 desiredPosition = target.position + targetOffset + (rotation * direction);

        // 4. 카메라 위치와 회전 적용
        transform.position = desiredPosition;
        transform.LookAt(target.position + targetOffset);
    }
}