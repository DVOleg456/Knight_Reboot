using UnityEngine;
using System.Collections;
using System;

// Менеджер бонусов на игроке
public class BonusManager : MonoBehaviour
{
    // События для UI
    public event EventHandler<BonusEventArgs> OnBonusActivated;
    public event EventHandler<BonusEventArgs> OnBonusDeactivated;

    // Текущие множители
    private float currentSpeedMultiplier = 1f;
    private float currentDamageMultiplier = 1f;

    // Корутины для отслеживания
    private Coroutine speedBonusCoroutine;
    private Coroutine damageBonusCoroutine;

    // Ссылки на компоненты
    private Player player;
    private PlayerCombat playerCombat;

    // Оригинальные значения (для восстановления)
    private float originalMoveSpeed;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    // Применить бонус скорости
    public void ApplySpeedBonus(float multiplier, float duration)
    {
        // Если уже есть бонус скорости - отменяем его
        if (speedBonusCoroutine != null)
        {
            StopCoroutine(speedBonusCoroutine);
            ResetSpeedBonus();
        }

        speedBonusCoroutine = StartCoroutine(SpeedBonusRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBonusRoutine(float multiplier, float duration)
    {
        currentSpeedMultiplier = multiplier;

        // Уведомляем о активации бонуса
        OnBonusActivated?.Invoke(this, new BonusEventArgs("Ускорение", duration, multiplier));

        Debug.Log($"BonusManager: Бонус скорости активирован (x{multiplier}) на {duration} сек.");

        yield return new WaitForSeconds(duration);

        ResetSpeedBonus();
    }

    private void ResetSpeedBonus()
    {
        currentSpeedMultiplier = 1f;
        speedBonusCoroutine = null;

        OnBonusDeactivated?.Invoke(this, new BonusEventArgs("Ускорение", 0, 1f));

        Debug.Log("BonusManager: Бонус скорости закончился.");
    }

    // Применить бонус урона
    public void ApplyDamageBonus(float multiplier, float duration)
    {
        // Если уже есть бонус урона - отменяем его
        if (damageBonusCoroutine != null)
        {
            StopCoroutine(damageBonusCoroutine);
            ResetDamageBonus();
        }

        damageBonusCoroutine = StartCoroutine(DamageBonusRoutine(multiplier, duration));
    }

    private IEnumerator DamageBonusRoutine(float multiplier, float duration)
    {
        currentDamageMultiplier = multiplier;

        // Уведомляем о активации бонуса
        OnBonusActivated?.Invoke(this, new BonusEventArgs("Увеличенный урон", duration, multiplier));

        Debug.Log($"BonusManager: Бонус урона активирован (x{multiplier}) на {duration} сек.");

        yield return new WaitForSeconds(duration);

        ResetDamageBonus();
    }

    private void ResetDamageBonus()
    {
        currentDamageMultiplier = 1f;
        damageBonusCoroutine = null;

        OnBonusDeactivated?.Invoke(this, new BonusEventArgs("Увеличенный урон", 0, 1f));

        Debug.Log("BonusManager: Бонус урона закончился.");
    }

    // Получить текущий множитель скорости
    public float GetSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }

    // Получить текущий множитель урона
    public float GetDamageMultiplier()
    {
        return currentDamageMultiplier;
    }

    // Проверка активных бонусов
    public bool HasSpeedBonus() => speedBonusCoroutine != null;
    public bool HasDamageBonus() => damageBonusCoroutine != null;
}

// Класс для передачи данных о бонусе в событиях
public class BonusEventArgs : EventArgs
{
    public string BonusName { get; private set; }
    public float Duration { get; private set; }
    public float Multiplier { get; private set; }

    public BonusEventArgs(string name, float duration, float multiplier)
    {
        BonusName = name;
        Duration = duration;
        Multiplier = multiplier;
    }
}
