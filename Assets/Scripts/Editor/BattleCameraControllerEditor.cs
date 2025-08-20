using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BattleCameraController))]
public class BattleCameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터를 그립니다.
        DrawDefaultInspector();

        BattleCameraController controller = (BattleCameraController)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("버튼을 누르면 카메라의 위치/회전이 변경되고, \n렌즈 시프트가 적용된 최종 모습은 카메라 프리뷰 창에 표시됩니다.", MessageType.Info);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Focus on Enemy (Preview)"))
        {
            // 에디터에서 카메라 프리뷰를 갱신합니다.
            controller.UpdatePreviewInEditor(BattleCameraController.CameraState.CommandFocus);
            // 씬 뷰를 업데이트하여 변경사항을 즉시 반영합니다.
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Focus on Player (Preview)"))
        {
            // 에디터에서 카메라 프리뷰를 갱신합니다.
            controller.UpdatePreviewInEditor(BattleCameraController.CameraState.ActionFocus);
            // 씬 뷰를 업데이트하여 변경사항을 즉시 반영합니다.
            SceneView.RepaintAll();
        }

        EditorGUILayout.EndHorizontal();
    }
}
