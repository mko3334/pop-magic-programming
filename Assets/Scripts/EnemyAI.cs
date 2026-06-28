using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float chaseRange = 8f;

    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
        CharacterStyleManager.Instance?.ApplyToEnemies();
    }

    void FixedUpdate()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < chaseRange)
        {
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
