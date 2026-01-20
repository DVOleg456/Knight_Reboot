using UnityEngine;
using System;
 
// Базовый класс для всех снарядов (стрелы, бомбы и т.д.)
public abstract class Projectile : MonoBehaviour
{
    [Header("Настройки снаряда")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float lifetime = 5f;
 
    [Header("Слои")]
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected LayerMask obstacleLayer;
 
    [Header("Визуал")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected bool rotateToDirection = true;
 
    [Header("Звуки")]
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip flySound;
 
    // Приватные переменные
    protected Vector2 direction;
    protected bool isActive = true;
    protected float spawnTime;
    protected GameObject owner; // Кто выпустил снаряд
 
    // События
    public event EventHandler OnProjectileHit;
    public event EventHandler OnProjectileDestroyed;
 
    protected virtual void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
 
    protected virtual void Start()
    {
        spawnTime = Time.time;
 
        // Воспроизводим звук полёта
        if (flySound != null)
        {
            AudioSource.PlayClipAtPoint(flySound, transform.position, 0.5f);
        }
    }
 
    protected virtual void Update()
    {
        if (!isActive) return;
 
        // Движение
        MoveProjectile();
 
        // Проверка времени жизни
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyProjectile();
        }
 
        // Поворот в направлении движения
        if (rotateToDirection && direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
 
    // Движение снаряда
    protected virtual void MoveProjectile()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
 
    // Инициализация снаряда
    public virtual void Initialize(Vector2 direction, GameObject owner, int damage = -1)
    {
        this.direction = direction.normalized;
        this.owner = owner;
 
        if (damage >= 0)
        {
            this.damage = damage;
        }
 
        isActive = true;
    }
 
    // Установить цель (снаряд летит к цели)
    public virtual void SetTarget(Transform target)
    {
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
    }
 
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
 
        // Игнорируем владельца снаряда
        if (owner != null && other.gameObject == owner) return;
 
        // Проверяем попадание в цель
        if (IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            OnHitTarget(other);
            return;
        }
 
        // Проверяем попадание в препятствие
        if (IsInLayerMask(other.gameObject.layer, obstacleLayer))
        {
            OnHitObstacle(other);
        }
    }
 
    // Обработка попадания в цель
    protected virtual void OnHitTarget(Collider2D target)
    {
        // Наносим урон
        HealthSystem healthSystem = target.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
            Debug.Log($"Projectile: Нанесён урон {damage} объекту {target.name}");
        }
 
        // Воспроизводим звук попадания
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
 
        // Вызываем событие
        OnProjectileHit?.Invoke(this, EventArgs.Empty);
 
        // Уничтожаем снаряд
        DestroyProjectile();
    }
 
    // Обработка попадания в препятствие
    protected virtual void OnHitObstacle(Collider2D obstacle)
    {
        // Воспроизводим звук попадания
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, 0.5f);
        }
 
        DestroyProjectile();
    }
 
    // Уничтожение снаряда
    protected virtual void DestroyProjectile()
    {
        if (!isActive) return;
 
        isActive = false;
        OnProjectileDestroyed?.Invoke(this, EventArgs.Empty);
 
        // Можно добавить эффект уничтожения
        Destroy(gameObject);
    }
 
    // Проверка принадлежности слою
    protected bool IsInLayerMask(int layer, LayerMask mask)
    {
        return ((1 << layer) & mask) != 0;
    }
 
    // Получить урон снаряда
    public int GetDamage() => damage;
 
    // Получить скорость снаряда
    public float GetSpeed() => speed;
 
#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (direction != Vector2.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
#endif
}