using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private HealthSystem healthSystem;

    private const string IS_RUNNING = "IsRunning";
    private const string IS_DEAD = "IsDead";
    private const string TAKE_HIT_TRIGGER = "TakeHit";

    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Подписываемся на получение урона игроком
        if (Player.Instance != null)
        {
            healthSystem = Player.Instance.GetHealthSystem();
            if (healthSystem != null)
            {
                healthSystem.OnDamaged += HealthSystem_OnDamaged;
            }
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (healthSystem != null)
        {
            healthSystem.OnDamaged -= HealthSystem_OnDamaged;
        }
    }

    // Обработчик получения урона
    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger(TAKE_HIT_TRIGGER);
        }
    }

    private void Update()
    {
        // Если игрок мертв - не обновляем анимации движения
        if (isDead) return;

        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        AdjustPlayerFacingDirection();
    }

    private void AdjustPlayerFacingDirection()
    {
        // Получаем вектор движения от клавиш WASD
        Vector2 moveVector = GameInput.Instance.GetMovementVector();

        // Если есть горизонтальное движение - разворачиваем спрайт
        if (moveVector.x < -0.01f)
        {
            // Нажата клавиша A (движение влево) - отражаем спрайт
            spriteRenderer.flipX = true;
        }
        else if (moveVector.x > -0.01f)
        {
            // Нажата клавиша D (движение вправо) - нормальный спрайт
            spriteRenderer.flipX = false;
        }
        // Если moveVector.x == 0, не меняем направление (персонаж смотрит в ту сторону, куда смотрел)
    }

    // Запустить анимацию атаки (вызывается из PlayerCombat)
    public void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    // Запустить анимацию смерти (вызывается при смерти игрока)
    public void TriggerDeathAnimation()
    {
        if (animator != null)
        {
            isDead = true;
            animator.SetBool(IS_DEAD, true);
            Debug.Log($"PlayerVisual: Анимация смерти запущена!");
        }
    }

    // Проверить мёртв ли игрок 
    public bool IsDead()
    {
        return isDead;
    }
}