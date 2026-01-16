using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private const string IS_RUNNING = "IsRunning";
    private const string IS_DEAD = "IsDead";

    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
