using System.Collections;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    public void StartBurn(float duration, float dps)
    {
        StopAllCoroutines();
        StartCoroutine(DoBurn(duration, dps));
    }

    IEnumerator DoBurn(float duration, float dps)
    {
        var stats = GetComponent<EnemyStats>();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
            if (stats != null)
                stats.TakeDamage(dps * 0.5f, new Color(1f, 0.4f, 0.1f));
        }
        Destroy(this);
    }
}
