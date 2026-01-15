using UnityEngine;

// Управление визуалом и анимациями врага
// Аналог PlayerVisual, но для врагов
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisual : MonoBehaviour
{
    // Компоненты
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Enemy enemy;
    private HealthSystem healthSystem;

    // Параметры аниматора (должны совпадать с названиями в Animator Controller)
    private const string IS_MOVING = "IsMoving";
    private const string IS_ATTACKING = "IsAttacking";
    private const string IS_DEAD = "IsDead";
    private const string ATTACK_TRIGGER = "Attack";

    // Состояние
    private bool isDead = false;
    private Vector3 lastPosition;

    // Инициализация
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Получаем Enemy компонент (может быть на родительском объекте)
        enemy = GetComponentInParent<Enemy>();
        if (enemy == null)
        {
            Debug.LogWarning($"EnemyVisual {name}: Enemy компонент не найден!");
        }

        // Получаем HealthSystem
        healthSystem = GetComponentInParent<HealthSystem>();
        if (healthSystem != null)
        {
            // Подписываемся на событие смерти
            healthSystem.OnDead += HealthSystem_OnDead;
        }

        lastPosition = transform.position;
    }

    // Обновление 
    private void Update()
    {
        // Если враг мёртв - не обновляем анимации
        if (isDead) return;

        UpdateMovementAnimation();
        UpdateFacingDirection();
        UpdateAttackAnimation();
    }

    // Обновление анимации движения
    private void UpdateMovementAnimation()
    {
        if (enemy == null) return;

        // Проверяем двигается ли враг (сравниваем текущую позицию с предыдущей)
        bool isMoving = Vector3.Distance(transform.position, lastPosition) > 0.01f;

        animator.SetBool(IS_MOVING, isMoving);
        lastPosition = transform.position;
    }

    // Обновление направления взгляда 
    private void UpdateFacingDirection()
    {
        if (enemy == null) return;

        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Направление к игроку
            Vector3 directionToPlayer = player.transform.position - transform.position;

            // Поворачиваем спрайт в сторону игрока
            if (directionToPlayer.x < -0.1f)
            {
                // Игрок слева - отражаем спрайт 
                spriteRenderer.flipX = true;
            }
            else if (directionToPlayer.x > 0.1f)
            {
                // Игрок справа - нормальный спрайт
                spriteRenderer.flipX = false;
            }
        }
    }

    // Обновление анимации атаки
    private void UpdateAttackAnimation()
    {
        if (enemy == null) return;

        // Устанавливаем параметр атаки в аниматоре
        bool isAttacking = enemy.IsAttacking();
        animator.SetBool(IS_ATTACKING, isAttacking);
    }

    // Запустить триггер атаки (вызывается из Enemy при атаке)
    public void TriggerAttackAnimation()
    {
        if (!isDead && animator != null)
        {
            animator.SetTrigger(ATTACK_TRIGGER);
        }
    }

    // Обработчик смерти врага
    private void  HealthSystem_OnDead(object sender, System.EventArgs e)
    {
        if (isDead) return;

        isDead = true;

        if (animator != null)
        {
            animator.SetBool(IS_DEAD, true);
            Debug.Log($"EnemyVisual {name}: Анимация смерти запущена!");
        }

        // Отключаем коллайдеры чтобы игрок мог пройти через труп
        Collider2D[] colliders = GetComponentsInParent<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }

    // Очистка 
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDead -= HealthSystem_OnDead;
        }
    }

    // Проверка мёртв ли враг
    public bool IsDead()
    {
        return isDead;
    }
}