using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �ʿ��մϴ�.

public class InputDisplayUI : MonoBehaviour
{
    [Header("�ʼ� ������Ʈ")]
    [Tooltip("�Է��� �޾ƿ� PlayerInputController ��ũ��Ʈ�Դϴ�.")]
    public PlayerInputController inputController;

    [Header("UI ����")]
    [Tooltip("������ �ؽ�Ʈ UI�� �������Դϴ�.")]
    public GameObject textPrefab;

    [Tooltip("������ �ؽ�Ʈ UI�� ��ġ�� �θ� �����̳��Դϴ�. (Vertical Layout Group�� �ִ� Panel)")]
    public Transform container;

    [Tooltip("�ؽ�Ʈ�� ȭ�鿡 ǥ�õ� �ð�(��)�Դϴ�.")]
    public float lifeTime = 0.5f;

    private void OnEnable()
    {
        if (inputController != null)
        {
            // PlayerInputController�� �̺�Ʈ�� �߻��� ������ ShowInputLog �޼ҵ带 ȣ���ϵ��� ���
            inputController.OnActionTriggered += ShowInputLog;
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            // ��ũ��Ʈ�� ��Ȱ��ȭ�� �� �̺�Ʈ ��� ���� (�޸� ���� ����)
            inputController.OnActionTriggered -= ShowInputLog;
        }
    }

    private void ShowInputLog(string actionName)
    {
        if (textPrefab == null || container == null) return;

        // 1. ���������κ��� ���ο� �ؽ�Ʈ ���� ������Ʈ�� �����ϰ�, �����̳��� �ڽ����� ����ϴ�.
        GameObject newLogText = Instantiate(textPrefab, container);

        // 2. ������ ������Ʈ���� TextMeshProUGUI ������Ʈ�� ������ �ؽ�Ʈ�� �����մϴ�.
        var textComponent = newLogText.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = actionName;
        }

        // 3. lifeTime(0.5��) �Ŀ� ������ �ؽ�Ʈ ������Ʈ�� �ı��մϴ�.
        Destroy(newLogText, lifeTime);
    }
}