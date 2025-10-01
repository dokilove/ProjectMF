using UnityEngine;
using TMPro;

public class InputDisplayUI : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [Tooltip("입력을 받아올 PlayerInputController 스크립트입니다.")]
    public PlayerInputController inputController;

    [Header("UI 설정")]
    [Tooltip("로그 텍스트 UI의 프리팹입니다.")]
    public GameObject textPrefab;

    [Tooltip("로그 텍스트 UI가 위치할 부모 컨테이너입니다. (Vertical Layout Group이 있는 Panel)")]
    public Transform container;

    [Tooltip("텍스트가 화면에 표시될 시간(초)입니다.")]
    public float lifeTime = 0.5f;

    private void OnEnable()
    {
        if (inputController != null)
        {
            // PlayerInputController의 새 이벤트에 구독합니다.
            inputController.OnActionForDisplay += ShowInputLog;
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            // 스크립트가 비활성화될 때 이벤트 구독 해제 (메모리 누수 방지)
            inputController.OnActionForDisplay -= ShowInputLog;
        }
    }

    private void ShowInputLog(string actionName)
    {
        if (textPrefab == null || container == null) return;

        // 1. 프리팹으로부터 새로운 로그 텍스트 게임 오브젝트를 생성하고, 컨테이너의 자식으로 넣습니다.
        GameObject newLogText = Instantiate(textPrefab, container);

        // 2. 생성된 오브젝트에서 TextMeshProUGUI 컴포넌트를 가져와 텍스트를 설정합니다.
        var textComponent = newLogText.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = actionName;
        }

        // 3. lifeTime(0.5초) 이후에 생성된 로그 텍스트 오브젝트를 파괴합니다.
        Destroy(newLogText, lifeTime);
    }
}
