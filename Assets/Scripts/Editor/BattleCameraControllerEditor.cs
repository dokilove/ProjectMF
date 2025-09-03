using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BattleCameraController))]
public class BattleCameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BattleCameraController controller = (BattleCameraController)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("아래 버튼으로 각 전투 단계의 카메라 뷰를 미리 볼 수 있습니다.", MessageType.Info);
        
        EditorGUILayout.LabelField("Camera Previews", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Command View"))
        {
            controller.UpdatePreviewInEditor(BattleCameraController.CameraState.CommandFocus);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Selection View"))
        {
            controller.UpdatePreviewInEditor(BattleCameraController.CameraState.SelectionFocus);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Action View"))
        {
            controller.UpdatePreviewInEditor(BattleCameraController.CameraState.ActionFocus);
            SceneView.RepaintAll();
        }

        EditorGUILayout.EndHorizontal();
    }
}