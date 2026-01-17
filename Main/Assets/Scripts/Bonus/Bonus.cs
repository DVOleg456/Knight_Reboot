using UnityEngine;
using System;

// Базовый класс для всех бонусов
public abstract class Bonus : MonoBehaviour
{
    [Header("Настройки бонуса")]
    [SerializeField] protected string bonusName = "Bonus";
    [SerializeField] protected float duration = 5f; // Длительность эффекта в секундах
    [SerializeField] protected Sprite bonusIcon; // Иконка бонуса для UI
    [SerializeField] protected bool isInstant = false; // Мгновенный эффект (без длительности)

    [Header("Визуальные эффекты")]
    [SerializeField] protected float rotationSpeed = 50f; // Скорость вращения
    [SerializeField] protected float floatAmplitude = 0.2f; // Амплитуда покачивания
    [SerializeField] protected float floatFrequency = 2f; // Частота покачивания

    [Header("Звук")]
    [SerializeField] protected AudioClip pickupSound; // Звук подбора

    protected Vector3 startPosition;
    protected bool isCollected = false;

    protected virtual void Start()
    {
        startPosition = transform.position;
    }

    protected virtual void Update()
    {
        if (isCollected) return;

        // Анимация покачивания и вращения
        AnimateBonus();
    }

    protected void AnimateBonus()
    {
        // Покачивание вверх-вниз
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Вращение
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // Проверяем что это игрок
        Player player = other.GetComponent<Player>();
        if (player == null)
        {
            player = other.GetComponentInParent<Player>();
        }

        if (player != null)
        {
            Collect(player);
        }
    }

    protected virtual void Collect(Player player)
    {
        isCollected = true;

        Debug.Log($"Bonus: Игрок подобрал бонус {bonusName}!");

        // Воспроизводим звук
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Применяем эффект
        ApplyEffect(player);

        // Уничтожаем объект бонуса
        Destroy(gameObject);
    }

    // Применить эффект бонуса (переопределяется в наследниках)
    protected abstract void ApplyEffect(Player player);

    // Получить название бонуса
    public string GetBonusName() => bonusName;

    // Получить длительность
    public float GetDuration() => duration;

    // Получить иконку
    public Sprite GetIcon() => bonusIcon;

    // Мгновенный ли бонус
    public bool IsInstant() => isInstant;

    // Отладка в редакторе
    #if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    #endif
}
