using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlowEffect : MonoBehaviour
{
    float originalDrag;
    bool active;

    public void StartSlow(float duration)
    {
        if (!active)
        {
            active = true;
            originalDrag = GetComponent<Rigidbody2D>().linearDamping;
        }
        StopAllCoroutines();
        GetComponent<Rigidbody2D>().linearDamping = 10f;
        StartCoroutine(EndSlow(duration));
    }

    IEnumerator EndSlow(float duration)
    {
        yield return new WaitForSeconds(duration);
        var rb2 = GetComponent<Rigidbody2D>();
        if (rb2 != null) rb2.linearDamping = originalDrag;
        Destroy(this);
    }
}
