using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("MP")]
    public float maxMP = 100f;
    public float currentMP;
    public float mpRegenRate = 5f;

    [Header("EXP / Level")]
    public int level = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 100;

    public UnityEvent onDeath;
    public UnityEvent onLevelUp;

    void Start()
    {
        var sm = SettingsManager.Instance;
        if (sm != null) { maxHP = sm.maxHP; maxMP = sm.maxMP; mpRegenRate = sm.mpRegen; }
        currentHP = maxHP;
        currentMP = maxMP;
    }

    void Update()
    {
        currentMP = Mathf.Min(currentMP + mpRegenRate * Time.deltaTime, maxMP);
    }

    public bool UseMP(float amount)
    {
        if (currentMP < amount) return false;
        currentMP -= amount;
        return true;
    }

    public void TakeDamage(float amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0f);
        if (currentHP <= 0f) onDeath?.Invoke();
    }

    public void GainEXP(int amount)
    {
        currentEXP += amount;
        if (currentEXP >= expToNextLevel)
        {
            currentEXP -= expToNextLevel;
            level++;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f);
            currentHP = maxHP;
            currentMP = maxMP;
            LevelUpEffect.Show(level);
            onLevelUp?.Invoke();
        }
    }
}
