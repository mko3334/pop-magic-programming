using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PopMagicPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Magic")]
    public GameObject projectilePrefab;
    public float mpCostPerShot = 10f;

    private Rigidbody2D rb;
    private PlayerStats stats;
    private Camera mainCamera;

    // Z=0, X=1, C=2
    private string[] slotKeys = { "Z", "X", "C" };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        stats = GetComponent<PlayerStats>();
        mainCamera = Camera.main;
        var sm = SettingsManager.Instance;
        if (sm != null) { moveSpeed = sm.moveSpeed; mpCostPerShot = sm.mpCost; }
        CharacterStyleManager.Instance?.ApplyToPlayer();
        if (projectilePrefab == null) projectilePrefab = CreateDefaultProjectile();
    }

    GameObject CreateDefaultProjectile()
    {
        var go = new GameObject("DefaultProjectile");
        go.SetActive(false);
        var rb2 = go.AddComponent<Rigidbody2D>();
        rb2.gravityScale = 0f;
        rb2.freezeRotation = true;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(32);
        sr.color = new Color(1f, 0.8f, 0.2f);
        go.AddComponent<MagicProjectile>();
        DontDestroyOnLoad(go);
        return go;
    }

    Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size / 2f - 0.5f, r = size / 2f - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) <= r
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 0.3f);
    }

    void Update()
    {
        HandleAim();
        HandleSpellInput();
    }

    void FixedUpdate()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        float h = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1 : 0)
                - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1 : 0);
        float v = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1 : 0)
                - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1 : 0);
        rb.linearVelocity = new Vector2(h, v).normalized * moveSpeed;
    }

    void HandleAim()
    {
        if (Mouse.current == null) return;
        Vector2 ms = Mouse.current.position.ReadValue();
        Vector3 mw = mainCamera.ScreenToWorldPoint(new Vector3(ms.x, ms.y, 10f));
        mw.z = 0f;
        Vector2 dir = ((Vector2)mw - (Vector2)transform.position).normalized;
        if (dir == Vector2.zero) return;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
    }

    void HandleSpellInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Z/X/C キーで魔法発動
        if (kb.zKey.wasPressedThisFrame) CastSpell("Z");
        if (kb.xKey.wasPressedThisFrame) CastSpell("X");
        if (kb.cKey.wasPressedThisFrame) CastSpell("C");

        // 左クリックでもZスロット発動
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            CastSpell("Z");
    }

    void CastSpell(string slotKey)
    {
        if (projectilePrefab == null) return;
        if (!stats.UseMP(mpCostPerShot)) return;

        if (SpellManager.Instance == null) return;
        var spell = SpellManager.Instance.GetSpell(slotKey);
        if (spell == null || spell.blocks.Count == 0)
        {
            // ブロック未設定なら炎の直進弾をデフォルト発射
            FireProjectile(new List<BlockType> { BlockType.Fire, BlockType.Forward });
            return;
        }

        FireProjectile(spell.blocks);
    }

    void FireProjectile(List<BlockType> blocks)
    {
        if (Mouse.current == null) return;
        Vector2 ms = Mouse.current.position.ReadValue();
        Vector3 mw = mainCamera.ScreenToWorldPoint(new Vector3(ms.x, ms.y, 10f));
        mw.z = 0f;
        Vector2 dir = ((Vector2)mw - (Vector2)transform.position).normalized;

        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var mp = go.GetComponent<MagicProjectile>();
        if (mp != null) mp.Launch(dir, blocks);
    }
}
