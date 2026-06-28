using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        var go = new GameObject("[SettingsManager]");
        DontDestroyOnLoad(go);
        go.AddComponent<SettingsManager>();
    }

    // プレイヤー
    public float maxHP     = 100f;
    public float maxMP     = 100f;
    public float moveSpeed = 5f;
    public float mpRegen   = 5f;
    public float mpCost    = 10f;

    // 魔法ダメージ
    public float fireDmg      = 12f;
    public float lightningDmg = 20f;
    public float waterDmg     = 8f;
    public float woodDmg      = 10f;
    public float earthDmg     = 25f;
    public float lightDmg     = 10f;

    // HUD
    public int maxHearts = 5;
    public int maxMoons  = 6;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("cfg_maxHP",        maxHP);
        PlayerPrefs.SetFloat("cfg_maxMP",        maxMP);
        PlayerPrefs.SetFloat("cfg_moveSpeed",    moveSpeed);
        PlayerPrefs.SetFloat("cfg_mpRegen",      mpRegen);
        PlayerPrefs.SetFloat("cfg_mpCost",       mpCost);
        PlayerPrefs.SetFloat("cfg_fireDmg",      fireDmg);
        PlayerPrefs.SetFloat("cfg_lightningDmg", lightningDmg);
        PlayerPrefs.SetFloat("cfg_waterDmg",     waterDmg);
        PlayerPrefs.SetFloat("cfg_woodDmg",      woodDmg);
        PlayerPrefs.SetFloat("cfg_earthDmg",     earthDmg);
        PlayerPrefs.SetFloat("cfg_lightDmg",     lightDmg);
        PlayerPrefs.SetInt("cfg_maxHearts",      maxHearts);
        PlayerPrefs.SetInt("cfg_maxMoons",       maxMoons);
        PlayerPrefs.Save();
    }

    void Load()
    {
        maxHP        = PlayerPrefs.GetFloat("cfg_maxHP",        100f);
        maxMP        = PlayerPrefs.GetFloat("cfg_maxMP",        100f);
        moveSpeed    = PlayerPrefs.GetFloat("cfg_moveSpeed",    5f);
        mpRegen      = PlayerPrefs.GetFloat("cfg_mpRegen",      5f);
        mpCost       = PlayerPrefs.GetFloat("cfg_mpCost",       10f);
        fireDmg      = PlayerPrefs.GetFloat("cfg_fireDmg",      12f);
        lightningDmg = PlayerPrefs.GetFloat("cfg_lightningDmg", 20f);
        waterDmg     = PlayerPrefs.GetFloat("cfg_waterDmg",     8f);
        woodDmg      = PlayerPrefs.GetFloat("cfg_woodDmg",      10f);
        earthDmg     = PlayerPrefs.GetFloat("cfg_earthDmg",     25f);
        lightDmg     = PlayerPrefs.GetFloat("cfg_lightDmg",     10f);
        maxHearts    = PlayerPrefs.GetInt("cfg_maxHearts",      5);
        maxMoons     = PlayerPrefs.GetInt("cfg_maxMoons",       6);
    }

    public void ResetDefaults()
    {
        maxHP = 100f; maxMP = 100f; moveSpeed = 5f; mpRegen = 5f; mpCost = 10f;
        fireDmg = 12f; lightningDmg = 20f; waterDmg = 8f;
        woodDmg = 10f; earthDmg = 25f; lightDmg = 10f;
        maxHearts = 5; maxMoons = 6;
        Save();
    }
}
