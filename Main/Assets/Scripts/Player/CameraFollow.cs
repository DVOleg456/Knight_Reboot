using UnityEngine;

/// <summary>
/// Камера следует за игроком плавно и с настраиваемым смещением
/// Урок 1: Основы работы с камерой в 2D игре
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // === НАСТРОЙКИ (видны в Inspector) ===

    [Header("Цель слежения")]
    [SerializeField] private Transform target; // За кем следить (Player)

    [Header("Настройки следования")]
    [SerializeField] private float smoothSpeed = 0.125f; // Скорость сглаживания (0.1 - медленно, 1 - мгновенно)
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Смещение от игрока (Z=-10 для 2D)

    [Header("Ограничения камеры (опционально)")]
    [SerializeField] private bool useBounds = false; // Использовать границы карты?
    [SerializeField] private float minX = -50f; // Минимальная позиция X
    [SerializeField] private float maxX = 50f;  // Максимальная позиция X
    [SerializeField] private float minY = -50f; // Минимальная позиция Y
    [SerializeField] private float maxY = 50f;  // Максимальная позиция Y

    // === ИНИЦИАЛИЗАЦИЯ ===

    private void Start()
    {
        // Если цель не назначена, автоматически находим Player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("CameraFollow: Автоматически найден Player");
            }
            else
            {
                Debug.LogError("CameraFollow: Player не найден! Добавь тег 'Player' на объект игрока");
            }
        }

        // Мгновенно телепортируем камеру к игроку при старте игры
        // Без этого камера будет медленно плыть от стартовой позиции
        if (target != null)
        {
            SnapToTarget();
            Debug.Log($"CameraFollow: Камера телепортирована к игроку на позицию {target.position}");
        }
        else
        {
            Debug.LogError("CameraFollow: Target НЕ НАЗНАЧЕН! Камера не будет работать!");
        }
    }

    // === ОБНОВЛЕНИЕ КАМЕРЫ ===

    // LateUpdate вызывается ПОСЛЕ всех Update()
    // Это важно! Сначала Player двигается в FixedUpdate/Update, потом камера следует за ним
    private void LateUpdate()
    {
        // Если нет цели - не двигаем камеру
        if (target == null)
        {
            Debug.LogError("CameraFollow: Target is NULL! Камера не может следовать!");
            return;
        }

        // Желаемая позиция = позиция игрока + смещение
        Vector3 desiredPosition = target.position + offset;

        // Плавное следование (lerp = linear interpolation)
        // Lerp(A, B, t) = постепенное движение от A к B
        // t = 0 -> остаёмся на месте A
        // t = 1 -> мгновенно перемещаемся в B
        // t = 0.125 -> каждый кадр сдвигаемся на 12.5% ближе к B (плавно!)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Если включены границы карты - ограничиваем позицию
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        }

        // Применяем новую позицию к камере
        transform.position = smoothedPosition;
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    /// <summary>
    /// Сменить цель слежения (если нужно следить за другим объектом)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Установить границы карты программно
    /// </summary>
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        useBounds = true;
    }

    /// <summary>
    /// Мгновенно переместить камеру (без сглаживания)
    /// Полезно при телепортации игрока
    /// </summary>
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    // === ОТЛАДКА (видно в редакторе) ===

    #if UNITY_EDITOR
    // Рисуем границы камеры в редакторе Unity (зелёный прямоугольник)
    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.green;

            // Рисуем рамку границ
            Vector3 topLeft = new Vector3(minX, maxY, 0);
            Vector3 topRight = new Vector3(maxX, maxY, 0);
            Vector3 bottomLeft = new Vector3(minX, minY, 0);
            Vector3 bottomRight = new Vector3(maxX, minY, 0);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
    #endif
}