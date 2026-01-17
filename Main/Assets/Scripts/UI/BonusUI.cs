using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI для отображения активных бонусов
public class BonusUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private GameObject speedBonusIndicator;
    [SerializeField] private GameObject damageBonusIndicator;
    [SerializeField] private TextMeshProUGUI speedBonusText;
    [SerializeField] private TextMeshProUGUI damageBonusText;

    private BonusManager bonusManager;

    private void Start()
    {
        // Скрываем индикаторы
        if (speedBonusIndicator != null)
            speedBonusIndicator.SetActive(false);
        if (damageBonusIndicator != null)
            damageBonusIndicator.SetActive(false);

        // Получаем BonusManager игрока
        if (Player.Instance != null)
        {
            bonusManager = Player.Instance.GetComponent<BonusManager>();
            if (bonusManager != null)
            {
                bonusManager.OnBonusActivated += BonusManager_OnBonusActivated;
                bonusManager.OnBonusDeactivated += BonusManager_OnBonusDeactivated;
            }
        }
    }

    private void OnDestroy()
    {
        if (bonusManager != null)
        {
            bonusManager.OnBonusActivated -= BonusManager_OnBonusActivated;
            bonusManager.OnBonusDeactivated -= BonusManager_OnBonusDeactivated;
        }
    }

    private void BonusManager_OnBonusActivated(object sender, BonusEventArgs e)
    {
        if (e.BonusName == "Ускорение")
        {
            if (speedBonusIndicator != null)
            {
                speedBonusIndicator.SetActive(true);
            }
            if (speedBonusText != null)
            {
                speedBonusText.text = $"x{e.Multiplier:F1}";
            }
        }
        else if (e.BonusName == "Увеличенный урон")
        {
            if (damageBonusIndicator != null)
            {
                damageBonusIndicator.SetActive(true);
            }
            if (damageBonusText != null)
            {
                damageBonusText.text = $"x{e.Multiplier:F1}";
            }
        }
    }

    private void BonusManager_OnBonusDeactivated(object sender, BonusEventArgs e)
    {
        if (e.BonusName == "Ускорение")
        {
            if (speedBonusIndicator != null)
            {
                speedBonusIndicator.SetActive(false);
            }
        }
        else if (e.BonusName == "Увеличенный урон")
        {
            if (damageBonusIndicator != null)
            {
                damageBonusIndicator.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Обновляем состояние индикаторов
        if (bonusManager != null)
        {
            // Если по какой-то причине состояние не синхронизировано
            if (speedBonusIndicator != null && speedBonusIndicator.activeSelf != bonusManager.HasSpeedBonus())
            {
                speedBonusIndicator.SetActive(bonusManager.HasSpeedBonus());
            }
            if (damageBonusIndicator != null && damageBonusIndicator.activeSelf != bonusManager.HasDamageBonus())
            {
                damageBonusIndicator.SetActive(bonusManager.HasDamageBonus());
            }
        }
    }
}
