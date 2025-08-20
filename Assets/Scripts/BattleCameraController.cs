using UnityEngine;

[ExecuteAlways] // 에디터 모드에서도 실행되도록 설정
public class BattleCameraController : MonoBehaviour
{
    public enum CameraState { Idle, CommandFocus, ActionFocus }
    private CameraState currentState = CameraState.Idle;
    private Camera battleCamera;

    [Header("타겟 설정")]
    public Transform playerTarget;
    public Transform enemyTarget;

    [Header("카메라 공통 설정")]
    public float smoothSpeed = 0.05f;

    [Header("Command 상태 설정")]
    [Tooltip("적으로부터의 상대 위치")]
    public Vector3 commandStateLocalOffset = new Vector3(0, 4, -10);
    [Tooltip("적이 바라보이는 높이(Y) 오프셋")]
    public float enemyLookAtHeight = 1.5f;
    [Tooltip("화면 중심 이동 (Vanishing Point Offset)")]
    public Vector2 commandLensShift = Vector2.zero;

    [Header("Action 상태 설정")]
    [Tooltip("플레이어로부터의 상대 위치")]
    public Vector3 actionStateLocalOffset = new Vector3(1, 2, -5);
    [Tooltip("플레이어가 바라보이는 높이(Y) 오프셋")]
    public float playerLookAtHeight = 1.5f;
    [Tooltip("화면 중심 이동 (Vanishing Point Offset)")]
    public Vector2 actionLensShift = Vector2.zero;

    private Vector2 currentLensShift;

    void Awake()
    {
        battleCamera = GetComponent<Camera>();
        if (battleCamera == null) battleCamera = Camera.main;
        if (battleCamera == null)
        {
            Debug.LogError("씬에 카메라가 없습니다.");
            this.enabled = false;
        }
    }

    void LateUpdate()
    {
        // 게임 실행 중에만 자동으로 카메라 로직을 실행합니다.
        if (!Application.isPlaying) return;

        Vector2 targetLensShift = Vector2.zero;

        switch (currentState)
        {
            case CameraState.CommandFocus:
                if (enemyTarget == null) return;
                HandleCameraPositionLogic(enemyTarget, commandStateLocalOffset, enemyLookAtHeight, true);
                targetLensShift = commandLensShift;
                break;

            case CameraState.ActionFocus:
                if (playerTarget == null) return;
                HandleCameraPositionLogic(playerTarget, actionStateLocalOffset, playerLookAtHeight, true);
                targetLensShift = actionLensShift;
                break;
        }

        ApplyVanishingPointOffset(targetLensShift, true);
    }

    // isSmooth 파라미터를 추가하여 부드러운 움직임 여부를 제어합니다.
    private void HandleCameraPositionLogic(Transform target, Vector3 localOffset, float lookAtHeight, bool isSmooth)
    {
        Vector3 desiredPosition = target.position + target.right * localOffset.x + target.up * localOffset.y + target.forward * localOffset.z;
        transform.position = isSmooth ? Vector3.Lerp(transform.position, desiredPosition, smoothSpeed) : desiredPosition;

        Vector3 lookAtPoint = target.position + (Vector3.up * lookAtHeight);
        Quaternion lookAtTarget = Quaternion.LookRotation(lookAtPoint - transform.position);
        transform.rotation = isSmooth ? Quaternion.Slerp(transform.rotation, lookAtTarget, smoothSpeed) : lookAtTarget;
    }

    private void ApplyVanishingPointOffset(Vector2 targetOffset, bool isSmooth)
    {
        currentLensShift = isSmooth ? Vector2.Lerp(currentLensShift, targetOffset, smoothSpeed) : targetOffset;
        
        battleCamera.ResetProjectionMatrix();
        Matrix4x4 matrix = battleCamera.projectionMatrix;
        matrix.m02 = currentLensShift.x;
        matrix.m12 = currentLensShift.y;
        battleCamera.projectionMatrix = matrix;
    }

    // 에디터 스크립트에서 호출할 공개 메소드
    public void UpdatePreviewInEditor(CameraState previewState)
    {
        if (Application.isPlaying) return;

        // 에디터에서 호출 시에는 부드러운 움직임 없이 즉시 적용
        switch (previewState)
        {
            case CameraState.CommandFocus:
                if (enemyTarget == null) { Debug.LogWarning("Enemy Target이 설정되지 않았습니다."); return; }
                HandleCameraPositionLogic(enemyTarget, commandStateLocalOffset, enemyLookAtHeight, false);
                ApplyVanishingPointOffset(commandLensShift, false);
                break;
            case CameraState.ActionFocus:
                if (playerTarget == null) { Debug.LogWarning("Player Target이 설정되지 않았습니다."); return; }
                HandleCameraPositionLogic(playerTarget, actionStateLocalOffset, playerLookAtHeight, false);
                ApplyVanishingPointOffset(actionLensShift, false);
                break;
        }
    }

    public void FocusOnPlayer()
    {
        currentState = CameraState.ActionFocus;
    }

    public void FocusOnEnemy()
    {
        currentState = CameraState.CommandFocus;
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyTarget != null)
        {
            Gizmos.color = Color.red;
            Vector3 commandCamPos = enemyTarget.position + enemyTarget.right * commandStateLocalOffset.x + enemyTarget.up * commandStateLocalOffset.y + enemyTarget.forward * commandStateLocalOffset.z;
            Vector3 enemyLookAtPos = enemyTarget.position + Vector3.up * enemyLookAtHeight;
            DrawCameraGizmo(commandCamPos, enemyLookAtPos);
        }

        if (playerTarget != null)
        {
            Gizmos.color = Color.green;
            Vector3 actionCamPos = playerTarget.position + playerTarget.right * actionStateLocalOffset.x + playerTarget.up * actionStateLocalOffset.y + playerTarget.forward * actionStateLocalOffset.z;
            Vector3 playerLookAtPos = playerTarget.position + Vector3.up * playerLookAtHeight;
            DrawCameraGizmo(actionCamPos, playerLookAtPos);
        }
    }

    private void DrawCameraGizmo(Vector3 cameraPosition, Vector3 targetPosition)
    {
        Gizmos.DrawWireSphere(cameraPosition, 0.5f);
        Gizmos.DrawLine(cameraPosition, targetPosition);
    }
}
