using UnityEngine;
using UnityEngine.SceneManagement;
using System;

// Менеджер игры - управляет состоянием игры
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Настройки сцен")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string firstLevelSceneName = "Level_1";

    [Header("UI элементы")]
    [SerializeField] private GameObject gameOverPanel; // Панель Game Over
    [SerializeField] private GameObject victoryPanel; // Панель победы
    [SerializeField] private GameObject pausePanel; // Панель паузы

    // События
    public event EventHandler OnGameOver;
    public event EventHandler OnVictory;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameResumed;

    // Состояние игры
    private bool isGameOver = false;
    private bool isPaused = false;

    private void Awake()
    {
        // Singleton паттерн
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Сохраняем между сценами
    }

    private void Start()
    {
        // Подписываемся на событие смерти игрока
        if (Player.Instance != null)
        {
            HealthSystem playerHealth = Player.Instance.GetHealthSystem();
            if (playerHealth != null)
            {
                playerHealth.OnDead += PlayerHealth_OnDead;
            }
        }

        // Скрываем UI панели
        HideAllPanels();
    }

    private void Update()
    {
        // Пауза по Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else if (!isGameOver)
            {
                PauseGame();
            }
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (Player.Instance != null)
        {
            HealthSystem playerHealth = Player.Instance.GetHealthSystem();
            if (playerHealth != null)
            {
                playerHealth.OnDead -= PlayerHealth_OnDead;
            }
        }
    }

    // Обработчик смерти игрока
    private void PlayerHealth_OnDead(object sender, EventArgs e)
    {
        EndGame(false); // false = поражение
    }

    // Конец игры
    public void EndGame(bool isVictory)
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f; // Останавливаем время

        if (isVictory)
        {
            Debug.Log("GameManager: ПОБЕДА!");
            OnVictory?.Invoke(this, EventArgs.Empty);

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }
        else
        {
            Debug.Log("GameManager: GAME OVER!");
            OnGameOver?.Invoke(this, EventArgs.Empty);

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }
    }

    // Пауза
    public void PauseGame()
    {
        if (isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        OnGamePaused?.Invoke(this, EventArgs.Empty);
        Debug.Log("GameManager: Игра на паузе");
    }

    // Продолжить игру
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        OnGameResumed?.Invoke(this, EventArgs.Empty);
        Debug.Log("GameManager: Игра продолжается");
    }

    // Перезапуск текущего уровня
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Переход в главное меню
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning($"GameManager: Сцена {mainMenuSceneName} не найдена!");
        }
    }

    // Загрузка уровня по имени
    public void LoadLevel(string levelName)
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogWarning($"GameManager: Сцена {levelName} не найдена!");
        }
    }

    // Начать новую игру
    public void StartNewGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        if (Application.CanStreamedLevelBeLoaded(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogWarning($"GameManager: Сцена {firstLevelSceneName} не найдена!");
        }
    }

    // Выход из игры
    public void QuitGame()
    {
        Debug.Log("GameManager: Выход из игры");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // Скрыть все панели UI
    private void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // Проверки состояния
    public bool IsGameOver() => isGameOver;
    public bool IsPaused() => isPaused;
}
