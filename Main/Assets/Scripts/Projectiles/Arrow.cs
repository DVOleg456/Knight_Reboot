using UnityEngine;

// Стрела - быстрый прямолинейный снаряд
// Используется гоблином-лучником
public class Arrow : Projectile
{
    [Header("Настройки стрелы")]
    [SerializeField] private float arrowSpeed = 15f;
    [SerializeField] private int arrowDamage = 15;

    [Header("След стрелы (опционально)")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private bool enableTrail = false;

    protected override void Awake()
    {
        base.Awake();

        // Применяем настройки стрелы
        speed = arrowSpeed;
        damage = arrowDamage;

        // Настраиваем след
        if (trailRenderer != null)
        {
            trailRenderer.enabled = enableTrail;
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    // Инициализация стрелы
    public override void Initialize(Vector2 direction, GameObject owner, int damage = -1)
    {
        base.Initialize(direction, owner, damage);

        // Поворачиваем стрелу в направлении полёта
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    protected override void OnHitTarget(Collider2D target)
    {
        // Стрела просто наносит урон и исчезает
        base.OnHitTarget(target);
    }

    protected override void OnHitObstacle(Collider2D obstacle)
    {
        // Стрела застревает в препятствии
        // Можно оставить визуал на некоторое время

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, 0.3f);
        }

        // Отключаем движение и коллайдер
        isActive = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Уничтожаем через небольшую задержку (стрела "застряла")
        Destroy(gameObject, 1f);
    }
}
