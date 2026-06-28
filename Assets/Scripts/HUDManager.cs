using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("Icons")]
    public Sprite heartSprite;
    public Sprite moonSprite;
    public Sprite potionSprite;

    [Header("Settings")]
    public int maxHearts = 5;
    public int maxMoons = 6;

    private List<Image> heartIcons = new List<Image>();
    private List<Image> moonIcons = new List<Image>();
    private Text potionText;
    private Text expText;

    Canvas canvas;

    void Start()
    {
        var sm = SettingsManager.Instance;
        if (sm != null) { maxHearts = sm.maxHearts; maxMoons = sm.maxMoons; }
        canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        BuildHUD();
    }

    void BuildHUD()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // HUDルート（左上）
        var root = new GameObject("HUDRoot");
        root.AddComponent<RectTransform>();
        root.transform.SetParent(transform, false);
        var rr = root.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(0,1); rr.anchorMax = new Vector2(0,1);
        rr.pivot = new Vector2(0,1);
        rr.anchoredPosition = new Vector2(16, -16);
        rr.sizeDelta = new Vector2(320, 180);

        // 破れた紙風背景
        var bg = new GameObject("BG"); bg.AddComponent<RectTransform>(); bg.transform.SetParent(root.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.97f, 0.94f, 0.86f, 0.88f);
        var bgR = bg.GetComponent<RectTransform>();
        bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one; bgR.sizeDelta = new Vector2(16,16);

        var vl = root.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(12,12,10,10); vl.spacing = 4;
        vl.childControlWidth = false; vl.childControlHeight = false;
        vl.childForceExpandWidth = false;

        // ポーション+EXP行
        var row0 = MakeRow(root.transform);
        if (potionSprite != null) MakeIcon(row0, potionSprite, 32);
        potionText = MakeText(row0, "MP 100", font, 16, new Color(0.3f,0.1f,0.5f));
        var expBg = new GameObject("ExpBg"); expBg.AddComponent<RectTransform>(); expBg.transform.SetParent(row0, false);
        var expBgImg = expBg.AddComponent<Image>(); expBgImg.color = new Color(0.95f,0.8f,0.1f,0.9f);
        var expLE = expBg.AddComponent<LayoutElement>(); expLE.preferredWidth=90; expLE.preferredHeight=22;
        expText = new GameObject("ET").AddComponent<Text>();
        expText.transform.SetParent(expBg.transform, false);
        expText.text = "EXP 0%"; expText.font=font; expText.fontSize=13; expText.color=new Color(0.3f,0.15f,0f);
        expText.alignment=TextAnchor.MiddleCenter;
        var etr=expText.GetComponent<RectTransform>(); etr.anchorMin=Vector2.zero; etr.anchorMax=Vector2.one; etr.sizeDelta=Vector2.zero;

        // ハート行
        var row1 = MakeRow(root.transform);
        for (int i = 0; i < maxHearts; i++)
        {
            var img = MakeIcon(row1, heartSprite, 36);
            heartIcons.Add(img);
        }

        // 月行
        var row2 = MakeRow(root.transform);
        for (int i = 0; i < maxMoons; i++)
        {
            var img = MakeIcon(row2, moonSprite, 30);
            moonIcons.Add(img);
        }
    }

    Transform MakeRow(Transform parent)
    {
        var go = new GameObject("Row"); go.AddComponent<RectTransform>(); go.transform.SetParent(parent, false);
        var hl = go.AddComponent<HorizontalLayoutGroup>(); hl.spacing=4; hl.childControlHeight=false; hl.childForceExpandWidth=false;
        go.AddComponent<LayoutElement>().preferredHeight = 40;
        return go.transform;
    }

    Image MakeIcon(Transform parent, Sprite sprite, int size)
    {
        var go = new GameObject("Icon"); go.AddComponent<RectTransform>(); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.color = Color.white; }
        var le = go.AddComponent<LayoutElement>(); le.preferredWidth=size; le.preferredHeight=size;
        return img;
    }

    Text MakeText(Transform parent, string text, Font font, int size, Color color)
    {
        var go = new GameObject("Txt"); go.AddComponent<RectTransform>(); go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>(); t.text=text; t.font=font; t.fontSize=size; t.color=color;
        t.alignment=TextAnchor.MiddleLeft;
        go.AddComponent<LayoutElement>().preferredHeight=size+4;
        return t;
    }

    void Update()
    {
        if (playerStats == null) return;

        // ハートの表示（HP割合でON/OFF）
        float hpRatio = playerStats.currentHP / playerStats.maxHP;
        int activeHearts = Mathf.RoundToInt(hpRatio * maxHearts);
        for (int i = 0; i < heartIcons.Count; i++)
            heartIcons[i].color = i < activeHearts ? Color.white : new Color(1,1,1,0.25f);

        // 月の表示（MP割合）
        float mpRatio = playerStats.currentMP / playerStats.maxMP;
        int activeMoons = Mathf.RoundToInt(mpRatio * maxMoons);
        for (int i = 0; i < moonIcons.Count; i++)
            moonIcons[i].color = i < activeMoons ? new Color(0.7f,0.85f,1f) : new Color(1,1,1,0.2f);

        // テキスト
        if (potionText) potionText.text = Mathf.CeilToInt(playerStats.currentMP).ToString();
        if (expText)
        {
            int pct = playerStats.expToNextLevel > 0
                ? Mathf.RoundToInt((float)playerStats.currentEXP / playerStats.expToNextLevel * 100f)
                : 0;
            expText.text = "EXP " + pct + "%";
        }
    }
}
