using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    Canvas canvas;
    GameObject panelRoot;
    bool isOpen;

    static readonly Color HEADER_PLAYER  = new Color(0.68f, 0.88f, 1f);
    static readonly Color HEADER_MAGIC   = new Color(0.82f, 0.68f, 1f);
    static readonly Color HEADER_HUD     = new Color(0.68f, 0.94f, 0.68f);
    static readonly Color HEADER_TITLE   = new Color(1f, 0.84f, 0.68f);
    static readonly Color BTN_MINUS      = new Color(0.95f, 0.52f, 0.52f);
    static readonly Color BTN_PLUS       = new Color(0.52f, 0.82f, 0.52f);
    static readonly Color ROW_ODD        = new Color(0.93f, 0.90f, 0.84f);
    static readonly Color ROW_EVEN       = new Color(0.97f, 0.95f, 0.90f);

    void Awake()
    {
        Instance = this;
        BuildUI();
    }

    public void Open()
    {
        isOpen = true;
        panelRoot.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // ─── UI構築 ────────────────────────────────────

    void BuildUI()
    {
        canvas = GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        if (GetComponent<CanvasScaler>() == null)
        {
            var cs = gameObject.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.matchWidthOrHeight = 0.5f;
        }
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("SettingsRoot");
        panelRoot.transform.SetParent(transform, false);
        var rootRT = panelRoot.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;
        panelRoot.AddComponent<Image>().color = new Color(0.96f, 0.93f, 0.87f);

        BuildTitleBar();
        BuildScrollContent();
        panelRoot.SetActive(false);
    }

    void BuildTitleBar()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var bar = new GameObject("TitleBar");
        bar.transform.SetParent(panelRoot.transform, false);
        var barRT = bar.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 1f);
        barRT.anchorMax = new Vector2(1f, 1f);
        barRT.pivot = new Vector2(0.5f, 1f);
        barRT.sizeDelta = new Vector2(0f, 72f);
        barRT.anchoredPosition = Vector2.zero;
        bar.AddComponent<Image>().color = HEADER_TITLE;

        var tGO = new GameObject("Title");
        tGO.transform.SetParent(bar.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = "設 定";
        t.font = font;
        t.fontSize = 38;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.18f, 0.08f, 0.04f);
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.sizeDelta = Vector2.zero;

        // 閉じるボタン
        var closeGO = new GameObject("CloseBtn");
        closeGO.transform.SetParent(bar.transform, false);
        var closeRT = closeGO.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 0f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot = new Vector2(1f, 0.5f);
        closeRT.sizeDelta = new Vector2(72f, 0f);
        closeRT.anchoredPosition = Vector2.zero;
        var closeImg = closeGO.AddComponent<Image>();
        closeImg.color = new Color(0.75f, 0.25f, 0.25f);
        var closeBtn = closeGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        closeBtn.onClick.AddListener(() => { Close(); GameMenuDrawer.Instance?.Close(); });

        var xGO = new GameObject("X");
        xGO.transform.SetParent(closeGO.transform, false);
        var x = xGO.AddComponent<Text>();
        x.text = "✕";
        x.font = font;
        x.fontSize = 30;
        x.color = Color.white;
        x.alignment = TextAnchor.MiddleCenter;
        x.raycastTarget = false;
        var xRT = xGO.AddComponent<RectTransform>();
        xRT.anchorMin = Vector2.zero;
        xRT.anchorMax = Vector2.one;
        xRT.sizeDelta = Vector2.zero;
    }

    void BuildScrollContent()
    {
        // タイトルバー(72px)の下
        var scrollGO = new GameObject("Scroll");
        scrollGO.transform.SetParent(panelRoot.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = new Vector2(0f, -72f);

        var scroll = scrollGO.AddComponent<ScrollRect>();

        var vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(scrollGO.transform, false);
        var vpRT = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.sizeDelta = Vector2.zero;
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = Vector2.zero;

        var vl = contentGO.AddComponent<VerticalLayoutGroup>();
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.spacing = 0f;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRT;
        scroll.viewport = vpRT;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 40f;

        var sm = SettingsManager.Instance;

        // ── プレイヤー ──
        BuildSectionHeader(contentGO.transform, "プレイヤー", HEADER_PLAYER);
        int rowIdx = 0;
        MakeRow(contentGO.transform, "体力 (HP)",   10f, 500f, 10f, true,  rowIdx++, () => sm.maxHP,     v => { sm.maxHP = v;     ApplyToPlayer(); });
        MakeRow(contentGO.transform, "MP",           10f, 500f, 10f, true,  rowIdx++, () => sm.maxMP,     v => { sm.maxMP = v;     ApplyToPlayer(); });
        MakeRow(contentGO.transform, "移動速度",      1f,  20f,  0.5f,false, rowIdx++, () => sm.moveSpeed, v => { sm.moveSpeed = v; ApplyToPlayer(); });
        MakeRow(contentGO.transform, "MP回復速度",    0f,  50f,  1f,  false, rowIdx++, () => sm.mpRegen,   v => { sm.mpRegen = v;   ApplyToPlayer(); });
        MakeRow(contentGO.transform, "魔法MPコスト",  0f, 100f,  5f,  true,  rowIdx++, () => sm.mpCost,    v => { sm.mpCost = v;    ApplyToPlayer(); });

        // ── 魔法ダメージ ──
        BuildSectionHeader(contentGO.transform, "魔法ダメージ", HEADER_MAGIC);
        MakeRow(contentGO.transform, "🔥 炎",   1f, 200f, 1f, true, rowIdx++, () => sm.fireDmg,      v => sm.fireDmg = v);
        MakeRow(contentGO.transform, "⚡ 雷",   1f, 200f, 1f, true, rowIdx++, () => sm.lightningDmg, v => sm.lightningDmg = v);
        MakeRow(contentGO.transform, "💧 水",   1f, 200f, 1f, true, rowIdx++, () => sm.waterDmg,     v => sm.waterDmg = v);
        MakeRow(contentGO.transform, "🌿 木",   1f, 200f, 1f, true, rowIdx++, () => sm.woodDmg,      v => sm.woodDmg = v);
        MakeRow(contentGO.transform, "🪨 土",   1f, 200f, 1f, true, rowIdx++, () => sm.earthDmg,     v => sm.earthDmg = v);
        MakeRow(contentGO.transform, "✨ 光",   1f, 200f, 1f, true, rowIdx++, () => sm.lightDmg,     v => sm.lightDmg = v);

        // ── HUD ──
        BuildSectionHeader(contentGO.transform, "HUD表示", HEADER_HUD);
        MakeRow(contentGO.transform, "ハート数", 1f, 10f, 1f, true, rowIdx++, () => sm.maxHearts, v => { sm.maxHearts = (int)v; ApplyToHUD(); });
        MakeRow(contentGO.transform, "月数",     1f, 10f, 1f, true, rowIdx++, () => sm.maxMoons,  v => { sm.maxMoons  = (int)v; ApplyToHUD(); });

        // リセットボタン
        BuildResetButton(contentGO.transform);
    }

    void BuildSectionHeader(Transform parent, string label, Color color)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject("SH_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        go.AddComponent<LayoutElement>().preferredHeight = 48f;

        var tGO = new GameObject("T");
        tGO.transform.SetParent(go.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 24;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.15f, 0.08f, 0.06f);
        t.alignment = TextAnchor.MiddleLeft;
        t.raycastTarget = false;
        var tr = tGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(20f, 0f);
        tr.offsetMax = Vector2.zero;
    }

    void MakeRow(Transform parent, string label, float min, float max, float step, bool isInt,
                 int index, Func<float> getter, Action<float> setter)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var row = new GameObject("Row_" + label);
        row.transform.SetParent(parent, false);
        row.AddComponent<Image>().color = index % 2 == 0 ? ROW_EVEN : ROW_ODD;
        row.AddComponent<LayoutElement>().preferredHeight = 72f;

        var hl = row.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandHeight = true;
        hl.childForceExpandWidth = false;
        hl.spacing = 8f;
        hl.padding = new RectOffset(20, 20, 10, 10);

        // ラベル
        var labelGO = new GameObject("L");
        labelGO.transform.SetParent(row.transform, false);
        labelGO.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var lt = labelGO.AddComponent<Text>();
        lt.text = label;
        lt.font = font;
        lt.fontSize = 24;
        lt.color = new Color(0.18f, 0.08f, 0.06f);
        lt.alignment = TextAnchor.MiddleLeft;
        lt.raycastTarget = false;

        // 「−」ボタン
        var minusBtn = MakeStepBtn(row.transform, "−", BTN_MINUS);

        // 値テキスト
        var valGO = new GameObject("V");
        valGO.transform.SetParent(row.transform, false);
        valGO.AddComponent<LayoutElement>().preferredWidth = 100f;
        var vt = valGO.AddComponent<Text>();
        vt.font = font;
        vt.fontSize = 26;
        vt.fontStyle = FontStyle.Bold;
        vt.color = new Color(0.15f, 0.08f, 0.06f);
        vt.alignment = TextAnchor.MiddleCenter;
        vt.raycastTarget = false;
        UpdateValueText(vt, getter(), isInt);

        // 「＋」ボタン
        var plusBtn = MakeStepBtn(row.transform, "＋", BTN_PLUS);

        // ボタン処理
        minusBtn.onClick.AddListener(() =>
        {
            float newV = Mathf.Max(min, getter() - step);
            setter(newV);
            UpdateValueText(vt, newV, isInt);
            SettingsManager.Instance?.Save();
        });
        plusBtn.onClick.AddListener(() =>
        {
            float newV = Mathf.Min(max, getter() + step);
            setter(newV);
            UpdateValueText(vt, newV, isInt);
            SettingsManager.Instance?.Save();
        });
    }

    Button MakeStepBtn(Transform parent, string label, Color color)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().preferredWidth = 64f;
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var c = btn.colors;
        c.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f);
        btn.colors = c;

        var tGO = new GameObject("T");
        tGO.transform.SetParent(go.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 32;
        t.fontStyle = FontStyle.Bold;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var tr = tGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;

        return btn;
    }

    void UpdateValueText(Text t, float v, bool isInt)
    {
        t.text = isInt ? ((int)v).ToString() : v.ToString("F1");
    }

    void BuildResetButton(Transform parent)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        spacer.AddComponent<LayoutElement>().preferredHeight = 20f;

        var go = new GameObject("ResetBtn");
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().preferredHeight = 72f;

        var hl = go.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandHeight = true;
        hl.childForceExpandWidth = false;
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.padding = new RectOffset(40, 40, 10, 10);

        var btnGO = new GameObject("Btn");
        btnGO.transform.SetParent(go.transform, false);
        var le = btnGO.AddComponent<LayoutElement>();
        le.preferredWidth = 320f;
        le.preferredHeight = 56f;
        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.65f, 0.65f, 0.65f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            SettingsManager.Instance?.ResetDefaults();
            ApplyToPlayer();
            ApplyToHUD();
            // パネルを閉じて再開して値をリフレッシュ
            Close();
            Open();
        });

        var tGO = new GameObject("T");
        tGO.transform.SetParent(btnGO.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = "デフォルトにリセット";
        t.font = font;
        t.fontSize = 22;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var tr = tGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;

        var bottom = new GameObject("BottomPad");
        bottom.transform.SetParent(parent, false);
        bottom.AddComponent<LayoutElement>().preferredHeight = 40f;
    }

    // ─── 変更をゲームオブジェクトへ即時反映 ──────────

    void ApplyToPlayer()
    {
        var sm = SettingsManager.Instance;
        if (sm == null) return;
        var ps = FindObjectOfType<PlayerStats>();
        if (ps != null)
        {
            ps.maxHP = sm.maxHP;
            ps.maxMP = sm.maxMP;
            ps.mpRegenRate = sm.mpRegen;
            if (ps.currentHP > ps.maxHP) ps.currentHP = ps.maxHP;
            if (ps.currentMP > ps.maxMP) ps.currentMP = ps.maxMP;
        }
        var player = FindObjectOfType<PopMagicPlayer>();
        if (player != null)
        {
            player.moveSpeed = sm.moveSpeed;
            player.mpCostPerShot = sm.mpCost;
        }
    }

    void ApplyToHUD()
    {
        var sm = SettingsManager.Instance;
        if (sm == null) return;
        var hud = FindObjectOfType<HUDManager>();
        if (hud != null)
        {
            hud.maxHearts = sm.maxHearts;
            hud.maxMoons  = sm.maxMoons;
        }
    }
}
