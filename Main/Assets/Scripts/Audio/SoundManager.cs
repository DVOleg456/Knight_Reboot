using UnityEngine;
using System.Collections.Generic;

// Менеджер звуков - синглтон для воспроизведения звуковых эффектов
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private float _sfxVolume = 1f;
    [SerializeField] private int _maxSimultaneousSounds = 10;

    [Header("Звуки игрока")]
    [SerializeField] private AudioClip[] _swordSwingSounds;
    [SerializeField] private AudioClip[] _swordHitSounds;
    [SerializeField] private AudioClip[] _footstepSounds;
    [SerializeField] private AudioClip[] _playerHurtSounds;
    [SerializeField] private AudioClip _playerDeathSound;

    [Header("Звуки врагов")]
    [SerializeField] private AudioClip[] _enemyHurtSounds;
    [SerializeField] private AudioClip[] _enemyDeathSounds;
    [SerializeField] private AudioClip[] _enemyAttackSounds;

    [Header("Звуки снарядов")]
    [SerializeField] private AudioClip _arrowShootSound;
    [SerializeField] private AudioClip _arrowHitSound;
    [SerializeField] private AudioClip _bombThrowSound;
    [SerializeField] private AudioClip _bombExplosionSound;

    [Header("Звуки интерфейса")]
    [SerializeField] private AudioClip _buttonClickSound;
    [SerializeField] private AudioClip _pickupSound;

    // Пул аудио источников
    private List<AudioSource> _audioSourcePool = new List<AudioSource>();
    private int _currentPoolIndex = 0;

    private void Awake()
    {
        // Синглтон
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Создаём пул аудио источников
        CreateAudioSourcePool();
    }

    private void CreateAudioSourcePool()
    {
        for (int i = 0; i < _maxSimultaneousSounds; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _audioSourcePool.Add(source);
        }
    }

    // Получить свободный аудио источник из пула
    private AudioSource GetFreeAudioSource()
    {
        // Ищем свободный источник
        for (int i = 0; i < _audioSourcePool.Count; i++)
        {
            int index = (_currentPoolIndex + i) % _audioSourcePool.Count;
            if (!_audioSourcePool[index].isPlaying)
            {
                _currentPoolIndex = (index + 1) % _audioSourcePool.Count;
                return _audioSourcePool[index];
            }
        }

        // Если все заняты, используем следующий по кругу
        _currentPoolIndex = (_currentPoolIndex + 1) % _audioSourcePool.Count;
        return _audioSourcePool[_currentPoolIndex];
    }

    // Воспроизвести звук
    public void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetFreeAudioSource();
        source.clip = clip;
        source.volume = _masterVolume * _sfxVolume * volumeMultiplier;
        source.pitch = 1f;
        source.Play();
    }

    // Воспроизвести звук с вариацией высоты
    public void PlaySoundWithPitchVariation(AudioClip clip, float volumeMultiplier = 1f, float pitchVariation = 0.1f)
    {
        if (clip == null) return;

        AudioSource source = GetFreeAudioSource();
        source.clip = clip;
        source.volume = _masterVolume * _sfxVolume * volumeMultiplier;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.Play();
    }

    // Воспроизвести случайный звук из массива
    public void PlayRandomSound(AudioClip[] clips, float volumeMultiplier = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        PlaySound(clips[Random.Range(0, clips.Length)], volumeMultiplier);
    }

    // Воспроизвести звук в позиции (3D звук)
    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, _masterVolume * _sfxVolume * volumeMultiplier);
    }

    // Удобные методы для конкретных звуков

    // Игрок
    public void PlaySwordSwing()
    {
        PlayRandomSound(_swordSwingSounds, 0.7f);
    }

    public void PlaySwordHit()
    {
        PlayRandomSound(_swordHitSounds, 0.8f);
    }

    public void PlayFootstep()
    {
        PlayRandomSound(_footstepSounds, 0.3f);
    }

    public void PlayPlayerHurt()
    {
        PlayRandomSound(_playerHurtSounds, 0.9f);
    }

    public void PlayPlayerDeath()
    {
        PlaySound(_playerDeathSound, 1f);
    }

    // Враги
    public void PlayEnemyHurt()
    {
        PlayRandomSound(_enemyHurtSounds, 0.7f);
    }

    public void PlayEnemyDeath()
    {
        PlayRandomSound(_enemyDeathSounds, 0.8f);
    }

    public void PlayEnemyAttack()
    {
        PlayRandomSound(_enemyAttackSounds, 0.6f);
    }

    // Снаряды
    public void PlayArrowShoot()
    {
        PlaySound(_arrowShootSound, 0.6f);
    }

    public void PlayArrowHit()
    {
        PlaySound(_arrowHitSound, 0.5f);
    }

    public void PlayBombThrow()
    {
        PlaySound(_bombThrowSound, 0.7f);
    }

    public void PlayBombExplosion()
    {
        PlaySound(_bombExplosionSound, 1f);
    }

    // UI
    public void PlayButtonClick()
    {
        PlaySound(_buttonClickSound, 0.5f);
    }

    public void PlayPickup()
    {
        PlaySound(_pickupSound, 0.7f);
    }

    // Настройки громкости

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }

    public float GetMasterVolume() => _masterVolume;
    public float GetSFXVolume() => _sfxVolume;
}