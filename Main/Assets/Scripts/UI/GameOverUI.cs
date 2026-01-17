using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// UI для экранов Game Over и Победы
public class GameOverUI : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Кнопки Game Over")]
    [SerializeField] private Button restartButtonGameOver;
    [SerializeField] private Button mainMenuButtonGameOver;

    [Header("Кнопки Победы")]
    [SerializeField] private Button restartButtonVictory;
    [SerializeField] private Button mainMenuButtonVictory;
    [SerializeField] private Button nextLevelButton;

    [Header("Настройки")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = "";

    private void Start()
    {
        // Скрываем панели
        HideAllPanels();

        // Привязываем кнопки
        SetupButtons();

        // Подписываемся на события GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += GameManager_OnGameOver;
            GameManager.Instance.OnVictory += GameManager_OnVictory;
        }
        else
        {
            // Если нет GameManager, подписываемся на смерть игрока напрямую
            if (Player.Instance != null)
            {
                HealthSystem playerHealth = Player.Instance.GetHealthSystem();
                if (playerHealth != null)
                {
                    playerHealth.OnDead += PlayerHealth_OnDead;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= GameManager_OnGameOver;
            GameManager.Instance.OnVictory -= GameManager_OnVictory;
        }
    }

    private void SetupButtons()
    {
        // Кнопки Game Over
        if (restartButtonGameOver != null)
            restartButtonGameOver.onClick.AddListener(RestartLevel);
        if (mainMenuButtonGameOver != null)
            mainMenuButtonGameOver.onClick.AddListener(GoToMainMenu);

        // Кнопки Победы
        if (restartButtonVictory != null)
            restartButtonVictory.onClick.AddListener(RestartLevel);
        if (mainMenuButtonVictory != null)
            mainMenuButtonVictory.onClick.AddListener(GoToMainMenu);
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(GoToNextLevel);
            // Показываем кнопку только если есть следующий уровень
            nextLevelButton.gameObject.SetActive(!string.IsNullOrEmpty(nextLevelSceneName));
        }
    }

    private void GameManager_OnGameOver(object sender, System.EventArgs e)
    {
        ShowGameOverPanel();
    }

    private void GameManager_OnVictory(object sender, System.EventArgs e)
    {
        ShowVictoryPanel();
    }

    private void PlayerHealth_OnDead(object sender, System.EventArgs e)
    {
        ShowGameOverPanel();
    }

    public void ShowGameOverPanel()
    {
        Debug.Log("GameOverUI: Показываем экран Game Over");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Останавливаем время если GameManager не сделал это
        Time.timeScale = 0f;
    }

    public void ShowVictoryPanel()
    {
        Debug.Log("GameOverUI: Показываем экран Победы");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // Останавливаем время
        Time.timeScale = 0f;
    }

    public void HideAllPanels()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void RestartLevel()
    {
        Debug.Log("GameOverUI: Перезапуск уровня");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("GameOverUI: Переход в главное меню");

        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning($"GameOverUI: Сцена {mainMenuSceneName} не найдена!");
        }
    }

    public void GoToNextLevel()
    {
        Debug.Log($"GameOverUI: Переход на уровень {nextLevelSceneName}");

        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextLevelSceneName) && Application.CanStreamedLevelBeLoaded(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.LogWarning($"GameOverUI: Сцена {nextLevelSceneName} не найдена!");
        }
    }
}
