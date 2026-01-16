using UnityEngine;
using UnityEngine.UI;

// UI отображение здоровья игрока
public class HealthUI : MonoBehaviour
{
    // Настройки
    [Header("Ссылки")]
    [SerializeField] private HealthSystem playerHealthSystem; // Система здоровья игрока
    [SerializeField] private Image healthImage; // UI Image для отображения здоровья

    [Header("Спрайты здоровья (10 штук)")]
    [Tooltip("Расположи спрайты от Full_Health (индекс 0) до Death (индекс 9)")]
    [SerializeField] private Sprite[] healthSprites; // Массив спрайтов здоровья

    // Порядок спрайтов в массиве:
    // [0] = Full_Health (100% здоровья)
    // [1] = Injury_1 (90% здоровья)
    // [2] = Injury_2 (80% здоровья)
    // [3] = Injury_3 (70% здоровья)
    // [4] = Injury_4 (60% здоровья)
    // [5] = Injury_5 (50% здоровья)
    // [6] = Injury_6 (40% здоровья)
    // [7] = Injury_7 (30% здоровья)
    // [8] = Injury_8 (20% здоровья и меньше)
    // [9] = Death (0% здоровья)

    // Инициализация
    private void Start()
    {
        // Проверяем, что все ссылки назначены
        if (playerHealthSystem == null)
        {
           // Пытаемся найти HealthSystem на игроке автоматически
           GameObject player = GameObject.FindGameObjectWithTag("Player");
           if (player != null)
            {
                playerHealthSystem = player.GetComponent<HealthSystem>();
                if (playerHealthSystem != null)
                {
                    Debug.Log("HealthUI: Автоматически найдена система здоровья игрока");
                    
                }
                else
                {
                    Debug.LogError("HealthUI: На игроке нет компонента HealthSystem!");
                }
            } 
            else
            {
                Debug.LogError("HealthUI: Игрок с тегом 'Player' не найден!");
            }
        }

        if (healthImage == null)
        {
            Debug.LogError("HealthUI: Health Image не назначен! Добавь UI Image в Inspector");
        } 

        if (healthSprites == null || healthSprites.Length != 10)
        {
            Debug.LogError("HealthUI: Нужно ровно 10 спрайтов здоровья! Сейчас их:" + (healthSprites?.Length ?? 0));
        }

        // Подписываемся на событие изменения здоровья
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged += PlayerHealthSystem_OnHealthChanged;
            playerHealthSystem.OnDead += PlayerHealthSystem_OnDead;

            // Показываем начальное состояние здоровья
            UpdateHealthUI();
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта (важно!)
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged -= PlayerHealthSystem_OnHealthChanged;
            playerHealthSystem.OnDead -= PlayerHealthSystem_OnDead;
        }
    }

    // Обработчики событий

    // Вызывается когда здоровье игрока изменилось
    private void PlayerHealthSystem_OnHealthChanged(object sender, System.EventArgs e)
    {
        UpdateHealthUI();
    }

    // Вызывается когда игрок умер
    private void PlayerHealthSystem_OnDead(object sender, System.EventArgs e)
    {
        UpdateHealthUI();
        Debug.Log("HealthUI: Игрок умер! Показываем спрайт смерти");
    }

    // Обновление UI

    // Обновить отображение здоровья
    private void UpdateHealthUI()
    {
        if (playerHealthSystem == null || healthImage == null || healthSprites == null)
        {
            return;
        }
    

        // Получаем процент здоровья (от 0.0 до 1.0)
        float healthPercent = playerHealthSystem.GetHealthNormalized();

        // Выбираем правильный спрайт в зависимости от процента здоровья
        int spriteIndex = GetHealthSpriteIndex(healthPercent);

        // Применяем спрайт к UI Image
        if (spriteIndex >= 0 && spriteIndex < healthSprites.Length)
        {
        healthImage.sprite = healthSprites[spriteIndex];
        Debug.Log($"HealthUI: Здоровье {healthPercent:P0}, показываем спрайт # {spriteIndex}");
        }
    }

// Определить какой спрайт показывать в зависимости от процента здоровья
private int GetHealthSpriteIndex(float healthPercent)
    {
        // Если здоровье 0 или меньше - показываем Death
        if (healthPercent <= 0f)
        {
            return 9; // Death
        }

        // Если здоровье 100% - показываем Full_Health
        else if (healthPercent >= 1f)
        {
            return 0; //Full_Health
        }

        // Для остальных случаев вычисляем индекс
        else
        {
            // healthPercent от 0.01 до 0.99
            // Делим на 9 уровней: 90%, 80%, 70%, 60%, 50%, 40%, 30%, 20%, 10%

            if (healthPercent > 0.9f) return 0; // 91-100% = Full_Health
            if (healthPercent > 0.8f) return 1; // 81-90% = Injuty_1
            if (healthPercent > 0.7f) return 2; // 71-80% = Injury_2
            if (healthPercent > 0.6f) return 3; // 61-70% = Injury_3 
            if (healthPercent > 0.5f) return 4; // 51-60% = Injury_4
            if (healthPercent > 0.4f) return 5; // 41-50% = Injury_5
            if (healthPercent > 0.3f) return 6; // 31-40% = Injury_6
            if (healthPercent > 0.2f) return 7; // 21-30% = Injury_7
            if (healthPercent > 0.0f) return 8; // 1-20% = Injury_8
        }

        return 9; // 0% = Death
    }


    // Тестирование в редакторе
    #if UNITY_EDITOR
    // Тестовый метод для проверки UI (Вызывается из Inspector)
    [ContextMenu("Test: Take 10 Damage")]
    public void TestTakeDamage()
    {
        if (playerHealthSystem != null)
        {
            playerHealthSystem.TakeDamage(10);
        }
    }

    [ContextMenu("Test: Heal 20")]
    public void TestHeal()
    {
        if (playerHealthSystem != null)
        {
            playerHealthSystem.Heal(20);
        }
    }
    #endif
}