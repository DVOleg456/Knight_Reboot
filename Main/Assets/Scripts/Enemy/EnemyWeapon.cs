using UnityEngine;

// Оружие врага
public class EnemyWeapon : MonoBehaviour
{
    [Header("Настройки оружия")]
    [SerializeField] private string weaponName = "Dagger";
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Transform attackPoint; // Точка атаки

    [Header("Визуальные настройки")]
    [SerializeField] private SpriteRenderer weaponSprite;
    [SerializeField] private Sprite idleSprite; // Спрайт в покое
    [SerializeField] private Sprite attackSprite; // Спрайт при атаке

    [Header("Владелец")]
    [SerializeField] private EnemyAI owner;

    private bool isAttacking = false;

    private void Awake()
    {
        if (weaponSprite == null)
        {
            weaponSprite = GetComponent<SpriteRenderer>();
        }

        if (owner == null)
        {
            owner = GetComponentInParent<EnemyAI>();
        }
    }

    private void Start()
    {
        // Подписываемся на атаку врага
        if (owner != null)
        {
            owner.OnEnemyAttack += Owner_OnEnemyAttack;
        }

        // Устанавливаем начальный спрайт
        if (weaponSprite != null && idleSprite != null)
        {
            weaponSprite.sprite = idleSprite;
        }
    }

    private void OnDestroy()
    {
        if (owner != null)
        {
            owner.OnEnemyAttack -= Owner_OnEnemyAttack;
        }
    }

    // Обработчик атаки
    private void Owner_OnEnemyAttack(object sender, System.EventArgs e)
    {
        PerformAttack();
    }

    // Выполнить атаку
    private void PerformAttack()
    {
        if (isAttacking) return;

        isAttacking = true;

        // Меняем спрайт на атакующий
        if (weaponSprite != null && attackSprite != null)
        {
            weaponSprite.sprite = attackSprite;
        }

        Debug.Log($"EnemyWeapon: {weaponName} атакует!");

        // Возвращаем обычный спрайт через небольшую задержку
        Invoke(nameof(ResetAttack), 0.3f);
    }

    private void ResetAttack()
    {
        isAttacking = false;

        if (weaponSprite != null && idleSprite != null)
        {
            weaponSprite.sprite = idleSprite;
        }
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

    // Отладка в редакторе
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 pos = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(pos, attackRange);
    }
    #endif
}
