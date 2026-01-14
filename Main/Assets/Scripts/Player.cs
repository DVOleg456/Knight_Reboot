using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float movingSpeed = 5f;

    

    private Rigidbody2D rb;
    private HealthSystem healthSystem; // Система здоровья игрока

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();


        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogWarning("Player: HealthSystem не найден! Добавь компонент HealthSystem на игрока");
        }
    }
    

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
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
}
