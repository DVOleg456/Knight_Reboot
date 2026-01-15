using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float movingSpeed = 5f;



    private Rigidbody2D rb;
    private HealthSystem healthSystem; // Система здоровья игрока
    private PlayerVisual playerVisual; // Визуал игрока для анимаций

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;
    private bool isDead = false; // Флаг смерти

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();

        // Получаем компонент HealthSystem (он должен быть на этом же GameObject)
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogWarning("Player: HealthSystem не найден! Добавь компонент HealthSystem на игрока");
        }
        else
        {
            // Подписываемся на событие смерти
            healthSystem.OnDead += HealthSystem_OnDead;
        }

        // Получаем PlayerVisual (на дочернем объекте PlayerVisual)
        playerVisual = GetComponentInChildren<PlayerVisual>();
        if (playerVisual == null)
        {
            Debug.LogWarning("Player: PlayerVisual не найден! Убедись что он есть в дочерних объектах");
        }
    }
    

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Если игрок мёртв - не двигаемся
        if (isDead)
        {
            isRunning = false;
            return;
        }

        Vector2 inputVector = GameInput.Instance.GetMovementVector();

        inputVector = inputVector.normalized;

        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime));

        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }

    public bool IsRunning()
    {
        return isRunning;    
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint (transform.position);
        return playerScreenPosition;
    }

    // Получить систему здоровья игрока
    public HealthSystem GetHealthSystem()
    {
        return healthSystem;
    }

    // Обработчик события смерти
    private void HealthSystem_OnDead(object sender, System.EventArgs e)
    {
        Debug.Log("Player: Игрок умер!");

        isDead = true;

        // Запускаем анимацию смерти
        if (playerVisual != null)
        {
            playerVisual.TriggerDeathAnimation();
        }

        // Отключаем Rigidbody2D чтобы игрок не падал
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Отключаем PlayerCombat чтобы нельзя было атаковать
        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }

        // Здесь позже можно показать Game Over UI
    }

    // Проверить мёртв ли игрок
    public bool IsDead()
    {
        return isDead;
    }
}