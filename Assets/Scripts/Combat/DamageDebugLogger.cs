using UnityEngine;

public class DamageDebugLogger : MonoBehaviour
{
    public void LogHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"{name} health: {currentHealth}/{maxHealth}", this);
    }

    public void LogDamaged()
    {
        Debug.Log($"{name} took damage.", this);
    }

    public void LogDied()
    {
        Debug.Log($"{name} died.", this);
    }
}
