using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();
                if (_instance == null)
                {
                    GameObject soundManagerObj = new GameObject("SoundManager");
                    _instance = soundManagerObj.AddComponent<SoundManager>();
                }
            }
            return _instance;
        }
    }

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

    private List<AudioSource> _audioSourcePool = new List<AudioSource>();
    private int _currentPoolIndex = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSoundsFromResources();
        CreateAudioSourcePool();
    }

    private void LoadSoundsFromResources()
    {
        if (_swordHitSounds == null || _swordHitSounds.Length == 0)
        {
            AudioClip hitSword = Resources.Load<AudioClip>("Sounds/Hit_sword");
            if (hitSword != null)
            {
                _swordHitSounds = new AudioClip[] { hitSword };
                _swordSwingSounds = new AudioClip[] { hitSword };
            }
        }

        if (_arrowHitSound == null)
        {
            _arrowHitSound = Resources.Load<AudioClip>("Sounds/Hit_bow");
            _arrowShootSound = _arrowHitSound;
        }

        if (_playerHurtSounds == null || _playerHurtSounds.Length == 0)
        {
            AudioClip hurtSound = Resources.Load<AudioClip>("Sounds/Hurt_sound");
            if (hurtSound != null)
            {
                _playerHurtSounds = new AudioClip[] { hurtSound };
                _enemyHurtSounds = new AudioClip[] { hurtSound };
                _enemyDeathSounds = new AudioClip[] { hurtSound };
            }
        }

        if (_bombExplosionSound == null)
        {
            _bombExplosionSound = Resources.Load<AudioClip>("Sounds/Skeleton_bomb");
            _bombThrowSound = _bombExplosionSound;
        }

        if (_pickupSound == null)
        {
            _pickupSound = Resources.Load<AudioClip>("Sounds/Bonuses_sound");
        }
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

    private AudioSource GetFreeAudioSource()
    {
        for (int i = 0; i < _audioSourcePool.Count; i++)
        {
            int index = (_currentPoolIndex + i) % _audioSourcePool.Count;
            if (!_audioSourcePool[index].isPlaying)
            {
                _currentPoolIndex = (index + 1) % _audioSourcePool.Count;
                return _audioSourcePool[index];
            }
        }
        _currentPoolIndex = (_currentPoolIndex + 1) % _audioSourcePool.Count;
        return _audioSourcePool[_currentPoolIndex];
    }

    public void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        AudioSource source = GetFreeAudioSource();
        source.clip = clip;
        source.volume = _masterVolume * _sfxVolume * volumeMultiplier;
        source.pitch = 1f;
        source.Play();
    }

    public void PlayRandomSound(AudioClip[] clips, float volumeMultiplier = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        PlaySound(clips[Random.Range(0, clips.Length)], volumeMultiplier);
    }

    public void PlaySwordSwing() => PlayRandomSound(_swordSwingSounds, 0.7f);
    public void PlaySwordHit() => PlayRandomSound(_swordHitSounds, 0.8f);
    public void PlayFootstep() => PlayRandomSound(_footstepSounds, 0.3f);
    public void PlayPlayerHurt() => PlayRandomSound(_playerHurtSounds, 0.9f);
    public void PlayPlayerDeath() => PlaySound(_playerDeathSound, 1f);
    public void PlayEnemyHurt() => PlayRandomSound(_enemyHurtSounds, 0.7f);
    public void PlayEnemyDeath() => PlayRandomSound(_enemyDeathSounds, 0.8f);
    public void PlayEnemyAttack() => PlayRandomSound(_enemyAttackSounds, 0.6f);
    public void PlayArrowShoot() => PlaySound(_arrowShootSound, 0.6f);
    public void PlayArrowHit() => PlaySound(_arrowHitSound, 0.5f);
    public void PlayBombThrow() => PlaySound(_bombThrowSound, 0.7f);
    public void PlayBombExplosion() => PlaySound(_bombExplosionSound, 1f);
    public void PlayButtonClick() => PlaySound(_buttonClickSound, 0.5f);
    public void PlayPickup() => PlaySound(_pickupSound, 0.7f);
}
