using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [Tooltip("카메라가 따라다닐 대상입니다. 보통 플레이어 캐릭터를 할당합니다.")]
    public Transform target;

    [Tooltip("입력을 받아올 PlayerInputController 스크립트입니다.")]
    public PlayerInputController inputController;

    [Header("카메라 설정")]
    [Tooltip("카메라 회전 속도입니다.")]
    public float lookSpeed = 200f;

    [Tooltip("타겟으로부터의 고정 거리입니다.")]
    public float distance = 5f;

    [Tooltip("카메라의 상하 회전(Pitch) 최소 각도입니다.")]
    public float minPitch = -45f;

    [Tooltip("카메라의 상하 회전(Pitch) 최대 각도입니다.")]
    public float maxPitch = 80f;

    [Tooltip("카메라가 바라볼 타겟의 오프셋입니다. (예: 캐릭터의 머리 위치)")]
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    [Tooltip("카메라 충돌을 감지할 레이어 마스크입니다.")]
    public LayerMask obstacleMask;

    private float yaw = 0.0f; // 좌우 회전 (Y축 기준)
    private float pitch = 0.0f; // 상하 회전 (X축 기준)

    void Start()
    {
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

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
        pitch -= lookInput.y * lookSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 2. 원하는 카메라 위치 계산
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + targetOffset + (rotation * direction);

        // 3. 카메라 충돌 처리
        RaycastHit hit;
        // 플레이어 위치에서 원하는 카메라 위치까지 라인을 그려 장애물이 있는지 확인합니다.
        if (Physics.Linecast(target.position + targetOffset, desiredPosition, out hit, obstacleMask))
        {
            // 장애물이 있다면, 충돌 지점으로 카메라 위치를 조정합니다.
            transform.position = hit.point;
        }
        else
        {
            // 장애물이 없다면, 원하는 위치에 카메라를 둡니다.
            transform.position = desiredPosition;
        }

        // 4. 카메라가 항상 타겟을 바라보도록 설정
        transform.LookAt(target.position + targetOffset);
    }
}
