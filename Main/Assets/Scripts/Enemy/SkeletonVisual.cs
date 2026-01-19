using UnityEngine;
using System;

// Визуальный контроллер для скелета-бомбардира
// Управляет анимациями на основе событий от BomberEnemyAI
public class SkeletonVisual : MonoBehaviour
{
    [SerializeField] private BomberEnemyAI _enemyAI;
    private Animator _animator;

    // Параметры аниматора (такие же как у гоблина для совместимости)
    private const string IS_RUNNING = "IsRunning";
    private const string CHASING_SPEED_MULTIPLAYER = "chasingSpeedMultiplayer";
    private const string IS_DEAD = "IsDead";
    private const string ATTACK_TRIGGER = "Attack";
    private const string TAKE_HIT_TRIGGER = "TakeHit";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnEnemyAttack += EnemyAI_OnEnemyAttack;
            _enemyAI.OnEnemyDeath += EnemyAI_OnEnemyDeath;
            _enemyAI.OnEnemyTakeHit += EnemyAI_OnEnemyTakeHit;
        }
    }

    private void OnDestroy()
    {
        if (_enemyAI != null)
        {
            _enemyAI.OnEnemyAttack -= EnemyAI_OnEnemyAttack;
            _enemyAI.OnEnemyDeath -= EnemyAI_OnEnemyDeath;
            _enemyAI.OnEnemyTakeHit -= EnemyAI_OnEnemyTakeHit;
        }
    }

    private void Update()
    {
        if (_enemyAI == null || _enemyAI.IsDead()) return;

        _animator.SetBool(IS_RUNNING, _enemyAI.IsRunning());
        _animator.SetFloat(CHASING_SPEED_MULTIPLAYER, _enemyAI.GetRoamingAnimationSpeed());
    }

    private void EnemyAI_OnEnemyAttack(object sender, EventArgs e)
    {
        if (_animator != null)
        {
            _animator.SetTrigger(ATTACK_TRIGGER);
        }
    }

    private void EnemyAI_OnEnemyDeath(object sender, EventArgs e)
    {
        if (_animator != null)
        {
            _animator.SetBool(IS_DEAD, true);
        }
    }

    private void EnemyAI_OnEnemyTakeHit(object sender, EventArgs e)
    {
        if (_animator != null)
        {
            _animator.SetTrigger(TAKE_HIT_TRIGGER);
        }
    }
}
