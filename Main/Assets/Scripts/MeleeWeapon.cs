using System.Collections.Generic;
using UnityEngine;

// Оружие ближнего боя (меч, кинжал)
public class MeleeWeapon : Weapon
{
    [Header("Настройки ближнего боя")]
    [SerializeField] private LayerMask enemyLayer; // Слой врагов
    [SerializeField] private Transform attackPoint; // Точка атаки (перед игроком)

    // Выполнение атаки
    protected override void PerformAttack()
    {
        isAttacking = true;

        // Получаем множитель урона от бонуса
        int finalDamage = damage;
        if (Player.Instance != null)
        {
            BonusManager bonusManager = Player.Instance.GetComponent<BonusManager>();
            if (bonusManager != null)
            {
                finalDamage = Mathf.RoundToInt(damage * bonusManager.GetDamageMultiplier());
            }
        }

        Debug.Log($"{weaponName}: Атака! Урон: {finalDamage}");

        // Находим всех врагов в радиусе атаки
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            GetAttackPosition(),
            attackRange,
            enemyLayer
        );

        // Наносим урон каждому найденному врагу
        foreach (Collider2D enemy in hitEnemies)
        {
            // Проверяем есть ли у врага HealthSystem
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(finalDamage);
                Debug.Log($"{weaponName}: Нанесён урон {enemy.name} ({finalDamage} HP)");
            }
        }

        // Через небольшую задержку сбрасываем флаг атаки
        Invoke(nameof(ResetAttack), 0.3f);
    }

    // Сбросить состояние атаки
    private void ResetAttack()
    {
        isAttacking = false;
    }

    // Получить позицию атаки (перед игроком)
    private Vector3 GetAttackPosition()
    {
        // Если задана точка атаки - используем её
        if (attackPoint != null)
        {
            return attackPoint.position;
        }

        // Иначе атакуем от центра оружия
        return transform.position;
    }

    // Отладка в редакторе
#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected(){
        // Рисуем зону атаки красным кругом
        Gizmos.color = Color.red;
        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
#endif
}