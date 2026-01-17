using UnityEngine;

// Бонус восстановления здоровья (Restoring_health)
public class HealthBonus : Bonus
{
    [Header("Настройки лечения")]
    [SerializeField] private int healAmount = 25; // Количество восстанавливаемого здоровья

    protected override void Start()
    {
        base.Start();
        bonusName = "Восстановление здоровья";
        isInstant = true; // Мгновенный эффект
    }

    protected override void ApplyEffect(Player player)
    {
        HealthSystem healthSystem = player.GetHealthSystem();
        if (healthSystem != null)
        {
            healthSystem.Heal(healAmount);
            Debug.Log($"HealthBonus: Игрок восстановил {healAmount} HP!");
        }
    }

    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Красный для здоровья
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    #endif
}
