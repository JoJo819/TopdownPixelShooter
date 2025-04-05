using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform player; // ������� ������ ������
    public float followSpeed = 5f; // �������� �������� ���������� �� �������
    public float cursorMoveSpeed = 8f; // �������� �������� �������� � �������
    public float maxCursorOffset = 3f; // ������������ �������� ������ ������������ ������

    private Vector3 targetPosition; // ������� ������� ������

    void Start()
    {
        // ��������� ������� ������ ��������� � �������
        targetPosition = player.position;
    }

    void Update()
    {
        Vector3 cursorWorldPosition = GetMouseWorldPosition();
        targetPosition = player.position + Vector3.ClampMagnitude(cursorWorldPosition - player.position, maxCursorOffset);
    }

    void LateUpdate()
    {
        // ������ ���������� ������ � ������� �������
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    private Vector3 GetMouseWorldPosition()
    {
        // ����������� �������� ���������� ������� � ������� ����������
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(player.position).z; // ��������� ���������� �� ������
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}