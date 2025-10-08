
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [Header("카메라")]
    public Camera commandCamera;
    public Camera selectionCamera;
    public Camera actionCamera;

    [Header("Command 카메라 설정")]
    [Tooltip("Command 카메라가 바라볼 대상")]
    public Transform commandTarget;
    [Tooltip("대상을 바라볼 때의 높이 오프셋")]
    public float commandLookAtHeight = 1.5f;
    [Tooltip("카메라 회전의 부드러움 정도")]
    public float smoothSpeed = 5f;

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
