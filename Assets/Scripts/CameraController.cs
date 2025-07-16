using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("�ʼ� ������Ʈ")]
    [Tooltip("ī�޶� ����ٴ� ����Դϴ�. ���� �÷��̾� ĳ���͸� �Ҵ��մϴ�.")]
    public Transform target;

    [Tooltip("�Է��� �޾ƿ��� PlayerInputController ��ũ��Ʈ�Դϴ�.")]
    public PlayerInputController inputController;

    [Header("ī�޶� ����")]
    [Tooltip("ī�޶� ȸ�� �ӵ��Դϴ�.")]
    public float lookSpeed = 200f;

    [Tooltip("ī�޶� �� �ӵ��Դϴ�.")]
    public float zoomSpeed = 10f;

    [Tooltip("Ÿ�ٰ��� �ּ� �Ÿ� (�� �� �ִ�)")]
    public float minDistance = 1f;

    [Tooltip("Ÿ�ٰ��� �ִ� �Ÿ� (�� �ƿ� �ִ�)")]
    public float maxDistance = 10f;

    [Tooltip("ī�޶��� ���� ȸ��(Pitch) �ּ� �����Դϴ�.")]
    public float minPitch = -45f;

    [Tooltip("ī�޶��� ���� ȸ��(Pitch) �ִ� �����Դϴ�.")]
    public float maxPitch = 80f;

    [Tooltip("ī�޶� �ٶ� Ÿ���� �������Դϴ�. (��: ĳ������ �Ӹ� ��ġ)")]
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

    // ���� ����
    private float currentDistance;
    private float yaw = 0.0f; // ���� ȸ�� (Y�� ����)
    private float pitch = 0.0f; // ���� ȸ�� (X�� ����)

    void Start()
    {
        // �ʱ� �Ÿ� ����
        currentDistance = (minDistance + maxDistance) / 2;

        // ���� �� ī�޶��� �ʱ� ȸ�� ���� ���� Ÿ���� �ٶ󺸵��� ����
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }

        // ���콺 Ŀ�� ����� �� ���
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update�� ���� �� ����Ǿ�, ĳ������ �������� ��� ���� �� ī�޶� ���󰡵��� �մϴ�.
    void LateUpdate()
    {
        if (target == null || inputController == null)
        {
            Debug.LogWarning("ī�޶� Ÿ�� �Ǵ� �Է� ��Ʈ�ѷ��� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // 1. ī�޶� ȸ�� (���콺 �Է�)
        Vector2 lookInput = inputController.LookDirection;
        yaw += lookInput.x * lookSpeed * Time.deltaTime;
        pitch -= lookInput.y * lookSpeed * Time.deltaTime; // Y�� �Է��� �������Ѿ� �ڿ��������ϴ�.
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // ���� ���� ����

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 2. ī�޶� �� (���콺 ��ũ��)
        float zoomInput = inputController.ZoomValue;
        if (zoomInput != 0)
        {
            // ��ũ�� ���� ���⸸ ��� (��ũ�� ���� ���� 120 ������ ������ ����)
            currentDistance -= Mathf.Sign(zoomInput) * zoomSpeed * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        // 3. ī�޶� ��ġ ���
        // Ÿ�� ��ġ���� ȸ�� �������� �Ÿ���ŭ ������ ��ġ�� ����մϴ�.
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        Vector3 desiredPosition = target.position + targetOffset + (rotation * direction);

        // 4. ī�޶� ��ġ�� ȸ�� ����
        transform.position = desiredPosition;
        transform.LookAt(target.position + targetOffset);
    }
}