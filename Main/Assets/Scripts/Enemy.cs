using UnityEngine;

// Базовый класс врага с простым AI
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class Enemy : MonoBehaviour
{
    // Перечисления
    public enum EnemyState
    {
        Idle, // Стоит на месте
        Chase, // Преследует игрока
        Attack // Атакует игрока
    }

    // Настройки
    [Header("Характеристики врага")]
    [SerializeField] private float moveSpeed = 2f; // Скорость передвижения
    [SerializeField] private int attackDamage = 10; // Урон атаки
    [SerializeField] private float attackCooldown = 1.5f; // Задержка между атаками

    [Header("Дистанции")]
    [SerializeField] private float detectionRange = 5f; // Дальность обнаружения игрока
    [SerializeField] private float attackRange = 1.2f; // Дальность атаки
    [SerializeField] private float stopChaseDistance = 7f; // На какой дистанции прекратить погоню

    [Header("Слои")]
    [SerializeField] private LayerMask playerLayer; // Слой игрока

    // Компоненты
    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private Transform playerTransform;

    // Состояние 
    private EnemyState currentState = EnemyState.Idle;
    private float lastAttackTime;
    private bool isAttacking = false;

    // Инициализация
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();

        // Настраиваем Rigidbody2D
        rb.gravityScale = 0f; // Отключаем гравитацию для 2D сверху 
        rb.freezeRotation = true; // Не даём вращаться
    }

    private void Start()
    {
        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError($"Enemy {name}: Игрок с тегом 'Player' не найден!");
        }

        // Подписываемся на смерть
        if (healthSystem != null)
        {
            healthSystem.OnDead += HealthSystem_OnDead;
        }
    }

    // Обновление
    private void Update()
    {
        // Если нет игрока или враг мёртв - ничего не делаем
        if (playerTransform == null || healthSystem.IsDead())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Обновляем AI в зависимости от состояния
        UpdateAI();
    }

    // Обновление искуственного интеллекта 
    private void UpdateAI()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Переключение состояний
        switch (currentState)
        {
            case EnemyState.Idle:
                // Если игрок в зоне обнаружения - начинаем предследование
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                // Если игрок в зоне атаки - атакуем
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                // Если игрок убежал - возвращаемся в Idle
                else if (distanceToPlayer > stopChaseDistance)
                {
                    ChangeState(EnemyState.Idle);
                }
                else
                {
                    // Двигаемся к игроку
                    MoveTowardsPlayer();
                }
                break;

            case EnemyState.Attack:
                // Если игрок вышел из зоны атаки - преследуем
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                else
                {
                    // Останавливаемся и атакуем
                    rb.linearVelocity = Vector2.zero;
                    TryAttack();
                }
                break;
        }
    }

    // Сменить состояние врага
    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"Enemy {name}: {currentState} -> {newState}");
        currentState = newState;

        // При смене состояния сбрасываем скорость
        if (newState != EnemyState.Chase)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Двигаться к игроку
    private void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    // Попытка атаки
    private void TryAttack()
    {
        // Проверяем кулдаун
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        // Выполняем атаку
        PerformAttack();
        lastAttackTime = Time.time;
    }

    // Выполнить атаку
    private void PerformAttack()
    {
        isAttacking = true;

        Debug.Log($"Enemy {name}: Атака игрока! Урон: {attackDamage}");

        // Проверяем попадание по игроку 
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);

        foreach (Collider2D hit in hits)
        {
            HealthSystem playerHealth = hit.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Enemy {name}: Нанёс {attackDamage} урона игроку!");
            }
        }
        // Сбрасываем флаг атаки
        Invoke(nameof(ResetAttack), 0.3f);
    }

    // Сбросить флаг атаки
    private void ResetAttack()
    {
        isAttacking = false;
    }

    // Обработчик смерти врага
    private void HealthSystem_OnDead(object sender, System.EventArgs e)
    {
        Debug.Log($"Enemy {name}: Умер!");

        // Останавливаем врага
        rb.linearVelocity = Vector2.zero;
        currentState = EnemyState.Idle;

        // Уничтожаем врага через 2 секунды (время для анимации смерти)
        Destroy(gameObject, 2f); 
    }

    // Получить текущее состояние
    public EnemyState GetCurrentState()
    {
        return currentState;
    }

    // Атакует ли враг сейчас
    public bool IsAttacking()
    {
        return isAttacking;
    }

    // Отладка
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected(){
        // Зона обнаружения (желтый круг)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Зона атаки (красный круг)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Дистанция прекращения погони (зелёный круг)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }
    #endif

    private void OnDestroy()
    {
        // Отписываемся от событий 
        if (healthSystem != null)
        {
            healthSystem.OnDead -= HealthSystem_OnDead;
        }
    }
}