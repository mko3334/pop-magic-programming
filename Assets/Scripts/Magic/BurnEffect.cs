using System.Collections;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    private Coroutine burnCoroutine;

    public void StartBurn(float duration, float damagePerSec)
    {
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(Burn(duration, damagePerSec));
    }

    IEnumerator Burn(float duration, float dps)
    {
        var stats = GetComponent<EnemyStats>();
        float elapsed = 0f;
        var sr = GetComponent<SpriteRenderer>();
        Color orig = sr ? sr.color : Color.white;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
            if (stats) stats.TakeDamage(dps);
            if (sr) sr.color = Color.Lerp(new Color(1f,0.3f,0.1f), orig, elapsed/duration);
        }
        if (sr) sr.color = orig;
    }
}
