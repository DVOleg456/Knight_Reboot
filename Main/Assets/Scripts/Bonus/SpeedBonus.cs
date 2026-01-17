using UnityEngine;
using System.Collections;

// Бонус ускорения (Accelerator)
public class SpeedBonus : Bonus
{
    [Header("Настройки ускорения")]
    [SerializeField] private float speedMultiplier = 1.5f; // Множитель скорости

    protected override void Start()
    {
        base.Start();
        bonusName = "Ускорение";
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

        bonusManager.ApplySpeedBonus(speedMultiplier, duration);
        Debug.Log($"SpeedBonus: Скорость увеличена x{speedMultiplier} на {duration} секунд!");
    }

    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan; // Голубой для скорости
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    #endif
}
