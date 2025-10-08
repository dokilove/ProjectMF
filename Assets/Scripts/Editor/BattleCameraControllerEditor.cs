
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleCameraController))]
public class BattleCameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 UI를 그립니다.
        base.OnInspectorGUI();

        // 타겟 스크립트의 참조를 가져옵니다.
        BattleCameraController cameraController = (BattleCameraController)target;

        // 에디터 UI에 약간의 공간을 추가합니다.
        EditorGUILayout.Space();

        // 미리보기 버튼을 위한 제목을 추가합니다.
        EditorGUILayout.LabelField("에디터 카메라 미리보기", EditorStyles.boldLabel);

        // 버튼들을 가로로 배열합니다.
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Command View"))
        {
            // Command View 버튼이 눌렸을 때
            // Undo를 기록하여 에디터에서 변경 사항을 되돌릴 수 있도록 합니다.
            if(cameraController.commandCamera != null) Undo.RecordObject(cameraController.commandCamera.gameObject, "Enable Command Camera");
            if(cameraController.selectionCamera != null) Undo.RecordObject(cameraController.selectionCamera.gameObject, "Disable Selection Camera");
            if(cameraController.actionCamera != null) Undo.RecordObject(cameraController.actionCamera.gameObject, "Disable Action Camera");

            cameraController.SwitchToCommandView();
            
            // 씬 뷰를 업데이트하여 변경 사항을 즉시 반영합니다.
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Selection View"))
        {
            // Selection View 버튼이 눌렸을 때
            if(cameraController.commandCamera != null) Undo.RecordObject(cameraController.commandCamera.gameObject, "Disable Command Camera");
            if(cameraController.selectionCamera != null) Undo.RecordObject(cameraController.selectionCamera.gameObject, "Enable Selection Camera");
            if(cameraController.actionCamera != null) Undo.RecordObject(cameraController.actionCamera.gameObject, "Disable Action Camera");

            cameraController.SwitchToSelectionView();
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Action View"))
        {
            // Action View 버튼이 눌렸을 때
            if(cameraController.commandCamera != null) Undo.RecordObject(cameraController.commandCamera.gameObject, "Disable Command Camera");
            if(cameraController.selectionCamera != null) Undo.RecordObject(cameraController.selectionCamera.gameObject, "Disable Selection Camera");
            if(cameraController.actionCamera != null) Undo.RecordObject(cameraController.actionCamera.gameObject, "Enable Action Camera");

            cameraController.SwitchToActionView();
            SceneView.RepaintAll();
        }

        EditorGUILayout.EndHorizontal();
    }
}
