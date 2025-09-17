using UnityEngine;
using UnityEngine.UIElements;

public class InputDisplayController : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [Tooltip("입력을 받아올 PlayerInputController 스크립트입니다.")]
    public PlayerInputController inputController;

    [Tooltip("UI를 표시할 UIDocument 입니다.")]
    public UIDocument uiDocument;

    [Header("UI 설정")]
    [Tooltip("텍스트가 화면에 표시될 시간(초)입니다.")]
    public float lifeTime = 0.5f;

    private VisualElement m_LogContainer;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument가 할당되지 않았습니다.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        m_LogContainer = root.Q<VisualElement>("LogContainer");

        if (m_LogContainer == null)
        {
            Debug.LogError("UXML에 'LogContainer'라는 이름의 VisualElement가 없습니다.");
            return;
        }

        if (inputController != null)
        {
            inputController.OnActionForDisplay += ShowInputLog;
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            inputController.OnActionForDisplay -= ShowInputLog;
        }
    }

    private void ShowInputLog(string actionName)
    {
        if (m_LogContainer == null) return;

        // 1. 새로운 Label VisualElement를 생성합니다.
        var newLog = new Label(actionName);

        // 2. USS에 정의된 스타일 클래스를 적용합니다.
        newLog.AddToClassList("log-entry");

        // 3. 컨테이너에 자식으로 추가합니다.
        m_LogContainer.Add(newLog);

        // 4. lifeTime 이후에 컨테이너에서 제거되도록 예약합니다.
        long lifeTimeMs = (long)(lifeTime * 1000);
        newLog.schedule.Execute(() => 
        {
            // 엘리먼트가 여전히 부모 컨테이너에 속해 있을 경우에만 제거
            if (newLog != null && newLog.parent == m_LogContainer)
            {
                m_LogContainer.Remove(newLog);
            }
        }).StartingIn(lifeTimeMs);
    }
}
