using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MagicProjectile : MonoBehaviour
{
    [Header("基本")]
    public float speed = 8f;
    public float lifetime = 4f;
    public float damage = 15f;

    [Header("ブロック設定")]
    public List<BlockType> blockSequence = new List<BlockType>();

    // 内部状態
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private int blockIndex = 0;

    // ベクトル系
    private bool isHoming;
    private bool isZigzag;
    private bool isBounce;
    private float zigzagTimer;
    private int bounceCount = 2;
    private Transform homingTarget;

    // 属性
    private BlockType element = BlockType.Fire;
    private bool isPiercing;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 direction, List<BlockType> blocks)
    {
        moveDir = direction.normalized;
        blockSequence = new List<BlockType>(blocks);
        ApplyBlocks();
        rb.linearVelocity = moveDir * speed;
        Destroy(gameObject, lifetime);
    }

    void ApplyBlocks()
    {
        foreach (var b in blockSequence)
        {
            switch (b)
            {
                // 属性
                case BlockType.Fire:      element = b; damage = Dmg(b); ApplyColor(new Color(1f,0.4f,0.1f)); break;
                case BlockType.Lightning: element = b; damage = Dmg(b); ApplyColor(new Color(1f,0.95f,0.1f)); break;
                case BlockType.Water:     element = b; damage = Dmg(b); ApplyColor(new Color(0.2f,0.6f,1f)); speed = 5f; break;
                case BlockType.Wood:      element = b; damage = Dmg(b); ApplyColor(new Color(0.2f,0.8f,0.2f)); break;
                case BlockType.Earth:     element = b; damage = Dmg(b); ApplyColor(new Color(0.6f,0.45f,0.2f)); speed = 4f; break;
                case BlockType.Light:     element = b; damage = Dmg(b); ApplyColor(new Color(1f,0.98f,0.65f)); speed = 14f; isPiercing = true; break;

                // ベクトル
                case BlockType.Forward:  break; // デフォルト
                case BlockType.Homing:   isHoming = true; FindHomingTarget(); break;
                case BlockType.Zigzag:   isZigzag = true; break;
                case BlockType.Bounce:   isBounce = true; break;

                // アクション（OnHit時に処理）
                case BlockType.Explode:
                case BlockType.Split:
                case BlockType.Attract:
                case BlockType.Slow:
                case BlockType.Speed:
                    break;
            }
        }

        // 雷は生成時に範囲ダメージ
        if (element == BlockType.Lightning)
            StartCoroutine(LightningSpawnEffect());
    }

    float Dmg(BlockType b)
    {
        var sm = SettingsManager.Instance;
        if (sm == null) return damage;
        return b switch
        {
            BlockType.Fire      => sm.fireDmg,
            BlockType.Lightning => sm.lightningDmg,
            BlockType.Water     => sm.waterDmg,
            BlockType.Wood      => sm.woodDmg,
            BlockType.Earth     => sm.earthDmg,
            BlockType.Light     => sm.lightDmg,
            _                   => damage,
        };
    }

    void ApplyColor(Color c)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = c;
    }

    void FindHomingTarget()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float best = float.MaxValue;
        foreach (var e in enemies)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < best) { best = d; homingTarget = e.transform; }
        }
    }

    void Update()
    {
        if (isHoming && homingTarget != null)
        {
            Vector2 toTarget = ((Vector2)homingTarget.position - (Vector2)transform.position).normalized;
            moveDir = Vector2.Lerp(moveDir, toTarget, 4f * Time.deltaTime).normalized;
            rb.linearVelocity = moveDir * speed;
        }

        if (isZigzag)
        {
            zigzagTimer += Time.deltaTime * 4f;
            Vector2 perp = new Vector2(-moveDir.y, moveDir.x);
            rb.linearVelocity = (moveDir + perp * Mathf.Sin(zigzagTimer) * 0.7f).normalized * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            if (hitEnemies.Contains(other.gameObject)) return;
            hitEnemies.Add(other.gameObject);
            HitEnemy(other.gameObject);
            if (!isPiercing) Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            if (isBounce && bounceCount > 0)
            {
                bounceCount--;
                var n = other.ClosestPoint(transform.position);
                var normal = ((Vector2)transform.position - n).normalized;
                moveDir = Vector2.Reflect(moveDir, normal);
                rb.linearVelocity = moveDir * speed;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    void HitEnemy(GameObject enemy)
    {
        // 属性別効果
        switch (element)
        {
            case BlockType.Fire:
                var burn = enemy.GetComponent<BurnEffect>() ?? enemy.AddComponent<BurnEffect>();
                burn.StartBurn(3f, 1f);
                break;
            case BlockType.Water:
                SplitProjectile();
                break;
            case BlockType.Wood:
                var rb2 = enemy.GetComponent<Rigidbody2D>();
                if (rb2) StartCoroutine(SlowEnemy(rb2, 2f));
                break;
        }

        // アクションブロック適用
        foreach (var b in blockSequence)
        {
            switch (b)
            {
                case BlockType.Explode: Explode(); break;
                case BlockType.Split:   SplitProjectile(); break;
                case BlockType.Attract: Attract(enemy); break;
            }
        }

        var stats = enemy.GetComponent<EnemyStats>();
        if (stats) stats.TakeDamage(damage);
    }

    void Explode()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var h in hits)
            if (h.CompareTag("Enemy"))
            {
                var s = h.GetComponent<EnemyStats>();
                if (s) s.TakeDamage(damage * 0.6f);
            }
        // TODO: 爆発エフェクト
    }

    void SplitProjectile()
    {
        for (int i = -1; i <= 1; i += 2)
        {
            var splitDir = Quaternion.Euler(0, 0, 30f * i) * (Vector3)moveDir;
            var go = Instantiate(gameObject, transform.position, Quaternion.identity);
            var mp = go.GetComponent<MagicProjectile>();
            if (mp != null)
            {
                mp.damage = damage * 0.5f;
                mp.blockSequence = new List<BlockType>(); // 分裂弾はシンプルに
                mp.Launch(splitDir, new List<BlockType>());
            }
        }
    }

    void Attract(GameObject target)
    {
        var rb2 = target.GetComponent<Rigidbody2D>();
        if (rb2)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)target.transform.position).normalized;
            rb2.AddForce(dir * 8f, ForceMode2D.Impulse);
        }
    }

    IEnumerator SlowEnemy(Rigidbody2D rb2, float duration)
    {
        var originalDrag = rb2.linearDamping;
        rb2.linearDamping = 10f;
        yield return new WaitForSeconds(duration);
        if (rb2) rb2.linearDamping = originalDrag;
    }

    IEnumerator LightningSpawnEffect()
    {
        yield return new WaitForSeconds(0.05f);
        var hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var h in hits)
            if (h.CompareTag("Enemy"))
            {
                var s = h.GetComponent<EnemyStats>();
                if (s) s.TakeDamage(damage * 0.5f);
            }
    }
}
