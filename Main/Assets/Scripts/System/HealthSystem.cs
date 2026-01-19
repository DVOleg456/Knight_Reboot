using System;
using UnityEngine;

// Система здоровья для персонажей (игрок и враги)
public class HealthSystem : MonoBehaviour
{
    // События (Events)
    // Другие скрипты могут подписаться на эти события и реагировать на изменения здоровья
    public event EventHandler OnHealthChanged; // Событие, когда здоровье изменилось
    public event EventHandler OnDead; // Событие, когда персонаж умер
    public event EventHandler OnDamaged; // Событие, когда персонаж получил урон
    public event EventHandler OnHealed; // Событие, когда персонаж восстановил здоровье

    // Настройки
    [Header("Настройки здоровья")]
    [SerializeField] private int maxHealth = 100; // Максимальное здоровье

    [Header("Звуки")]
    [SerializeField] private AudioClip hurtSound; // Звук получения урона
    [SerializeField] private AudioClip deathSound; // Звук смерти
    [SerializeField] private AudioClip healSound; // Звук исцеления

    // Приватные переменные
    private int currentHealth; // Текущее здоровье
    private bool isDead = false; // Мёртв ли персонаж

    // Инициализация
    private void Awake()
    {
        // При создании объекта здоровье максимальное
        currentHealth = maxHealth;
    }

    // Публичные методы (можно вызывать из других скриптов)

    // Нанести урон персонажу
    public void TakeDamage(int damageAmount)
    {
        // Если уже мёртв персонаж, он не получает урона
        if (isDead) return;

        // Уменьшаем здоровье(не может быть меньше 0)
        currentHealth = currentHealth - damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{gameObject.name} получил {damageAmount} урона! Осталось здоровья: {currentHealth}/{maxHealth}");

        // Воспроизводим звук получения урона
        if (hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(hurtSound, transform.position, 0.8f);
        }

        // Вызываем событие "получен урон"
        OnDamaged?.Invoke(this, EventArgs.Empty);

        // Вызываем событие "здоровье изменилось"
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        // Проверяем, умер ли персонаж
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Восстановить здоровье
    public void Heal(int healAmount)
    {
        // Если персонаж умер, не лечимся
        if (isDead) return;

        // Увеличиваем здоровье (не может быть больше max)
        currentHealth = currentHealth + healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} восстановил {healAmount} здоровья! Текущее здоровье: {currentHealth}/{maxHealth}");

        // Воспроизводим звук исцеления
        if (healSound != null)
        {
            AudioSource.PlayClipAtPoint(healSound, transform.position, 0.7f);
        }

        // Вызываем событие "вылечен"
        OnHealed?.Invoke(this, EventArgs.Empty);

        // Вызываем событие "здоровье изменилось"
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    // Смерть персонажа
    private void Die()
    {
        if (isDead) return; // Нельзя умереть дважды

        isDead = true;
        Debug.Log($"{gameObject.name} умер");

        // Воспроизводим звук смерти
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        }

        // Вызываем событие "смерть"
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    // Получить текущее здоровье
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Получить максимальное здоровье
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // Получить процент здоровья (от 0 до 1)
    // Например: 75/100 = 0.75
    public float GetHealthNormalized()
    {
        return (float)currentHealth / maxHealth;
    }

    // Мёртв ли персонаж
    public bool IsDead()
    {
        return isDead;
    }

    // Установить максимальное здоровье (для бонусов)
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }
}