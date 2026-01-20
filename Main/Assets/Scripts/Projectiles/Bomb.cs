using UnityEngine;
using System.Collections;
 
// Бомба - снаряд с взрывом по области
// Используется скелетом-бомбардиром
public class Bomb : Projectile
{
    [Header("Настройки бомбы")]
    [SerializeField] private float bombSpeed = 8f;
    [SerializeField] private int bombDamage = 20;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float fuseTime = 3f; // Время до взрыва (если не попала в цель)
 
    [Header("Полёт по дуге")]
    [SerializeField] private bool arcTrajectory = true;
    [SerializeField] private float arcHeight = 2f;
 
    [Header("Визуальные эффекты")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private SpriteRenderer bombSprite;
    [SerializeField] private bool flashBeforeExplosion = true;
    [SerializeField] private float flashInterval = 0.2f;
 
    [Header("Звуки")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip fuseSound;
 
    // Переменные для дуги
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightProgress = 0f;
    private float flightDuration;
    private bool isFlying = false;
 
    protected override void Awake()
    {
        base.Awake();
 
        speed = bombSpeed;
        damage = bombDamage;
 
        if (bombSprite == null)
        {
            bombSprite = GetComponent<SpriteRenderer>();
        }
    }
 
    protected override void Start()
    {
        base.Start();
 
        // Запускаем таймер взрыва
        StartCoroutine(FuseTimer());
 
        // Воспроизводим звук фитиля
        if (fuseSound != null)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = fuseSound;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }
    }
 
    protected override void Update()
    {
        if (!isActive) return;
 
        if (arcTrajectory && isFlying)
        {
            // Движение по дуге
            MoveInArc();
        }
        else
        {
            // Прямолинейное движение
            base.Update();
        }
    }
 
    // Инициализация бомбы с целевой позицией (для броска по дуге)
    public void InitializeWithTarget(Vector3 targetPos, GameObject owner, int damage = -1)
    {
        this.owner = owner;
 
        if (damage >= 0)
        {
            this.damage = damage;
        }
 
        startPosition = transform.position;
        targetPosition = targetPos;
 
        // Вычисляем время полёта
        float distance = Vector3.Distance(startPosition, targetPosition);
        flightDuration = distance / speed;
 
        isFlying = true;
        isActive = true;
    }
 
    // Движение по параболической траектории
    private void MoveInArc()
    {
        flightProgress += Time.deltaTime / flightDuration;
 
        if (flightProgress >= 1f)
        {
            // Достигли цели - взрываемся
            transform.position = targetPosition;
            Explode();
            return;
        }
 
        // Линейная интерполяция позиции
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, flightProgress);
 
        // Добавляем высоту по параболе
        float height = arcHeight * Mathf.Sin(flightProgress * Mathf.PI);
        currentPos.y += height;
 
        transform.position = currentPos;
 
        // Вращаем бомбу в полёте
        transform.Rotate(Vector3.forward, 360f * Time.deltaTime);
    }
 
    // Таймер фитиля
    private IEnumerator FuseTimer()
    {
        float timeRemaining = fuseTime;
 
        // Мигание перед взрывом
        if (flashBeforeExplosion && bombSprite != null)
        {
            while (timeRemaining > 1f)
            {
                yield return new WaitForSeconds(flashInterval * 2);
                timeRemaining -= flashInterval * 2;
            }
 
            // Быстрое мигание в последнюю секунду
            while (timeRemaining > 0 && isActive)
            {
                bombSprite.color = Color.red;
                yield return new WaitForSeconds(flashInterval / 2);
                bombSprite.color = Color.white;
                yield return new WaitForSeconds(flashInterval / 2);
                timeRemaining -= flashInterval;
            }
        }
        else
        {
            yield return new WaitForSeconds(fuseTime);
        }
 
        if (isActive)
        {
            Explode();
        }
    }
 
    protected override void OnHitTarget(Collider2D target)
    {
        // Игнорируем коллизии во время полёта по дуге - взрываемся только при приземлении
        if (arcTrajectory && isFlying && flightProgress < 0.95f)
        {
            return;
        }
        // При попадании в цель - взрываемся
        Explode();
    }

    protected override void OnHitObstacle(Collider2D obstacle)
    {
        // Игнорируем коллизии во время полёта по дуге - взрываемся только при приземлении
        if (arcTrajectory && isFlying && flightProgress < 0.95f)
        {
            return;
        }
        // При попадании в препятствие - взрываемся
        Explode();
    }
 
    // Взрыв бомбы
    private void Explode()
    {
        if (!isActive) return;
        isActive = false;

        // Сохраняем позицию взрыва до уничтожения объекта
        Vector3 explosionPosition = transform.position;

        Debug.Log($"Bomb: Взрыв на позиции {explosionPosition}! Радиус: {explosionRadius}, Урон: {damage}");

        // Воспроизводим звук взрыва
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, explosionPosition, 1f);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBombExplosion();
        }

        // Создаём эффект взрыва
        CreateExplosionEffect(explosionPosition);

        // Находим всех в радиусе взрыва
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius, targetLayer);

        foreach (Collider2D hit in hits)
        {
            // Не наносим урон владельцу
            if (owner != null && hit.gameObject == owner) continue;

            // Наносим урон
            HealthSystem healthSystem = hit.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                // Урон уменьшается с расстоянием
                float distance = Vector2.Distance(explosionPosition, hit.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius) * 0.5f; // 50% урона на краю
                int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

                healthSystem.TakeDamage(finalDamage);
                Debug.Log($"Bomb: Нанесён урон {finalDamage} объекту {hit.name}");
            }
        }

        // Уничтожаем бомбу
        Destroy(gameObject);
    }

    // Создание эффекта взрыва (вынесено в отдельный метод для надёжности)
    private void CreateExplosionEffect(Vector3 position)
    {
        if (explosionEffectPrefab == null) return;

        try
        {
            // Создаём копию эффекта в мировом пространстве
            GameObject effect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);

            // Гарантируем что эффект отвязан от родителя и активен
            effect.transform.SetParent(null);
            effect.SetActive(true);

            // Устанавливаем масштаб эффекта
            effect.transform.localScale = Vector3.one * 2f;

            Debug.Log($"Bomb: Создан эффект взрыва на позиции {position}");

            // Уничтожаем эффект через 2 секунды
            Destroy(effect, 2f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Bomb: Ошибка создания эффекта взрыва: {e.Message}");
        }
    }
 
    // Получить радиус взрыва
    public float GetExplosionRadius() => explosionRadius;
 
#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
 
        // Рисуем радиус взрыва
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
 
        // Рисуем траекторию полёта (если в редакторе)
        if (arcTrajectory && isFlying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPosition, targetPosition);
        }
    }
#endif
}