using UnityEngine;
using UnityEngine.SceneManagement;

// Телепортер для перехода между сценами/уровнями
public class Teleporter : MonoBehaviour
{
    [Header("Настройки телепорта")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private bool isFinalTeleporter = false;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Визуальные настройки")]
    [SerializeField] private Animator teleportAnimator;
    [SerializeField] private float teleportDelay = 1f;

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioSource teleportSound;

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

        if (teleportSound != null)
        {
            teleportSound.Play();
        }

        if (teleportAnimator != null)
        {
            foreach (AnimatorControllerParameter param in teleportAnimator.parameters)
            {
                if (param.name == "Activate" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    teleportAnimator.SetTrigger("Activate");
                    break;
                }
            }
        }

        Invoke(nameof(PerformTeleport), teleportDelay);
    }

    private void PerformTeleport()
    {
        if (isFinalTeleporter)
        {
            Debug.Log("Teleporter: Это финальный телепортер! Игра окончена.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(true);
            }
            else
            {
                LoadScene(mainMenuSceneName);
            }
        }
        else
        {
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
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Teleporter: Сцена '{sceneName}' не найдена! Проверь Build Settings.");
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
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
