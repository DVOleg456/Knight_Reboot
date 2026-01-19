using UnityEngine;

// Компонент для воспроизведения звуков игрока
// Добавляется на игрока и подписывается на события
public class PlayerSounds : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Player _player;
    [SerializeField] private HealthSystem _healthSystem;

    [Header("Звуки (локальные, если нет SoundManager)")]
    [SerializeField] private AudioClip[] _footstepSounds;
    [SerializeField] private AudioClip[] _hurtSounds;
    [SerializeField] private AudioClip _deathSound;

    [Header("Настройки шагов")]
    [SerializeField] private float _footstepInterval = 0.35f;
    [SerializeField] private float _footstepVolume = 0.3f;

    private float _footstepTimer;
    private bool _wasRunning;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.playOnAwake = false;

        if (_player == null)
        {
            _player = GetComponent<Player>();
        }

        if (_healthSystem == null)
        {
            _healthSystem = GetComponent<HealthSystem>();
        }
    }

    private void Start()
    {
        // Подписываемся на события здоровья
        if (_healthSystem != null)
        {
            _healthSystem.OnDamaged += HealthSystem_OnDamaged;
            _healthSystem.OnDead += HealthSystem_OnDead;
        }
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDamaged -= HealthSystem_OnDamaged;
            _healthSystem.OnDead -= HealthSystem_OnDead;
        }
    }

    private void Update()
    {
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (_player == null) return;

        bool isRunning = _player.IsRunning();

        if (isRunning)
        {
            _footstepTimer -= Time.deltaTime;

            if (_footstepTimer <= 0f)
            {
                PlayFootstep();
                _footstepTimer = _footstepInterval;
            }
        }
        else
        {
            _footstepTimer = 0f; // Сразу играем шаг при начале движения
        }

        _wasRunning = isRunning;
    }

    private void PlayFootstep()
    {
        // Пробуем использовать SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayFootstep();
            return;
        }

        // Иначе используем локальные звуки
        if (_footstepSounds != null && _footstepSounds.Length > 0)
        {
            AudioClip clip = _footstepSounds[Random.Range(0, _footstepSounds.Length)];
            _audioSource.PlayOneShot(clip, _footstepVolume);
        }
    }

    private void HealthSystem_OnDamaged(object sender, System.EventArgs e)
    {
        // Пробуем использовать SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerHurt();
            return;
        }

        // Иначе используем локальные звуки
        if (_hurtSounds != null && _hurtSounds.Length > 0)
        {
            AudioClip clip = _hurtSounds[Random.Range(0, _hurtSounds.Length)];
            _audioSource.PlayOneShot(clip, 0.8f);
        }
    }

    private void HealthSystem_OnDead(object sender, System.EventArgs e)
    {
        // Пробуем использовать SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerDeath();
            return;
        }

        // Иначе используем локальный звук
        if (_deathSound != null)
        {
            _audioSource.PlayOneShot(_deathSound, 1f);
        }
    }

  
    // Воспроизвести звук атаки (вызывается из PlayerCombat или анимации)
    public void PlayAttackSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySwordSwing();
        }
    }

    // Воспроизвести звук попадания (вызывается при успешной атаке)
    public void PlayHitSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySwordHit();
        }
    }
}
