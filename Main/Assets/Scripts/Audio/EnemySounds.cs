using UnityEngine;
using System;
 
// Компонент для воспроизведения звуков врагов
// Работает с EnemyAI, RangedEnemyAI и BomberEnemyAI
public class EnemySounds : MonoBehaviour
{
    [Header("Звуки (локальные, если нет SoundManager)")]
    [SerializeField] private AudioClip[] _attackSounds;
    [SerializeField] private AudioClip[] _hurtSounds;
    [SerializeField] private AudioClip[] _deathSounds;
    [SerializeField] private AudioClip[] _footstepSounds;
 
    [Header("Настройки")]
    [SerializeField] private float _footstepInterval = 0.4f;
    [SerializeField] private float _soundVolume = 0.7f;
 
    private AudioSource _audioSource;
    private float _footstepTimer;
 
    // Ссылки на различные типы AI
    private EnemyAI _enemyAI;
    private RangedEnemyAI _rangedAI;
    private BomberEnemyAI _bomberAI;
 
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.playOnAwake = false;
 
        // Находим AI компоненты
        _enemyAI = GetComponent<EnemyAI>();
        _rangedAI = GetComponent<RangedEnemyAI>();
        _bomberAI = GetComponent<BomberEnemyAI>();
    }
 
    private void Start()
    {
        // Подписываемся на события в зависимости от типа AI
        if (_enemyAI != null)
        {
            _enemyAI.OnEnemyAttack += OnAttack;
            _enemyAI.OnEnemyTakeHit += OnTakeHit;
            _enemyAI.OnEnemyDeath += OnDeath;
        }
 
        if (_rangedAI != null)
        {
            _rangedAI.OnEnemyAttack += OnAttack;
            _rangedAI.OnEnemyTakeHit += OnTakeHit;
            _rangedAI.OnEnemyDeath += OnDeath;
        }
 
        if (_bomberAI != null)
        {
            _bomberAI.OnEnemyAttack += OnAttack;
            _bomberAI.OnEnemyTakeHit += OnTakeHit;
            _bomberAI.OnEnemyDeath += OnDeath;
        }
    }
 
    private void OnDestroy()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnEnemyAttack -= OnAttack;
            _enemyAI.OnEnemyTakeHit -= OnTakeHit;
            _enemyAI.OnEnemyDeath -= OnDeath;
        }
 
        if (_rangedAI != null)
        {
            _rangedAI.OnEnemyAttack -= OnAttack;
            _rangedAI.OnEnemyTakeHit -= OnTakeHit;
            _rangedAI.OnEnemyDeath -= OnDeath;
        }
 
        if (_bomberAI != null)
        {
            _bomberAI.OnEnemyAttack -= OnAttack;
            _bomberAI.OnEnemyTakeHit -= OnTakeHit;
            _bomberAI.OnEnemyDeath -= OnDeath;
        }
    }
 
    private void Update()
    {
        HandleFootsteps();
    }
 
    private void HandleFootsteps()
    {
        bool isRunning = false;
 
        if (_enemyAI != null) isRunning = _enemyAI.IsRunning();
        else if (_rangedAI != null) isRunning = _rangedAI.IsRunning();
        else if (_bomberAI != null) isRunning = _bomberAI.IsRunning();
 
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
            _footstepTimer = 0f;
        }
    }
 
    private void PlayFootstep()
    {
        if (_footstepSounds != null && _footstepSounds.Length > 0)
        {
            AudioClip clip = _footstepSounds[UnityEngine.Random.Range(0, _footstepSounds.Length)];
            _audioSource.PlayOneShot(clip, _soundVolume * 0.4f);
        }
    }
 
    private void OnAttack(object sender, EventArgs e)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyAttack();
            return;
        }
 
        if (_attackSounds != null && _attackSounds.Length > 0)
        {
            AudioClip clip = _attackSounds[UnityEngine.Random.Range(0, _attackSounds.Length)];
            _audioSource.PlayOneShot(clip, _soundVolume);
        }
    }
 
    private void OnTakeHit(object sender, EventArgs e)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyHurt();
            return;
        }
 
        if (_hurtSounds != null && _hurtSounds.Length > 0)
        {
            AudioClip clip = _hurtSounds[UnityEngine.Random.Range(0, _hurtSounds.Length)];
            _audioSource.PlayOneShot(clip, _soundVolume);
        }
    }
 
    private void OnDeath(object sender, EventArgs e)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyDeath();
            return;
        }
 
        if (_deathSounds != null && _deathSounds.Length > 0)
        {
            AudioClip clip = _deathSounds[UnityEngine.Random.Range(0, _deathSounds.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position, _soundVolume);
        }
    }
}