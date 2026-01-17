using UnityEngine;

// Бонус увеличенного урона (Increased_damage)
public class DamageBonus : Bonus
{
    [Header("Настройки урона")]
    [SerializeField] private float damageMultiplier = 2f; // Множитель урона

    protected override void Start()
    {
        base.Start();
        bonusName = "Увеличенный урон";
        isInstant = false; // Временный эффект
    }

    protected override void ApplyEffect(Player player)
    {
        // Применяем эффект через BonusManager
        BonusManager bonusManager = player.GetComponent<BonusManager>();
        if (bonusManager == null)
        {
            bonusManager = player.gameObject.AddComponent<BonusManager>();
        }

        bonusManager.ApplyDamageBonus(damageMultiplier, duration);
        Debug.Log($"DamageBonus: Урон увеличен x{damageMultiplier} на {duration} секунд!");
    }

    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta; // Пурпурный для урона
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    #endif
}
