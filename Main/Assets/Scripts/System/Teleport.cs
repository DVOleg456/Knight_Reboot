using UnityEngine;
using UnityEngine.SceneManagement;

// Телепортер для перехода между сценами/уровнями
public class Teleporter : MonoBehaviour
{
    [Header("Настройки телепорта")]
    [SerializeField] private string targetSceneName; // Имя сцены для перехода
    [SerializeField] private bool isFinalTeleporter = false; // Это финальный телепортер (конец игры)?
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Имя сцены главного меню

    [Header("Визуальные настройки")]
    [SerializeField] private Animator teleportAnimator; // Аниматор телепорта (опционально)
    [SerializeField] private float teleportDelay = 1f; // Задержка перед телепортацией

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioSource teleportSound; // Звук телепортации (опционально)

    private bool isActivated = false;
    private Collider2D teleportCollider;

    private void Awake()
    {
        teleportCollider = GetComponent<Collider2D>();
        if (teleportCollider != null)
        {
            teleportCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем что это игрок и телепорт ещё не активирован
        if (isActivated) return;

        if (other.CompareTag("Player") || other.GetComponent<Player>() != null)
        {
            ActivateTeleport();
        }
    }

    private void ActivateTeleport()
    {
        isActivated = true;

        Debug.Log($"Teleporter: Игрок вошёл в телепорт!");

        // Воспроизводим звук телепортации
        if (teleportSound != null)
        {
            teleportSound.Play();
        }

        // Запускаем анимацию телепорта (если есть аниматор с триггером "Activate")
        if (teleportAnimator != null)
        {
            // Проверяем наличие параметра перед установкой
            foreach (AnimatorControllerParameter param in teleportAnimator.parameters)
            {
                if (param.name == "Activate" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    teleportAnimator.SetTrigger("Activate");
                    break;
                }
            }
        }

        // Начинаем телепортацию с задержкой
        Invoke(nameof(PerformTeleport), teleportDelay);
    }

    private void PerformTeleport()
    {
        if (isFinalTeleporter)
        {
            // Финальный телепортер - конец игры
            Debug.Log("Teleporter: Это финальный телепортер! Игра окончена.");

            // Вызываем событие конца игры через GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(true); // true = победа
            }
            else
            {
                // Если нет GameManager, просто переходим в главное меню
                LoadScene(mainMenuSceneName);
            }
        }
        else
        {
            // Переход на другой уровень
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"Teleporter: Переход на уровень {targetSceneName}");
                LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("Teleporter: Не указана целевая сцена!");
            }
        }
    }

    private void LoadScene(string sceneName)
    {
        // Проверяем существует ли сцена
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Teleporter: Сцена '{sceneName}' не найдена! Проверь Build Settings.");
        }
    }

    // Отладка в редакторе
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Рисуем зону телепорта
        Gizmos.color = isFinalTeleporter ? Color.yellow : Color.cyan;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
#endif
}