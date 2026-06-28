using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHP = 30f;
    public float currentHP;
    public int expReward = 20;

    void Start() { currentHP = maxHP; }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        ShowDamage(amount);
        if (currentHP <= 0f) Die();
    }

    void ShowDamage(float amount)
    {
        // ダメージ数値（簡易）
        Debug.Log($"{gameObject.name} に {amount:F0} ダメージ！残HP: {currentHP:F0}");
    }

    void Die()
    {
        // プレイヤーにEXP付与
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.GetComponent<PlayerStats>()?.GainEXP(expReward);
        Destroy(gameObject);
    }
}
