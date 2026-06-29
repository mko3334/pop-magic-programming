using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterStyleManager : MonoBehaviour
{
    public static CharacterStyleManager Instance { get; private set; }

    [Header("利用可能スプライト（Inspectorで追加）")]
    public Sprite[] availableSprites;

    // 保存データ
    List<string> playerFrames = new List<string>();
    float playerFps = 8f;
    List<string> enemyFrames = new List<string>();
    float enemyFps = 8f;
    string magicSpriteName = "";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("[CharacterStyleManager]");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<CharacterStyleManager>();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ─── 公開アクセサ ──────────────────────────────

    public List<string> GetPlayerFrameNames() => new List<string>(playerFrames);
    public float GetPlayerFps() => playerFps;
    public List<string> GetEnemyFrameNames() => new List<string>(enemyFrames);
    public float GetEnemyFps() => enemyFps;
    public string GetMagicSpriteName() => magicSpriteName;

    public List<Sprite> GetAllSprites()
    {
        var result = new List<Sprite>();
        if (availableSprites != null)
            result.AddRange(availableSprites.Where(s => s != null));
        foreach (var block in MagicBlockLibrary.All)
            if (block.icon != null && !result.Contains(block.icon))
                result.Add(block.icon);
        return result;
    }

    public Sprite FindSprite(string name)
    {
        foreach (var s in GetAllSprites())
            if (s.name == name) return s;
        return null;
    }

    // ─── 設定・保存 ────────────────────────────────

    public void SetPlayerAnim(List<string> names, float fps)
    {
        playerFrames = new List<string>(names);
        playerFps = fps;
        Save();
        ApplyToPlayer();
    }

    public void SetEnemyAnim(List<string> names, float fps)
    {
        enemyFrames = new List<string>(names);
        enemyFps = fps;
        Save();
        ApplyToEnemies();
    }

    public void SetMagicSprite(string name)
    {
        magicSpriteName = name;
        Save();
    }

    public void ApplyAll()
    {
        ApplyToPlayer();
        ApplyToEnemies();
    }

    // ─── 適用 ──────────────────────────────────────

    public void ApplyToPlayer()
    {
        var player = FindObjectOfType<PopMagicPlayer>();
        if (player == null) return;
        var anim = player.GetComponent<SpriteAnimator>();
        if (anim == null) return;
        ApplyAnim(anim, playerFrames, playerFps);
    }

    public void ApplyToEnemies()
    {
        foreach (var e in FindObjectsOfType<EnemyAI>())
        {
            var anim = e.GetComponent<SpriteAnimator>();
            if (anim == null) continue;
            ApplyAnim(anim, enemyFrames, enemyFps);
        }
    }

    void ApplyAnim(SpriteAnimator anim, List<string> names, float fps)
    {
        var sprites = NamesToSprites(names);
        if (sprites.Count == 0) return;
        if (sprites.Count == 1) anim.SetStatic(sprites[0]);
        else anim.Play(sprites, fps);
    }

    List<Sprite> NamesToSprites(List<string> names)
    {
        var result = new List<Sprite>();
        foreach (var n in names)
        {
            var s = FindSprite(n);
            if (s != null) result.Add(s);
        }
        return result;
    }

    // ─── 保存・読み込み ────────────────────────────

    void Save()
    {
        PlayerPrefs.SetString("cstyle_player",     string.Join(",", playerFrames));
        PlayerPrefs.SetFloat("cstyle_player_fps",  playerFps);
        PlayerPrefs.SetString("cstyle_enemy",      string.Join(",", enemyFrames));
        PlayerPrefs.SetFloat("cstyle_enemy_fps",   enemyFps);
        PlayerPrefs.SetString("cstyle_magic",      magicSpriteName);
        PlayerPrefs.Save();
    }

    void Load()
    {
        var pf = PlayerPrefs.GetString("cstyle_player", "");
        playerFrames = pf.Length > 0 ? new List<string>(pf.Split(',')) : new List<string>();
        playerFps    = PlayerPrefs.GetFloat("cstyle_player_fps", 8f);
        var ef = PlayerPrefs.GetString("cstyle_enemy", "");
        enemyFrames  = ef.Length > 0 ? new List<string>(ef.Split(',')) : new List<string>();
        enemyFps     = PlayerPrefs.GetFloat("cstyle_enemy_fps", 8f);
        magicSpriteName = PlayerPrefs.GetString("cstyle_magic", "");
    }
}
