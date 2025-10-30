
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [Header("카메라")]
    public Camera commandCamera;
    public Camera selectionCamera;
    public Camera actionCamera;

    // BattleManager에서 설정할 플레이어 캐릭터 트랜스폼
    public Transform playerTransform; 

    [Header("Command 카메라 설정") ]
    [Tooltip("Command 카메라가 바라볼 대상 (런타임에 BattleManager가 설정)")]
    public Transform commandTarget;
    [Tooltip("Command 카메라 미리보기를 위한 기본 대상 (에디터에서 설정)")]
    public Transform defaultCommandTarget; // Added for Editor preview
    [Tooltip("대상을 바라볼 때의 높이 오프셋")]
    public float commandLookAtHeight = 1.5f;
    [Tooltip("카메라 회전의 부드러움 정도")]
    public float smoothSpeed = 5f;

    [Header("Action 카메라 설정") ]
    [Tooltip("Action 카메라가 플레이어로부터 떨어질 거리 및 각도")]
    public Vector3 actionCameraOffset = new Vector3(0, 3, -5); // 뒤쪽 위
    [Tooltip("Action 카메라가 플레이어를 따라가는 속도")]
    public float actionCameraFollowSpeed = 5f;
    [Tooltip("Action 카메라가 플레이어를 바라보는 속도")]
    public float actionCameraLookSpeed = 5f;
    [Tooltip("Action 카메라가 플레이어를 바라볼 때의 높이 오프셋")]
    public float actionCameraLookAtHeight = 1.5f; // Added
    [Tooltip("Spring Arm의 시작점 (플레이어의 자식 오브젝트 권장)")]
    public Transform springArmPivot; // Spring Arm의 시작점 (플레이어의 자식 오브젝트 권장)
    [Tooltip("카메라와 플레이어 사이에 장애물이 있을 때 카메라가 얼마나 가까이 당겨질지")]
    public float springArmMinDistance = 1f; // 카메라가 플레이어에게 당겨질 최소 거리
    [Tooltip("Spring Arm 레이캐스트에 포함될 레이어 마스크")]
    public LayerMask springArmObstructionLayers; // 장애물 레이어 마스크

    void Start()
    {
        // 시작 시 Command 카메라만 활성화
        SwitchToCommandView();
    }

    void LateUpdate()
    {
        // Command 카메라가 활성화되어 있고 타겟이 지정된 경우에만 실행
        if (commandCamera != null && commandCamera.gameObject.activeInHierarchy && commandTarget != null)
        {
            Transform camTransform = commandCamera.transform;

            // 목표 지점 계산 (카메라가 바라볼 실제 위치)
            Vector3 lookAtPoint = commandTarget.position + Vector3.up * commandLookAtHeight;

            // 카메라의 로컬 X축(좌우)으로 얼마나 이동해야 타겟이 중앙에 오는지 계산합니다.
            // 1. 카메라에서 타겟까지의 벡터를 구합니다.
            Vector3 toTarget = lookAtPoint - camTransform.position;
            // 2. 이 벡터를 카메라의 오른쪽(right) 벡터에 투영(Dot)하여 X축 방향의 차이를 구합니다.
            float xDifference = Vector3.Dot(toTarget, camTransform.right);

            // 3. 현재 위치에서 계산된 차이만큼 오른쪽 벡터 방향으로 이동한 위치가 목표 위치입니다.
            Vector3 desiredPosition = camTransform.position + camTransform.right * xDifference;

            // 부드럽게 위치를 이동시킵니다.
            camTransform.position = Vector3.Lerp(camTransform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        }
        // Action 카메라가 활성화되어 있고 플레이어 트랜스폼이 지정된 경우에만 실행
        else if (actionCamera != null && actionCamera.gameObject.activeInHierarchy && playerTransform != null)
        {
            Transform camTransform = actionCamera.transform;

            // Spring Arm Pivot이 없으면 플레이어 위치를 사용
            Transform pivot = springArmPivot != null ? springArmPivot : playerTransform;

            // 목표 카메라 위치 계산 (플레이어 뒤쪽 위)
            Vector3 idealPosition = pivot.position + playerTransform.TransformDirection(actionCameraOffset);
            Vector3 currentTargetPosition = idealPosition;

            // Spring Arm Raycast
            RaycastHit hit;
            if (Physics.Linecast(pivot.position, idealPosition, out hit, springArmObstructionLayers))
            {
                // 장애물이 있으면 카메라를 장애물 앞쪽으로 당깁니다.
                currentTargetPosition = hit.point + (pivot.position - idealPosition).normalized * springArmMinDistance;
            }

            // 카메라 위치 부드럽게 이동
            camTransform.position = Vector3.Lerp(camTransform.position, currentTargetPosition, Time.deltaTime * actionCameraFollowSpeed);

            // 목표 카메라 회전 계산 (플레이어를 바라보도록)
            Vector3 lookAtPoint = playerTransform.position + Vector3.up * actionCameraLookAtHeight; // Modified
            Quaternion desiredRotation = Quaternion.LookRotation(lookAtPoint - camTransform.position); // Modified
            camTransform.rotation = Quaternion.Slerp(camTransform.rotation, desiredRotation, Time.deltaTime * actionCameraLookSpeed);
        }
    }

    public void SwitchToCommandView()
    {
        if (commandCamera != null) commandCamera.gameObject.SetActive(true);
        if (selectionCamera != null) selectionCamera.gameObject.SetActive(false);
        if (actionCamera != null) actionCamera.gameObject.SetActive(false);
    }

    public void SwitchToSelectionView()
    {
        if (commandCamera != null) commandCamera.gameObject.SetActive(false);
        if (selectionCamera != null) selectionCamera.gameObject.SetActive(true);
        if (actionCamera != null) actionCamera.gameObject.SetActive(false);
    }

    public void SwitchToActionView()
    {
        if (commandCamera != null) commandCamera.gameObject.SetActive(false);
        if (selectionCamera != null) selectionCamera.gameObject.SetActive(false);
        if (actionCamera != null) actionCamera.gameObject.SetActive(true);
    }
}
