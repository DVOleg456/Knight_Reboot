using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private int _maxHealth;
    private int _currentHealth;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        DetectDeath();
    }

    public void DetectDeath()
    {
        if (_currentHealth < 0)
        {
            Destroy(gameObject);
        }
    }
}
