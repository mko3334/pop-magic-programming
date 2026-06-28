using System.Collections;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHP = 30f;
    public float currentHP;
    public int expReward = 20;

    void Start() { currentHP = maxHP; }

    public void TakeDamage(float amount, Color? popupColor = null)
    {
        currentHP -= amount;
        DamagePopup.Show(transform.position + Vector3.up * 0.3f, amount, popupColor ?? Color.white);
        if (currentHP <= 0f) Die();
    }

    void Die()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.GetComponent<PlayerStats>()?.GainEXP(expReward);
        StartCoroutine(DeathEffect());
    }

    IEnumerator DeathEffect()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.white;
            yield return null;
            sr.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            yield return new WaitForSeconds(0.12f);
        }
        Destroy(gameObject);
    }
}
