using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Контроллер главного меню
public class MainMenu : MonoBehaviour
{
    [Header("Настройки сцен")]
    [SerializeField] private string firstLevelSceneName = "Level_1";
    [SerializeField] private string secondLevelSceneName = "Level_2";

    [Header("UI элементы")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject optionsPanel;

    private void Start()
    {
        // Убеждаемся что время не остановлено
        Time.timeScale = 1f;

        // Привязываем кнопки к методам
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartNewGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
            // Показываем кнопку продолжить только если есть сохранение
            continueButton.gameObject.SetActive(HasSaveData());
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // Скрываем панель настроек
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    // Начать новую игру
    public void StartNewGame()
    {
        Debug.Log("MainMenu: Начинаем новую игру!");

        // Пробуем загрузить первый уровень
        if (Application.CanStreamedLevelBeLoaded(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else if (Application.CanStreamedLevelBeLoaded(secondLevelSceneName))
        {
            // Если первого уровня нет, загружаем второй
            SceneManager.LoadScene(secondLevelSceneName);
        }
        else
        {
            Debug.LogError($"MainMenu: Не найдена сцена уровня!");
        }
    }

    // Продолжить игру (загрузить сохранение)
    public void ContinueGame()
    {
        Debug.Log("MainMenu: Продолжаем игру!");

        // Здесь должна быть загрузка сохранения
        string lastLevel = PlayerPrefs.GetString("LastLevel", firstLevelSceneName);

        if (Application.CanStreamedLevelBeLoaded(lastLevel))
        {
            SceneManager.LoadScene(lastLevel);
        }
        else
        {
            StartNewGame();
        }
    }

    // Открыть настройки
    public void OpenOptions()
    {
        Debug.Log("MainMenu: Открываем настройки");

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    // Закрыть настройки
    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    // Выйти из игры
    public void QuitGame()
    {
        Debug.Log("MainMenu: Выход из игры");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // Загрузить уровень напрямую
    public void LoadLevel(string levelName)
    {
        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogWarning($"MainMenu: Уровень {levelName} не найден!");
        }
    }

    // Проверка наличия сохранения
    private bool HasSaveData()
    {
        return PlayerPrefs.HasKey("LastLevel");
    }
}
