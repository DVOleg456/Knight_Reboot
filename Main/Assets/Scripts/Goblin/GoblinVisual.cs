using UnityEngine;

public class GoblinVisual : MonoBehaviour
{

    [SerializeField] private EnemyAI _enemyAI;
    private Animator _animator;

    private const string IS_RUNNING = "IsRunning";

    private const string CHASING_SPEED_MULTYPLAYER = "chasingSpeedMultiplayer";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        _animator.SetBool(IS_RUNNING, _enemyAI.IsRunning());
        _animator.SetFloat(CHASING_SPEED_MULTYPLAYER, _enemyAI.GetRoamingAnimationSpeed());

    }
}
