using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// Менеджер пролога с эффектом печатной машинки
public class PrologueManager : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI prologueText;
    [SerializeField] private TextMeshProUGUI skipText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Настройки текста")]
    [SerializeField] private float characterDelay = 0.05f; // Задержка между буквами
    [SerializeField] private float paragraphDelay = 1.5f; // Задержка между абзацами
    [SerializeField] private float endDelay = 3f; // Задержка после окончания

    [Header("Настройки перехода")]
    [SerializeField] private string nextSceneName = "Level_1";
    [SerializeField] private float fadeOutDuration = 2f;

    [Header("Звук (опционально)")]
    [SerializeField] private AudioSource typingSound;
    [SerializeField] private AudioSource musicSource;

    // Текст пролога разбитый на абзацы
    private string[] prologueParagraphs = new string[]
    {
        "В начале времён существовал лишь Свет.",

        "Амулет Света хранил равновесие мира,\nне позволяя Тьме поглотить всё живое.",

        "Веками он защищал людей, животных и саму землю.",

        "Но ничто не вечно...",

        "Тёмные силы нашли способ разрушить амулет.\nВ одно мгновение свет погас.",

        "Осколки амулета разлетелись по самым\nопасным уголкам мира.",

        "Тьма начала поглощать всё на своём пути.",

        "Но из самой тьмы родилось нечто неожиданное —\nТень, порождённый угасающим светом амулета.",

        "Ни свет, ни тьма. Нечто между.",

        "Только Тень может пройти там,\nгде не выживет ни один смертный.",

        "Только он способен найти осколки\nи возродить Амулет Света.",

        "Мир на грани гибели.",

        "Время уходит.",

        "Твой путь начинается сейчас...",

        "\n\n<size=100%>S H A D O W ' S   Q U E S T</size>"
    };

    private bool isSkipping = false;
    private bool prologueFinished = false;
    private Coroutine prologueCoroutine;

    private void Start()
    {
        // Инициализация
        if (prologueText != null)
        {
            prologueText.text = "";
        }

        if (skipText != null)
        {
            skipText.text = "Нажмите SPACE или ENTER чтобы пропустить";
            skipText.alpha = 0.5f;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // Запускаем пролог
        prologueCoroutine = StartCoroutine(PlayPrologue());
    }

    private void Update()
    {
        // Пропуск пролога
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (!prologueFinished)
            {
                SkipPrologue();
            }
        }

        // Также можно пропустить кликом мыши
        if (Input.GetMouseButtonDown(0))
        {
            if (!prologueFinished && isSkipping)
            {
                // Второй клик — полный пропуск
                FinishPrologue();
            }
            else if (!prologueFinished)
            {
                // Первый клик — ускорение
                isSkipping = true;
            }
        }
    }

    private IEnumerator PlayPrologue()
    {
        yield return new WaitForSeconds(1f); // Начальная пауза

        foreach (string paragraph in prologueParagraphs)
        {
            // Очищаем текст перед новым абзацем
            prologueText.text = "";

            // Печатаем текст посимвольно
            yield return StartCoroutine(TypeText(paragraph));

            // Пауза между абзацами
            float delay = isSkipping ? paragraphDelay * 0.3f : paragraphDelay;
            yield return new WaitForSeconds(delay);
        }

        // Пролог закончен
        prologueFinished = true;

        if (skipText != null)
        {
            skipText.text = "Нажмите любую клавишу...";
        }

        yield return new WaitForSeconds(endDelay);

        // Переход к игре
        StartCoroutine(FadeOutAndLoadScene());
    }

    private IEnumerator TypeText(string text)
    {
        prologueText.text = "";

        foreach (char c in text)
        {
            prologueText.text += c;

            // Звук печати (если есть)
            if (typingSound != null && c != ' ' && c != '\n')
            {
                typingSound.Play();
            }

            // Задержка между символами
            float delay = isSkipping ? characterDelay * 0.2f : characterDelay;

            // Для знаков препинания — дольше пауза
            if (c == '.' || c == ',' || c == '!' || c == '?')
            {
                delay *= 3f;
            }
            else if (c == '\n')
            {
                delay *= 2f;
            }

            yield return new WaitForSeconds(delay);
        }
    }

    private void SkipPrologue()
    {
        if (isSkipping)
        {
            // Уже ускорено — пропускаем полностью
            FinishPrologue();
        }
        else
        {
            // Ускоряем
            isSkipping = true;
            if (skipText != null)
            {
                skipText.text = "Нажмите ещё раз чтобы пропустить";
            }
        }
    }

    private void FinishPrologue()
    {
        // Останавливаем текущую корутину
        if (prologueCoroutine != null)
        {
            StopCoroutine(prologueCoroutine);
        }

        prologueFinished = true;

        // Сразу переходим к игре
        StartCoroutine(FadeOutAndLoadScene());
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        // Плавное затемнение
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);

                // Также затухание музыки
                if (musicSource != null)
                {
                    musicSource.volume = 1f - (elapsed / fadeOutDuration);
                }

                yield return null;
            }
        }

        // Загружаем следующую сцену
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"PrologueManager: Сцена '{nextSceneName}' не найдена!");
        }
    }

    // Публичный метод для вызова из кнопки
    public void SkipButton()
    {
        FinishPrologue();
    }
}
