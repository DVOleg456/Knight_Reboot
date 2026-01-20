using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
 
// Менеджер эпилога - финальная история и титры
public class EpilogueManager : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI epilogueText;
    [SerializeField] private TextMeshProUGUI skipText;
    [SerializeField] private CanvasGroup canvasGroup;
 
    [Header("Настройки текста")]
    [SerializeField] private float characterDelay = 0.05f;
    [SerializeField] private float paragraphDelay = 2f;
    [SerializeField] private float creditsScrollSpeed = 50f; // Скорость прокрутки титров
 
    [Header("Настройки перехода")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float fadeOutDuration = 2f;
 
    [Header("Звук (опционально)")]
    [SerializeField] private AudioSource musicSource;
 
    // Финальная история
    private string[] epilogueParagraphs = new string[]
    {
        "Тень собрал все осколки Амулета Света...",
 
        "Древняя магия пробудилась вновь.",
 
        "Свет вернулся в мир,\nизгоняя тьму из каждого уголка.",
 
        "Там, где царила безнадёжность,\nснова расцвела жизнь.",
 
        "Тень выполнил своё предназначение.",
 
        "Но что станет с существом,\nрождённым из тьмы,\nкогда тьмы больше нет?",
 
        "Говорят, в самые тёмные ночи\nможно увидеть силуэт —",
 
        "Хранителя, вечно стоящего на страже,\nготового защитить мир снова.",
 
        "Ибо пока есть свет —\nбудет и тень.",
 
        "\n\n<size=150%>К О Н Е Ц</size>"
    };
 
    // Титры
    private string creditsText = @"
 
<size=80%>S H A D O W ' S   Q U E S T</size>
 
 
<size=60%>РАЗРАБОТКА</size>
 
Программирование
Хальченко Олег
(тг @Khalchenko_Oleg)
 
Шокиров Зик
(тг @aryanR1a)
 
Геймдизайн
Хальченко Олег
(тг @Khalchenko_Oleg)
 
Художник
Хальченко Олег
(тг @Khalchenko_Oleg)
 
 
<size=60%>ГРАФИКА</size>
 
Персонажи и анимации
Shadow's Quest Asset Pack
 
Тайлсеты и окружение
Shadow's Quest Asset Pack
 
 
<size=60%>МУЗЫКА И ЗВУКИ</size>
 
Музыкальное оформление
1. Adventure by Alexander Nakarada |
https://creatorchords.com
Music promoted by https://www.chosic.com/free-music/all/
Attribution 4.0 International (CC BY 4.0)
https://creativecommons.org/licenses/by/4.0/
 
2. The Great Battle by Alexander Nakarada |
https://creatorchords.com
Attribution 4.0 International (CC BY 4.0)
https://creativecommons.org/licenses/by/4.0/
Music promoted by https://www.chosic.com/free-music/all/
 
3. Glory Eternal by Darren Curtis |
https://www.darrencurtismusic.com/
Music promoted by https://www.chosic.com/free-music/all/
Creative Commons CC BY 3.0
https://creativecommons.org/licenses/by/3.0/
 
4. EpicBattle J by PeriTune |
https://peritune.com/
Music promoted by https://www.chosic.com/free-music/all/
Creative Commons CC BY 4.0
https://creativecommons.org/licenses/by/4.0/
 
5. Charlotte by Damiano Baldoni |
https://soundcloud.com/damiano_baldoni
Music promoted by https://www.chosic.com/free-music/all/
Creative Commons CC BY 4.0
https://creativecommons.org/licenses/by/4.0/
 
Звуковые эффекты
[Источник звуков]
 
 
<size=60%>Благодарности</size>
 
Спасибо Unity Technologies
За потрясающий игровой движок
 
Особая благодарность за видеокурс
по программированию
ютуберу TinyGames,
название курса:
2D Unity Game | Knight Adventure
 
А также огромная благодарность
за гайд по созданию Tilemap в Aseprite
ютуберу NIBICU, название курса:
Aseprite | Гайды | Рисования
 
И благодарю всех игроков
За то, что играете в нашу игру!
 
 
<size=60%>ИНСТРУМЕНТЫ</size>
 
Unity 6
TextMeshPro
Visual Studio Code
Visual Studio Community 2026
Aseprite
Claude Code
Nano Banana (задний фон главного меню)
 
";
 
    private bool isSkipping = false;
    private bool epilogueFinished = false;
    private bool showingCredits = false;
    private Coroutine currentCoroutine;
 
    private RectTransform creditsRectTransform;
    private float creditsStartY;
 
    private void Start()
    {
        if (epilogueText != null)
        {
            epilogueText.text = "";
        }
 
        if (skipText != null)
        {
            skipText.text = "Нажмите SPACE чтобы пропустить";
            skipText.alpha = 0.5f;
        }
 
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
 
        // Запускаем эпилог
        currentCoroutine = StartCoroutine(PlayEpilogue());
    }
 
    private void Update()
    {
        // Пропуск
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (!epilogueFinished)
            {
                SkipEpilogue();
            }
        }
 
        // Ускорение титров
        if (showingCredits && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)))
        {
            // Ускоренная прокрутка
        }
    }
 
    private IEnumerator PlayEpilogue()
    {
        yield return new WaitForSeconds(1f);
 
        // Показываем финальную историю
        foreach (string paragraph in epilogueParagraphs)
        {
            epilogueText.text = "";
            yield return StartCoroutine(TypeText(paragraph));
 
            float delay = isSkipping ? paragraphDelay * 0.3f : paragraphDelay;
            yield return new WaitForSeconds(delay);
        }
 
        // Пауза перед титрами
        yield return new WaitForSeconds(2f);
 
        // Показываем титры
        showingCredits = true;
        yield return StartCoroutine(ShowCredits());
 
        // Финал
        epilogueFinished = true;
        yield return new WaitForSeconds(3f);
 
        // Переход в главное меню
        StartCoroutine(FadeOutAndGoToMenu());
    }
 
    private IEnumerator TypeText(string text)
    {
        epilogueText.text = "";
 
        foreach (char c in text)
        {
            epilogueText.text += c;
 
            float delay = isSkipping ? characterDelay * 0.2f : characterDelay;
 
            if (c == '.' || c == ',' || c == '!' || c == '?' || c == '—')
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
 
    private IEnumerator ShowCredits()
    {
        // Показываем титры с прокруткой снизу вверх
        epilogueText.text = creditsText;
        epilogueText.alignment = TextAlignmentOptions.Top;
 
        // Получаем RectTransform для прокрутки
        RectTransform textRect = epilogueText.GetComponent<RectTransform>();
        float startY = textRect.anchoredPosition.y;
        float textHeight = epilogueText.preferredHeight;
        float screenHeight = Screen.height;
 
        // Начинаем снизу экрана
        textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, -screenHeight);
 
        float targetY = textHeight + screenHeight;
        float currentY = -screenHeight;
 
        while (currentY < targetY)
        {
            float speed = creditsScrollSpeed;
 
            // Ускорение при зажатии
            if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            {
                speed *= 3f;
            }
 
            currentY += speed * Time.deltaTime;
            textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, currentY);
 
            yield return null;
        }
    }
 
    private void SkipEpilogue()
    {
        if (isSkipping)
        {
            // Полный пропуск
            FinishEpilogue();
        }
        else
        {
            isSkipping = true;
            if (skipText != null)
            {
                skipText.text = "Нажмите ещё раз чтобы пропустить";
            }
        }
    }
 
    private void FinishEpilogue()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
 
        epilogueFinished = true;
        StartCoroutine(FadeOutAndGoToMenu());
    }
 
    private IEnumerator FadeOutAndGoToMenu()
    {
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
 
                if (musicSource != null)
                {
                    musicSource.volume = 1f - (elapsed / fadeOutDuration);
                }
 
                yield return null;
            }
        }
 
        // Переход в главное меню
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
        else if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
 
    // Публичный метод для кнопки "Пропустить"
    public void SkipButton()
    {
        FinishEpilogue();
    }
}