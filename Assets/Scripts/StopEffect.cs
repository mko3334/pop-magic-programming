using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StopEffect : MonoBehaviour
{
    public void StartFreeze(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DoFreeze(duration));
    }

    IEnumerator DoFreeze(float duration)
    {
        var rb2 = GetComponent<Rigidbody2D>();
        if (rb2 == null) { Destroy(this); yield break; }
        var orig = rb2.linearDamping;
        rb2.linearVelocity = Vector2.zero;
        rb2.linearDamping = 999f;
        yield return new WaitForSeconds(duration);
        if (rb2 != null) rb2.linearDamping = orig;
        Destroy(this);
    }
}
