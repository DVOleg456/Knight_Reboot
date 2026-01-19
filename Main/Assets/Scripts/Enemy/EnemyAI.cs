using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Shadows_Quest.Utils;
using System;

public class EnemyAI : MonoBehaviour
{
    [Header("Состояния")]
    [SerializeField] private State _startingState;

    [Header("Роуминг или же враг бродит")]
    [SerializeField] private float _roamingDistanceMax = 7f;
    [SerializeField] private float _roamingDistanceMin = 3f;
    [SerializeField] private float _roamingTimerMax = 2f;

    [Header("Преследование")]
    [SerializeField] private bool _isChasingEnemy = false;
    [SerializeField] private float _chasingDistance = 4f;
    [SerializeField] private float _chasingSpeedMultiplayer = 2f;

    [Header("Атака")]
    [SerializeField] private bool _isAttackingEnemy = false;
    [SerializeField] private float _attackingDistance = 2f;
    [SerializeField] private float _attackRate = 2f;
    [SerializeField] private int _attackDamage = 10; // Урон от атаки врага
    private float _nextAttackTime = 0f;

    [Header("Звуки")]
    [SerializeField] private AudioClip _attackSound;
    [SerializeField] private AudioClip _hurtSound;
    [SerializeField] private AudioClip _deathSound;

    private NavMeshAgent _navMeshAgent;
    private HealthSystem _healthSystem; // Система здоровья врага
    private State _currentState;
    private float _roamingTimer;
    private Vector3 _roamPosition;
    private Vector3 _startingPosition;

    private float _roamingSpeed;
    private float _chasingSpeed;

    private float _nextCheckDirectionTime = 0f;
    private float _checkDirectionDuration = 0.1f;
    private Vector3 _lastPosition;

    // События
    public event EventHandler OnEnemyAttack;
    public event EventHandler OnEnemyDeath;
    public event EventHandler OnEnemyTakeHit;

    private enum State
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _currentState = _startingState;

        _roamingSpeed = _navMeshAgent.speed;
        _chasingSpeed = _navMeshAgent.speed * _chasingSpeedMultiplayer;

        // Получаем систему здоровья
        _healthSystem = GetComponent<HealthSystem>();
        if (_healthSystem != null)
        {
            _healthSystem.OnDead += HealthSystem_OnDead;
            _healthSystem.OnDamaged += HealthSystem_OnDamaged;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: HealthSystem не найден!");
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (_healthSystem != null)
        {
            _healthSystem.OnDead -= HealthSystem_OnDead;
            _healthSystem.OnDamaged -= HealthSystem_OnDamaged;
        }
    }

    // Обработка смерти врага
    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        _currentState = State.Death;
        _navMeshAgent.ResetPath();
        _navMeshAgent.enabled = false;

        // Вызываем событие смерти
        OnEnemyDeath?.Invoke(this, EventArgs.Empty);

        // Воспроизводим звук смерти
        if (_deathSound != null)
        {
            AudioSource.PlayClipAtPoint(_deathSound, transform.position, 0.8f);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyDeath();
        }

        Debug.Log($"{gameObject.name}: Враг умер!");

        // Уничтожаем врага через 2 секунды (после анимации)
        Destroy(gameObject, 2f);
    }

    // Обработка получения урона
    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        OnEnemyTakeHit?.Invoke(this, EventArgs.Empty);

        // Воспроизводим звук получения урона
        if (_hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(_hurtSound, transform.position, 0.7f);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyHurt();
        }
    }

    private void Update()
    {
        StateHandler();
        MovementDirectionHandler();
    }

    private void StateHandler()
    {
        switch (_currentState)
        {
            case State.Roaming:
                _roamingTimer -= Time.deltaTime;
                if (_roamingTimer < 0)
                {
                    Roaming();
                    _roamingTimer = _roamingTimerMax;
                }
                CheckCurrentState();
                break;
            case State.Chasing:
                ChasingTarget();
                CheckCurrentState();
                break;
            case State.Attacking:
                AttackingTarget();
                CheckCurrentState();
                break;
            case State.Death:
                break;
            default:
            case State.Idle:
                break;
        }
    }

    private void ChasingTarget()
    {
        _navMeshAgent.SetDestination(Player.Instance.transform.position);
    }

    public float GetRoamingAnimationSpeed()
    {
        return _navMeshAgent.speed / _roamingSpeed;
    }

    private void CheckCurrentState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        State newState = State.Roaming;

        if (_isChasingEnemy)
        {
            if (distanceToPlayer <= _chasingDistance)
            {
                newState = State.Chasing;
            }
        }

        if (_isAttackingEnemy)
        {
            if (distanceToPlayer <= _attackingDistance)
            {
                newState = State.Attacking;
            }
        }

        if (newState != _currentState)
        {
            if (newState == State.Chasing)
            {
                _navMeshAgent.ResetPath();
                _navMeshAgent.speed = _chasingSpeed;
            }
            else if (newState == State.Roaming)
            {
                _roamingTimer = 0f;
                _navMeshAgent.speed = _roamingSpeed;
            }
            else if (newState == State.Attacking)
            {
                _navMeshAgent.ResetPath();
            }

            _currentState = newState;
        }
    }

    public bool IsRunning()
    {
        if (_navMeshAgent.velocity == Vector3.zero)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void AttackingTarget()
    {
        if (Time.time > _nextAttackTime)
        {
            // Вызываем событие атаки (для анимации)
            OnEnemyAttack?.Invoke(this, EventArgs.Empty);

            // Воспроизводим звук атаки
            if (_attackSound != null)
            {
                AudioSource.PlayClipAtPoint(_attackSound, transform.position, 0.6f);
            }
            else if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayEnemyAttack();
            }

            // Наносим урон игроку
            if (Player.Instance != null && !Player.Instance.IsDead())
            {
                HealthSystem playerHealth = Player.Instance.GetHealthSystem();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(_attackDamage);
                    Debug.Log($"{gameObject.name}: Нанесён урон игроку ({_attackDamage} HP)");
                }
            }

            _nextAttackTime = Time.time + _attackRate;
        }
    }

    private void MovementDirectionHandler()
    {
        if (Time.time > _nextCheckDirectionTime)
        {
            if (IsRunning())
            {
                ChangeFacingDirection(_lastPosition, transform.position);
            }
            else if (_currentState == State.Attacking)
            {
                ChangeFacingDirection(transform.position, Player.Instance.transform.position);
            }

            _lastPosition = transform.position;
            _nextCheckDirectionTime = Time.time + _checkDirectionDuration;
        }
    }

    private void Roaming()
    {
        _startingPosition = transform.position;
        _roamPosition = GetRoamingPosition();
        ChangeFacingDirection(_startingPosition, _roamPosition);
        _navMeshAgent.SetDestination(_roamPosition);
    }

    private Vector3 GetRoamingPosition()
    {
        return _startingPosition + Utils.GetRandomDir() * UnityEngine.Random.Range(_roamingDistanceMin, _roamingDistanceMax);
    }

    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition)
    {
        if (sourcePosition.x > targetPosition.x)
        {
            transform.rotation = Quaternion.Euler(0, -180, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    // Проверка состояния атаки
    public bool IsAttacking()
    {
        return _currentState == State.Attacking;
    }

    // Проверка состояния смерти
    public bool IsDead()
    {
        return _currentState == State.Death;
    }

    // Получить систему здоровья
    public HealthSystem GetHealthSystem()
    {
        return _healthSystem;
    }
}