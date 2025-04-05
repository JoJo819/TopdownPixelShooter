using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform player; // Целевой объект игрока
    public float followSpeed = 5f; // Скорость плавного следования за игроком
    public float cursorMoveSpeed = 8f; // Скорость плавного смещения к курсору
    public float maxCursorOffset = 3f; // Максимальное смещение камеры относительно игрока

    private Vector3 targetPosition; // Целевая позиция камеры

    void Start()
    {
        // Начальная позиция камеры совпадает с игроком
        targetPosition = player.position;
    }

    void Update()
    {
        Vector3 cursorWorldPosition = GetMouseWorldPosition();
        targetPosition = player.position + Vector3.ClampMagnitude(cursorWorldPosition - player.position, maxCursorOffset);
    }

    void LateUpdate()
    {
        // Плавно перемещаем камеру к целевой позиции
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Преобразуем экранные координаты курсора в мировые координаты
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(player.position).z; // Учитываем расстояние до камеры
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}