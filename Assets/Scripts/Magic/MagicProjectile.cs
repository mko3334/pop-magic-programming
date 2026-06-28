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

    // ベクトル系
    private bool isHoming;
    private bool isZigzag;
    private bool isBounce;
    private bool isRotate;
    private bool isReturn;
    private int bounceCount = 2;
    private float zigzagTimer;
    private float rotateSpeed = 150f;
    private float returnDelay = 1.5f;
    private bool hasReturned;
    private Transform homingTarget;

    // トリガー系
    private bool isWait;
    private float waitRemaining = 1.5f;
    private bool isOnHit;
    private bool isStuck;

    // アクション系
    private bool isGrow;
    private float spawnTime;

    // 属性
    private BlockType element = BlockType.Fire;
    private bool isPiercing;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        spawnTime = Time.time;
    }

    public void Launch(Vector2 direction, List<BlockType> blocks)
    {
        moveDir = direction.normalized;
        blockSequence = new List<BlockType>(blocks);
        ApplyBlocks();
        rb.linearVelocity = isWait ? Vector2.zero : moveDir * speed;
        Destroy(gameObject, lifetime);

        var csm = CharacterStyleManager.Instance;
        var magicSpriteName = csm?.GetMagicSpriteName();
        var magicSprite = !string.IsNullOrEmpty(magicSpriteName) ? csm.FindSprite(magicSpriteName) : null;
        if (magicSprite != null) ApplyColor(Color.white);
        var sr2 = GetComponent<SpriteRenderer>();
        if (sr2 != null && magicSprite != null) sr2.sprite = magicSprite;
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
                case BlockType.Forward: break;
                case BlockType.Homing:  isHoming = true; FindHomingTarget(); break;
                case BlockType.Zigzag:  isZigzag = true; break;
                case BlockType.Bounce:  isBounce = true; break;
                case BlockType.Rotate:  isRotate = true; break;
                case BlockType.Return:  isReturn = true; break;

                // トリガー
                case BlockType.Wait:    isWait = true; break;
                case BlockType.OnHit:   isOnHit = true; break;
                case BlockType.WaitTap: break;

                // アクション（即時適用）
                case BlockType.Speed:   speed *= 1.8f; break;
                case BlockType.Grow:    isGrow = true; break;
                case BlockType.Fortify: damage *= 1.5f; bounceCount += 3; break;

                // アクション（HitEnemy時）
                case BlockType.Explode:
                case BlockType.Split:
                case BlockType.Attract:
                case BlockType.Slow:
                case BlockType.Stop:
                    break;
            }
        }

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

    Color ElementColor() => element switch
    {
        BlockType.Fire      => new Color(1f, 0.4f, 0.1f),
        BlockType.Lightning => new Color(1f, 0.95f, 0.1f),
        BlockType.Water     => new Color(0.2f, 0.6f, 1f),
        BlockType.Wood      => new Color(0.2f, 0.8f, 0.2f),
        BlockType.Earth     => new Color(0.8f, 0.6f, 0.3f),
        BlockType.Light     => new Color(1f, 0.98f, 0.65f),
        _                   => Color.white,
    };

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
        // Wait: 発射前に一時停止してから動き出す
        if (isWait && waitRemaining > 0f)
        {
            waitRemaining -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            if (waitRemaining <= 0f)
                rb.linearVelocity = moveDir * speed;
            return;
        }

        // Grow: 時間とともにスケール拡大
        if (isGrow)
        {
            float t = Mathf.Clamp01((Time.time - spawnTime) / lifetime);
            transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 2.5f, t);
        }

        // Return: 一定時間後に引き返す（プレイヤーへホーミング）
        if (isReturn && !hasReturned)
        {
            returnDelay -= Time.deltaTime;
            if (returnDelay <= 0f)
            {
                hasReturned = true;
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) { isHoming = true; homingTarget = p.transform; }
            }
        }

        // Rotate: 移動方向を旋回（Homing/Zigzag中は無効）
        if (isRotate && !isHoming && !isZigzag)
        {
            moveDir = (Quaternion.Euler(0f, 0f, rotateSpeed * Time.deltaTime) * (Vector3)moveDir).normalized;
            rb.linearVelocity = moveDir * speed;
        }

        // Homing
        if (isHoming && homingTarget != null)
        {
            Vector2 toTarget = ((Vector2)homingTarget.position - (Vector2)transform.position).normalized;
            moveDir = Vector2.Lerp(moveDir, toTarget, 4f * Time.deltaTime).normalized;
            rb.linearVelocity = moveDir * speed;
        }

        // Zigzag
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

            // OnHit: 敵に貼り付いて1秒後に発動（粘着爆弾）
            if (isOnHit && !isStuck)
            {
                isStuck = true;
                hitEnemies.Add(other.gameObject);
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
                transform.SetParent(other.transform);
                StartCoroutine(DelayedHit(other.gameObject, 1.0f));
                return;
            }

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
                var slow = enemy.GetComponent<SlowEffect>() ?? enemy.AddComponent<SlowEffect>();
                slow.StartSlow(2f);
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
                case BlockType.Stop:
                    var freeze = enemy.GetComponent<StopEffect>() ?? enemy.AddComponent<StopEffect>();
                    freeze.StartFreeze(2.5f);
                    break;
            }
        }

        var stats = enemy.GetComponent<EnemyStats>();
        if (stats) stats.TakeDamage(damage, ElementColor());
    }

    void Explode()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var h in hits)
            if (h.CompareTag("Enemy"))
            {
                var s = h.GetComponent<EnemyStats>();
                if (s) s.TakeDamage(damage * 0.6f, ElementColor());
            }
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
                mp.blockSequence = new List<BlockType>();
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

    IEnumerator DelayedHit(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.SetParent(null);
        if (enemy != null && enemy.activeInHierarchy)
            HitEnemy(enemy);
        Destroy(gameObject);
    }

    IEnumerator LightningSpawnEffect()
    {
        yield return new WaitForSeconds(0.05f);
        var hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var h in hits)
            if (h.CompareTag("Enemy"))
            {
                var s = h.GetComponent<EnemyStats>();
                if (s) s.TakeDamage(damage * 0.5f, ElementColor());
            }
    }
}
