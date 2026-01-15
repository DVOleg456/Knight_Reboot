using UnityEngine;

// Система боя для игрока
public class PlayerCombat : MonoBehaviour
{
    // Настройки
    [Header("Оружие")]
    [SerializeField] private Weapon currentWeapon; // Текущее оружие

    [Header("Точка атаки")]
    [Tooltip("Точка откуда идёт атака (перед игроком)")]
    [SerializeField] private Transform attackPoint;

    // Приватные переменные
    private bool isAttacking = false;

    // Инициализация
    private void Start()
    {
        // Проверяем наличие оружия 
        if (currentWeapon == null)
        {
            // Пытаемся найти оружие на дочерних объектах
            currentWeapon = GetComponentInChildren<Weapon>();
            if (currentWeapon == null)
            {
                Debug.LogWarning("PlayerCombat: Оружие не назначено! Добавь компонент Weapon");
            }
            else
            {
                Debug.Log($"PlayerCombat: Автоматически найдено оружие {currentWeapon.GetWeaponName()}");
            }
        }

        // Проверяем точку атаки
        if (attackPoint == null)
        {
            Debug.LogWarning("PlayerCombat: Attack Point не назначен! Атака будет от центра игрока");
        }

    }

    // Обновление 
    private void Update()
    {
        HandleAttackInput();
    }

    // Обработка ввода атаки
    private void HandleAttackInput()
    {
        // Проверяем нажатие кнопки атаки (Space или ЛКМ)
        if (GameInput.Instance.GetAttackButtonDown())
        {
            TryAttack();
        }
    }

    // Попытка атаки 
    public void TryAttack()
    {
        // Проверяем есть ли оружие
        if (currentWeapon == null)
        {
            Debug.LogWarning("PlayerCombat: Нет оружия для атаки!");
            return;
        }

        // Проверяем можно ли атаковать (кулдаун)
        if (!currentWeapon.CanAttack())
        {
            Debug.LogWarning("PlayerCombat: Оружие на кулдауне, подожди!");
            return;
        }

        // Выполняем атаку
        bool attackSuccessful = currentWeapon.TryAttack();

        if (attackSuccessful)
        {
            isAttacking = true;
            Debug.Log($"PlayerCombat: Атака с {currentWeapon.GetWeaponName()}!");

            // Сбрасываем флаг атаки через короткое время
            Invoke(nameof(ResetAttacking), 0.5f);
        }
    }

    // Сбросить флаг атаки
    private void ResetAttacking()
    {
        isAttacking = false;
    }

    // Сменить оружие 
    public void EquipWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null)
        {
            // Убираем старое оружие
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeapon = newWeapon;
        
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
            Debug.Log($"PlayerCombat: Экипировано оружие {currentWeapon.GetWeaponName()}");
        }
    }

    // Получить текущее оружие 
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    // Атакует ли игрок сейчас
    public bool IsAttacking()
    {
        return isAttacking || (currentWeapon != null && currentWeapon.IsAttacking());
    }

    // Отладка
    #if UNITY_EDITOR
    // Тестовая атака (вызывать из Inspector)
    [ContextMenu("Test: Attack")]
    public void TestAttack(){
        TryAttack();
    }
    #endif
}