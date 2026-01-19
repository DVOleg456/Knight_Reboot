using System;
using UnityEngine;
using UnityEngine.AI;
using Shadows_Quest.Utils;

// AI для врагов с дальней атакой (гоблин-лучник)
// Стреляет стрелами, держится на расстоянии от игрока
public class RangedEnemyAI : MonoBehaviour
{
    [Header("Состояния")]
    [SerializeField] private State _startingState = State.Roaming;

    [Header("Роуминг")]
    [SerializeField] private float _roamingDistanceMax = 7f;
    [SerializeField] private float _roamingDistanceMin = 3f;
    [SerializeField] private float _roamingTimerMax = 2f;

    [Header("Преследование/Отступление")]
    [SerializeField] private bool _isChasingEnemy = true;
    [SerializeField] private float _detectionDistance = 10f; // Дистанция обнаружения игрока
    [SerializeField] private float _chasingSpeedMultiplier = 1.5f;
    [SerializeField] private float _preferredDistance = 6f; // Предпочитаемая дистанция для стрельбы
    [SerializeField] private float _minDistance = 4f; // Минимальная дистанция (начинает отступать)

    [Header("Атака")]
    [SerializeField] private bool _isAttackingEnemy = true;
    [SerializeField] private float _attackDistance = 8f; // Дистанция стрельбы
    [SerializeField] private float _attackRate = 2f; // Скорострельность
    [SerializeField] private int _attackDamage = 15;
    private float _nextAttackTime = 0f;

    [Header("Снаряд")]
    [SerializeField] private GameObject _arrowPrefab; // Префаб стрелы
    [SerializeField] private Transform _shootPoint; // Точка выстрела

    [Header("Звуки")]
    [SerializeField] private AudioClip _shootSound;

    // Компоненты
    private NavMeshAgent _navMeshAgent;
    private HealthSystem _healthSystem;

    // Переменные состояния
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
        Chasing,     // Преследование (приближение к игроку)
        Retreating,  // Отступление (слишком близко)
        Attacking,   // Стрельба
        Death
    }

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _currentState = _startingState;

        _roamingSpeed = _navMeshAgent.speed;
        _chasingSpeed = _navMeshAgent.speed * _chasingSpeedMultiplier;

        _healthSystem = GetComponent<HealthSystem>();
        if (_healthSystem != null)
        {
            _healthSystem.OnDead += HealthSystem_OnDead;
            _healthSystem.OnDamaged += HealthSystem_OnDamaged;
        }
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDead -= HealthSystem_OnDead;
            _healthSystem.OnDamaged -= HealthSystem_OnDamaged;
        }
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        _currentState = State.Death;
        _navMeshAgent.ResetPath();
        _navMeshAgent.enabled = false;

        OnEnemyDeath?.Invoke(this, EventArgs.Empty);
        Debug.Log($"{gameObject.name}: Лучник умер!");

        Destroy(gameObject, 2f);
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        OnEnemyTakeHit?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (_currentState == State.Death) return;

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
                ChaseTarget();
                CheckCurrentState();
                break;

            case State.Retreating:
                RetreatFromTarget();
                CheckCurrentState();
                break;

            case State.Attacking:
                AttackTarget();
                CheckCurrentState();
                break;

            case State.Death:
                break;

            default:
            case State.Idle:
                CheckCurrentState();
                break;
        }
    }

    private void CheckCurrentState()
    {
        if (Player.Instance == null || Player.Instance.IsDead()) return;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        State newState = State.Roaming;

        // Проверяем обнаружение игрока
        if (_isChasingEnemy && distanceToPlayer <= _detectionDistance)
        {
            // Слишком близко - отступаем
            if (distanceToPlayer < _minDistance)
            {
                newState = State.Retreating;
            }
            // В зоне атаки - стреляем
            else if (_isAttackingEnemy && distanceToPlayer <= _attackDistance)
            {
                newState = State.Attacking;
            }
            // Далеко - преследуем
            else
            {
                newState = State.Chasing;
            }
        }

        if (newState != _currentState)
        {
            OnStateChanged(newState);
            _currentState = newState;
        }
    }

    private void OnStateChanged(State newState)
    {
        switch (newState)
        {
            case State.Chasing:
            case State.Retreating:
                _navMeshAgent.speed = _chasingSpeed;
                break;

            case State.Roaming:
                _roamingTimer = 0f;
                _navMeshAgent.speed = _roamingSpeed;
                break;

            case State.Attacking:
                _navMeshAgent.ResetPath();
                break;
        }
    }

    private void ChaseTarget()
    {
        if (Player.Instance == null) return;

        // Двигаемся к позиции на предпочитаемой дистанции
        Vector3 directionToPlayer = (Player.Instance.transform.position - transform.position).normalized;
        Vector3 targetPosition = Player.Instance.transform.position - directionToPlayer * _preferredDistance;

        _navMeshAgent.SetDestination(targetPosition);
    }

    private void RetreatFromTarget()
    {
        if (Player.Instance == null) return;

        // Отступаем от игрока
        Vector3 directionFromPlayer = (transform.position - Player.Instance.transform.position).normalized;
        Vector3 retreatPosition = transform.position + directionFromPlayer * 3f;

        _navMeshAgent.SetDestination(retreatPosition);
    }

    private void AttackTarget()
    {
        if (Player.Instance == null || Player.Instance.IsDead()) return;

        // Останавливаемся для выстрела
        _navMeshAgent.ResetPath();

        // Поворачиваемся к игроку
        ChangeFacingDirection(transform.position, Player.Instance.transform.position);

        // Проверяем кулдаун атаки
        if (Time.time >= _nextAttackTime)
        {
            ShootArrow();
            _nextAttackTime = Time.time + _attackRate;
        }
    }

    private void ShootArrow()
    {
        // Вызываем событие атаки (для анимации)
        OnEnemyAttack?.Invoke(this, EventArgs.Empty);

        // Создаём стрелу
        if (_arrowPrefab != null && Player.Instance != null)
        {
            Vector3 spawnPosition = _shootPoint != null ? _shootPoint.position : transform.position;
            Vector2 direction = (Player.Instance.transform.position - spawnPosition).normalized;

            GameObject arrowObj = Instantiate(_arrowPrefab, spawnPosition, Quaternion.identity);
            Arrow arrow = arrowObj.GetComponent<Arrow>();

            if (arrow != null)
            {
                arrow.Initialize(direction, gameObject, _attackDamage);
            }
            else
            {
                // Если нет компонента Arrow, пробуем базовый Projectile
                Projectile projectile = arrowObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(direction, gameObject, _attackDamage);
                }
            }

            Debug.Log($"{gameObject.name}: Выстрел стрелой!");

            // Воспроизводим звук
            if (_shootSound != null)
            {
                AudioSource.PlayClipAtPoint(_shootSound, transform.position);
            }
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

    private void MovementDirectionHandler()
    {
        if (Time.time > _nextCheckDirectionTime)
        {
            if (IsRunning())
            {
                ChangeFacingDirection(_lastPosition, transform.position);
            }
            else if (_currentState == State.Attacking && Player.Instance != null)
            {
                ChangeFacingDirection(transform.position, Player.Instance.transform.position);
            }

            _lastPosition = transform.position;
            _nextCheckDirectionTime = Time.time + _checkDirectionDuration;
        }
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

    public bool IsRunning()
    {
        return _navMeshAgent.velocity != Vector3.zero;
    }

    public float GetRoamingAnimationSpeed()
    {
        return _navMeshAgent.speed / _roamingSpeed;
    }

    public bool IsAttacking()
    {
        return _currentState == State.Attacking;
    }

    public bool IsDead()
    {
        return _currentState == State.Death;
    }

    public HealthSystem GetHealthSystem()
    {
        return _healthSystem;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Дистанция обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionDistance);

        // Дистанция атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackDistance);

        // Предпочитаемая дистанция
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _preferredDistance);

        // Минимальная дистанция
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _minDistance);
    }
#endif
}
