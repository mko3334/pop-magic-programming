using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicProgrammerUI : MonoBehaviour
{
    public static MagicProgrammerUI Instance { get; private set; }

    const int MAX_BLOCKS = 5;

    Canvas canvas;
    GameObject panelRoot;
    string activeSlot = "Z";
    bool isOpen;

    readonly List<Image> chainSlotImages = new List<Image>();
    readonly List<GameObject> chainSlotGOs = new List<GameObject>();
    readonly Dictionary<string, Image> slotTabImages = new Dictionary<string, Image>();

    static readonly Color EMPTY_SLOT   = new Color(0.65f, 0.65f, 0.65f);
    static readonly Color ACTIVE_TAB   = new Color(1f, 0.71f, 0.71f);
    static readonly Color INACTIVE_TAB = new Color(0.82f, 0.79f, 0.74f);
    static readonly Color ARROW_COLOR  = new Color(0.4f, 0.65f, 1f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildUI();
    }

    public void Open()
    {
        if (panelRoot == null) BuildUI();
        isOpen = true;
        panelRoot.SetActive(true);
        RefreshAll();
    }

    public void Close()
    {
        isOpen = false;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    void RefreshAll()
    {
        RefreshTabs();
        RefreshChain();
    }

    // ─── UI構築 ───────────────────────────────────

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

        panelRoot = new GameObject("SpellbookRoot");
        panelRoot.transform.SetParent(transform, false);
        var rootRT = panelRoot.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;
        panelRoot.AddComponent<Image>().color = new Color(0.96f, 0.93f, 0.87f);

        BuildTitleBar();
        BuildMainContent();
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
        bar.AddComponent<Image>().color = new Color(1f, 0.71f, 0.71f);

        var tGO = new GameObject("Title");
        tGO.transform.SetParent(bar.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = "魔 導 書";
        t.font = font;
        t.fontSize = 38;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.18f, 0.06f, 0.06f);
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.sizeDelta = Vector2.zero;

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

    void BuildMainContent()
    {
        var content = new GameObject("MainContent");
        content.transform.SetParent(panelRoot.transform, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero;
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = new Vector2(0f, -72f);

        var hl = content.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.spacing = 0f;

        BuildPalettePanel(content.transform);
        BuildDivider(content.transform);
        BuildChainPanel(content.transform);
    }

    // ─── 左：パレット ───────────────────────────────

    void BuildPalettePanel(Transform parent)
    {
        var go = new GameObject("Palette");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1.2f;
        le.flexibleHeight = 1f;

        var scroll = go.AddComponent<ScrollRect>();

        var vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(go.transform, false);
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
        vl.spacing = 10f;
        vl.padding = new RectOffset(10, 10, 12, 12);
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRT;
        scroll.viewport = vpRT;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30f;

        BuildCategory(contentGO.transform, "ジェネレート", new Color(1f, 0.71f, 0.71f),    BlockCategory.Generate);
        BuildCategory(contentGO.transform, "ベクトル",     new Color(0.68f, 0.88f, 1f),    BlockCategory.Vector);
        BuildCategory(contentGO.transform, "トリガー",     new Color(1f, 0.92f, 0.58f),    BlockCategory.Trigger);
        BuildCategory(contentGO.transform, "アクション",   new Color(0.82f, 0.68f, 1f),    BlockCategory.Action);
        BuildCategory(contentGO.transform, "制御",         new Color(0.68f, 0.94f, 0.68f), BlockCategory.Control);
    }

    void BuildCategory(Transform parent, string label, Color color, BlockCategory category)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var headerGO = new GameObject("H_" + label);
        headerGO.transform.SetParent(parent, false);
        headerGO.AddComponent<Image>().color = color;
        headerGO.AddComponent<LayoutElement>().preferredHeight = 36f;

        var ht = new GameObject("T").AddComponent<Text>();
        ht.transform.SetParent(headerGO.transform, false);
        ht.text = label;
        ht.font = font;
        ht.fontSize = 20;
        ht.fontStyle = FontStyle.Bold;
        ht.color = new Color(0.15f, 0.08f, 0.08f);
        ht.alignment = TextAnchor.MiddleLeft;
        ht.raycastTarget = false;
        var htRT = ht.GetComponent<RectTransform>();
        htRT.anchorMin = Vector2.zero;
        htRT.anchorMax = Vector2.one;
        htRT.offsetMin = new Vector2(12f, 0f);
        htRT.offsetMax = Vector2.zero;

        var gridGO = new GameObject("G_" + label);
        gridGO.transform.SetParent(parent, false);
        var glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(76f, 76f);
        glg.spacing = new Vector2(8f, 8f);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.padding = new RectOffset(6, 6, 6, 6);
        gridGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        gridGO.AddComponent<LayoutElement>().flexibleWidth = 1f;

        foreach (var block in MagicBlockLibrary.All.FindAll(b => b.category == category))
        {
            var b = block;
            var icon = BuildBlockIcon(gridGO.transform, b, 76f);
            icon.GetComponent<Button>().onClick.AddListener(() => AddBlockToChain(b.type));
        }
    }

    GameObject BuildBlockIcon(Transform parent, MagicBlock block, float size)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var go = new GameObject(block.type.ToString());
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(size, size);

        var bg = go.AddComponent<Image>();
        bg.color = block.color;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        var c = btn.colors;
        c.highlightedColor = Color.white;
        c.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = c;

        if (block.icon != null)
        {
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(go.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.08f, 0.08f);
            iconRT.anchorMax = new Vector2(0.92f, 0.92f);
            iconRT.sizeDelta = Vector2.zero;
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = block.icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
        }
        else
        {
            var tGO = new GameObject("L");
            tGO.transform.SetParent(go.transform, false);
            var t = tGO.AddComponent<Text>();
            t.text = block.emoji + "\n" + block.label;
            t.font = font;
            t.fontSize = 15;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            var tRT = tGO.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.sizeDelta = Vector2.zero;
        }

        return go;
    }

    // ─── 区切り線 ────────────────────────────────

    void BuildDivider(Transform parent)
    {
        var go = new GameObject("Divider");
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = new Color(0.72f, 0.68f, 0.60f);
        go.AddComponent<LayoutElement>().preferredWidth = 2f;
    }

    // ─── 右：チェーンパネル ────────────────────────

    void BuildChainPanel(Transform parent)
    {
        var go = new GameObject("ChainPanel");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.flexibleHeight = 1f;
        go.AddComponent<Image>().color = new Color(0.92f, 0.89f, 0.83f);

        BuildSlotTabs(go.transform);
        BuildChainScroll(go.transform);
    }

    void BuildSlotTabs(Transform parent)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var tabsGO = new GameObject("Tabs");
        tabsGO.transform.SetParent(parent, false);
        var tabsRT = tabsGO.AddComponent<RectTransform>();
        tabsRT.anchorMin = new Vector2(0f, 1f);
        tabsRT.anchorMax = new Vector2(1f, 1f);
        tabsRT.pivot = new Vector2(0.5f, 1f);
        tabsRT.sizeDelta = new Vector2(0f, 60f);
        tabsRT.anchoredPosition = Vector2.zero;

        var hl = tabsGO.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandWidth = true;
        hl.childForceExpandHeight = true;
        hl.spacing = 6f;
        hl.padding = new RectOffset(10, 10, 8, 8);

        foreach (var key in new[] { "Z", "X", "C" })
        {
            string k = key;
            var tabGO = new GameObject("Tab_" + k);
            tabGO.transform.SetParent(tabsGO.transform, false);
            var tabImg = tabGO.AddComponent<Image>();
            tabImg.color = k == activeSlot ? ACTIVE_TAB : INACTIVE_TAB;
            slotTabImages[k] = tabImg;
            var tabBtn = tabGO.AddComponent<Button>();
            tabBtn.targetGraphic = tabImg;
            tabBtn.onClick.AddListener(() => SelectSlot(k));

            var tGO = new GameObject("T");
            tGO.transform.SetParent(tabGO.transform, false);
            var t = tGO.AddComponent<Text>();
            t.text = k;
            t.font = font;
            t.fontSize = 28;
            t.fontStyle = FontStyle.Bold;
            t.color = new Color(0.18f, 0.08f, 0.08f);
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            var tRT = tGO.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.sizeDelta = Vector2.zero;
        }
    }

    void BuildChainScroll(Transform parent)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var scrollGO = new GameObject("ChainScroll");
        scrollGO.transform.SetParent(parent, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = new Vector2(0f, -60f);

        var scroll = scrollGO.AddComponent<ScrollRect>();

        var vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(scrollGO.transform, false);
        var vpRT = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.sizeDelta = Vector2.zero;
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;

        var contentGO = new GameObject("ChainContent");
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 1f);
        contentRT.anchorMax = new Vector2(0.5f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(100f, 0f);

        var vl = contentGO.AddComponent<VerticalLayoutGroup>();
        vl.childForceExpandWidth = false;
        vl.childForceExpandHeight = false;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.spacing = 0f;
        vl.padding = new RectOffset(0, 0, 20, 20);
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRT;
        scroll.viewport = vpRT;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30f;

        chainSlotImages.Clear();
        chainSlotGOs.Clear();

        for (int i = 0; i < MAX_BLOCKS; i++)
        {
            int idx = i;

            var slotGO = new GameObject("Slot_" + i);
            slotGO.transform.SetParent(contentGO.transform, false);
            var slotLE = slotGO.AddComponent<LayoutElement>();
            slotLE.preferredWidth = 88f;
            slotLE.preferredHeight = 88f;
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = EMPTY_SLOT;
            var slotBtn = slotGO.AddComponent<Button>();
            slotBtn.targetGraphic = slotImg;
            slotBtn.onClick.AddListener(() => RemoveBlockFromChain(idx));

            chainSlotImages.Add(slotImg);
            chainSlotGOs.Add(slotGO);

            if (i < MAX_BLOCKS - 1)
            {
                var arrowGO = new GameObject("Arrow_" + i);
                arrowGO.transform.SetParent(contentGO.transform, false);
                var arrowLE = arrowGO.AddComponent<LayoutElement>();
                arrowLE.preferredWidth = 88f;
                arrowLE.preferredHeight = 28f;
                var arrowT = arrowGO.AddComponent<Text>();
                arrowT.text = "▼";
                arrowT.font = font;
                arrowT.fontSize = 22;
                arrowT.color = ARROW_COLOR;
                arrowT.alignment = TextAnchor.MiddleCenter;
                arrowT.raycastTarget = false;
            }
        }
    }

    // ─── インタラクション ─────────────────────────

    void SelectSlot(string key)
    {
        activeSlot = key;
        RefreshTabs();
        RefreshChain();
    }

    void RefreshTabs()
    {
        foreach (var kv in slotTabImages)
            kv.Value.color = kv.Key == activeSlot ? ACTIVE_TAB : INACTIVE_TAB;
    }

    void AddBlockToChain(BlockType type)
    {
        var spell = SpellManager.Instance?.GetSpell(activeSlot);
        if (spell == null || spell.blocks.Count >= MAX_BLOCKS) return;
        spell.blocks.Add(type);
        SpellManager.Instance.Save();
        RefreshChain();
    }

    void RemoveBlockFromChain(int index)
    {
        var spell = SpellManager.Instance?.GetSpell(activeSlot);
        if (spell == null || index >= spell.blocks.Count) return;
        spell.blocks.RemoveAt(index);
        SpellManager.Instance.Save();
        RefreshChain();
    }

    void RefreshChain()
    {
        var spell = SpellManager.Instance?.GetSpell(activeSlot);
        var blocks = spell?.blocks ?? new List<BlockType>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        for (int i = 0; i < MAX_BLOCKS && i < chainSlotImages.Count; i++)
        {
            foreach (Transform child in chainSlotGOs[i].transform)
                Destroy(child.gameObject);

            if (i < blocks.Count)
            {
                var block = MagicBlockLibrary.Get(blocks[i]);
                if (block == null) continue;
                chainSlotImages[i].color = block.color;

                if (block.icon != null)
                {
                    var iconGO = new GameObject("Icon");
                    iconGO.transform.SetParent(chainSlotGOs[i].transform, false);
                    var iconRT = iconGO.AddComponent<RectTransform>();
                    iconRT.anchorMin = new Vector2(0.08f, 0.08f);
                    iconRT.anchorMax = new Vector2(0.92f, 0.92f);
                    iconRT.sizeDelta = Vector2.zero;
                    var iconImg = iconGO.AddComponent<Image>();
                    iconImg.sprite = block.icon;
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;
                }
                else
                {
                    var tGO = new GameObject("L");
                    tGO.transform.SetParent(chainSlotGOs[i].transform, false);
                    var t = tGO.AddComponent<Text>();
                    t.text = block.emoji + "\n" + block.label;
                    t.font = font;
                    t.fontSize = 14;
                    t.color = Color.white;
                    t.alignment = TextAnchor.MiddleCenter;
                    t.raycastTarget = false;
                    var tRT = tGO.AddComponent<RectTransform>();
                    tRT.anchorMin = Vector2.zero;
                    tRT.anchorMax = Vector2.one;
                    tRT.sizeDelta = Vector2.zero;
                }
            }
            else
            {
                chainSlotImages[i].color = EMPTY_SLOT;
            }
        }
    }
}
