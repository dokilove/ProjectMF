using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleCameraController))]
public class BattleCameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        BattleCameraController controller = (BattleCameraController)target;

        GUILayout.Space(10);
        GUILayout.Label("Camera Preview Controls", EditorStyles.boldLabel);

        // Add a button to preview Command Camera
        if (GUILayout.Button("Preview Command Camera"))
        {
            Transform targetForPreview = controller.commandTarget != null ? controller.commandTarget : controller.defaultCommandTarget;

            if (controller.commandCamera != null && targetForPreview != null)
            {
                controller.SwitchToCommandView();
                // Simulate LateUpdate logic for Command Camera
                Transform camTransform = controller.commandCamera.transform;
                Vector3 lookAtPoint = targetForPreview.position + Vector3.up * controller.commandLookAtHeight;
                Vector3 toTarget = lookAtPoint - camTransform.position;
                float xDifference = Vector3.Dot(toTarget, camTransform.right);
                Vector3 desiredPosition = camTransform.position + camTransform.right * xDifference;
                camTransform.position = desiredPosition; // Snap to position for preview

                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning("Command Camera 또는 Command Target/Default Command Target이 설정되지 않아 미리보기를 할 수 없습니다.");
            }
        }

        // Add a button to preview Selection Camera
        if (GUILayout.Button("Preview Selection Camera"))
        {
            if (controller.selectionCamera != null)
            {
                controller.SwitchToSelectionView();
                // Selection Camera는 고정된 위치에 있을 것이므로 추가적인 로직은 필요 없습니다.
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning("Selection Camera가 설정되지 않아 미리보기를 할 수 없습니다.");
            }
        }

        // Add a button to preview Action Camera
        if (GUILayout.Button("Preview Action Camera"))
        {
            if (controller.actionCamera != null && controller.playerTransform != null)
            {
                controller.SwitchToActionView();
                // Simulate LateUpdate logic for Action Camera
                Transform camTransform = controller.actionCamera.transform;

                // Spring Arm Pivot이 없으면 플레이어 위치를 사용
                Transform pivot = controller.springArmPivot != null ? controller.springArmPivot : controller.playerTransform;

                // 목표 카메라 위치 계산 (플레이어 뒤쪽 위)
                Vector3 idealPosition = pivot.position + controller.playerTransform.TransformDirection(controller.actionCameraOffset);
                Vector3 currentTargetPosition = idealPosition;

                // Spring Arm Raycast
                RaycastHit hit;
                if (Physics.Linecast(pivot.position, idealPosition, out hit, controller.springArmObstructionLayers))
                {
                    // 장애물이 있으면 카메라를 장애물 앞쪽으로 당깁니다.
                    currentTargetPosition = hit.point + (pivot.position - idealPosition).normalized * controller.springArmMinDistance;
                    Debug.Log($"[Preview Action Camera] Raycast hit! Obstruction at {hit.point}. Camera pulled to {currentTargetPosition}");
                }
                else
                {
                    Debug.Log($"[Preview Action Camera] No Raycast hit. Ideal position: {idealPosition}. Current target position: {currentTargetPosition}");
                }

                // 카메라 위치 설정
                camTransform.position = currentTargetPosition;

                // 목표 카메라 회전 계산 (플레이어를 바라보도록)
                Vector3 lookAtPoint = controller.playerTransform.position + Vector3.up * controller.actionCameraLookAtHeight; // Added
                Quaternion desiredRotation = Quaternion.LookRotation(lookAtPoint - camTransform.position); // Modified
                camTransform.rotation = desiredRotation;

                Debug.Log($"[Preview Action Camera] Player Position: {controller.playerTransform.position}");
                Debug.Log($"[Preview Action Camera] Player Rotation: {controller.playerTransform.rotation.eulerAngles}");
                Debug.Log($"[Preview Action Camera] Action Camera Offset: {controller.actionCameraOffset}");
                Debug.Log($"[Preview Action Camera] Ideal Position: {idealPosition}");
                Debug.Log($"[Preview Action Camera] Final Camera Position: {camTransform.position}");
                Debug.Log($"[Preview Action Camera] Final Camera Rotation: {camTransform.rotation.eulerAngles}");


                // Scene 뷰 업데이트 강제
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning("Action Camera 또는 Player Transform이 설정되지 않아 미리보기를 할 수 없습니다.");
            }
        }
    }
}