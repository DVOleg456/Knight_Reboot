using UnityEngine;

// Базовый класс для всех видов оружия
public abstract class Weapon : MonoBehaviour
{
    // Настройки оружия
    [Header("Характеристики оружия")]
    [SerializeField] protected string weaponName = "Weapon"; // Название оружия
    [SerializeField] protected int damage = 10; // Урон
    [SerializeField] protected float attackCooldown = 1f; // Задержка между атаками (в секундах)
    [SerializeField] protected float attackRange = 1.5f; // Дальность атаки

    [Header("Визуальные эффекты")]
    [SerializeField] protected Sprite weaponSprite; // Спрайт оружия

    // Приватные переменные
    protected float lastAttackTime; // Время последней атаки
    protected bool isAttacking = false; // Атакует ли сейчас персонаж

    // Публичные методы

    // Попытка атаки (проверяет можно ли атаковать)
    public bool TryAttack()
    {
        // Проверяем кулдаун (прошло ли достаточно времени)
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
            return true;
        }
        return false;
    }

    // Выполнить атаку (переопределяется в наследниках)
    protected abstract void PerformAttack();

    // Может ли оружие атаковать сейчас 
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    // Получить урон оружия
    public int GetDamage()
    {
        return damage;
    }

    // Получить дальность атаки
    public float GetAttackRange()
    {
        return attackRange;
    }

    // Получить название оружия
    public string GetWeaponName()
    {
        return weaponName;
    }

    // Атакует ли сейчас
    public bool IsAttacking()
    {
        return isAttacking;
    }

    // Отладка

    #if UNITY_EDITOR
    // Рисуем дальность атаки в редакторе (зеленый круг)
    protected virtual void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endif
}