using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEditorUI : MonoBehaviour
{
    public static CharacterEditorUI Instance { get; private set; }

    enum Target { Player, Enemy, Magic }

    Canvas canvas;
    GameObject panelRoot;
    Target activeTarget = Target.Player;
    bool isOpen;

    // 編集中データ
    List<string> editFrameNames = new List<string>();
    float editFps = 8f;

    // UI参照
    Transform frameListContent;
    Text fpsText;
    Image previewImage;
    Coroutine previewCoroutine;
    readonly Dictionary<Target, Image> targetTabImgs = new Dictionary<Target, Image>();

    static readonly Color TAB_ACTIVE   = new Color(1f, 0.84f, 0.68f);
    static readonly Color TAB_INACTIVE = new Color(0.82f, 0.79f, 0.74f);
    static readonly Color TITLE_COLOR  = new Color(1f, 0.84f, 0.68f);
    static readonly Color FRAME_BG     = new Color(0.75f, 0.72f, 0.68f);

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
        Canvas.ForceUpdateCanvases();
        LoadFromTarget();
        RefreshFrameList();
    }

    public void Close()
    {
        isOpen = false;
        StopPreview();
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // ─── ターゲット切替 ────────────────────────────

    void SelectTarget(Target t)
    {
        StopPreview();
        activeTarget = t;
        foreach (var kv in targetTabImgs)
            kv.Value.color = kv.Key == t ? TAB_ACTIVE : TAB_INACTIVE;
        LoadFromTarget();
        RefreshFrameList();
    }

    void LoadFromTarget()
    {
        var csm = CharacterStyleManager.Instance;
        if (csm == null) { editFrameNames.Clear(); editFps = 8f; return; }
        switch (activeTarget)
        {
            case Target.Player:
                editFrameNames = csm.GetPlayerFrameNames();
                editFps = csm.GetPlayerFps();
                break;
            case Target.Enemy:
                editFrameNames = csm.GetEnemyFrameNames();
                editFps = csm.GetEnemyFps();
                break;
            case Target.Magic:
                var mn = csm.GetMagicSpriteName();
                editFrameNames = mn.Length > 0 ? new List<string> { mn } : new List<string>();
                editFps = 1f;
                break;
        }
        if (fpsText != null) fpsText.text = ((int)editFps).ToString();
    }

    void SaveToTarget()
    {
        var csm = CharacterStyleManager.Instance;
        if (csm == null) return;
        switch (activeTarget)
        {
            case Target.Player: csm.SetPlayerAnim(editFrameNames, editFps); break;
            case Target.Enemy:  csm.SetEnemyAnim(editFrameNames, editFps); break;
            case Target.Magic:
                csm.SetMagicSprite(editFrameNames.Count > 0 ? editFrameNames[0] : "");
                break;
        }
    }

    void AddFrame(string spriteName)
    {
        if (activeTarget == Target.Magic)
        {
            editFrameNames.Clear();
            editFrameNames.Add(spriteName);
        }
        else
        {
            editFrameNames.Add(spriteName);
        }
        RefreshFrameList();
        UpdatePreviewStatic();
    }

    void RemoveFrame(int index)
    {
        if (index < 0 || index >= editFrameNames.Count) return;
        editFrameNames.RemoveAt(index);
        RefreshFrameList();
        UpdatePreviewStatic();
    }

    // ─── プレビュー ─────────────────────────────────

    void StartPreview()
    {
        StopPreview();
        if (editFrameNames.Count > 0)
            previewCoroutine = StartCoroutine(PreviewCoroutine());
    }

    void StopPreview()
    {
        if (previewCoroutine != null) { StopCoroutine(previewCoroutine); previewCoroutine = null; }
    }

    void UpdatePreviewStatic()
    {
        StopPreview();
        if (previewImage == null || editFrameNames.Count == 0) return;
        var s = CharacterStyleManager.Instance?.FindSprite(editFrameNames[0]);
        previewImage.sprite = s;
        previewImage.color = s != null ? Color.white : new Color(0.5f, 0.5f, 0.5f);
    }

    IEnumerator PreviewCoroutine()
    {
        int idx = 0;
        while (true)
        {
            if (editFrameNames.Count == 0) yield break;
            idx %= editFrameNames.Count;
            var s = CharacterStyleManager.Instance?.FindSprite(editFrameNames[idx]);
            if (previewImage != null)
            {
                previewImage.sprite = s;
                previewImage.color = s != null ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }
            if (editFrameNames.Count > 1) idx++;
            yield return new WaitForSeconds(editFps > 0 ? 1f / editFps : 0.5f);
        }
    }

    // ─── UI構築 ────────────────────────────────────

    void BuildUI()
    {
        canvas = GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 65;
        if (GetComponent<CanvasScaler>() == null)
        {
            var cs = gameObject.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.matchWidthOrHeight = 0.5f;
        }
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("CharEditRoot");
        panelRoot.transform.SetParent(transform, false);
        var rootRT = panelRoot.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;
        panelRoot.AddComponent<Image>().color = new Color(0.96f, 0.93f, 0.87f);

        BuildTitleBar();
        BuildTargetTabs();
        BuildMainArea();
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
        bar.AddComponent<Image>().color = TITLE_COLOR;

        var tGO = new GameObject("T");
        tGO.transform.SetParent(bar.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = "キャラクター設定";
        t.font = font;
        t.fontSize = 34;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.18f, 0.08f, 0.04f);
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

    void BuildTargetTabs()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var tabsGO = new GameObject("TargetTabs");
        tabsGO.transform.SetParent(panelRoot.transform, false);
        var tabsRT = tabsGO.AddComponent<RectTransform>();
        tabsRT.anchorMin = new Vector2(0f, 1f);
        tabsRT.anchorMax = new Vector2(1f, 1f);
        tabsRT.pivot = new Vector2(0.5f, 1f);
        tabsRT.sizeDelta = new Vector2(0f, 64f);
        tabsRT.anchoredPosition = new Vector2(0f, -72f);

        var hl = tabsGO.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandWidth = true;
        hl.childForceExpandHeight = true;
        hl.spacing = 6f;
        hl.padding = new RectOffset(10, 10, 8, 8);

        var targets = new[] { (Target.Player, "プレイヤー"), (Target.Enemy, "敵"), (Target.Magic, "魔法") };
        foreach (var (target, label) in targets)
        {
            var t = target;
            var tabGO = new GameObject("Tab_" + label);
            tabGO.transform.SetParent(tabsGO.transform, false);
            var tabImg = tabGO.AddComponent<Image>();
            tabImg.color = t == activeTarget ? TAB_ACTIVE : TAB_INACTIVE;
            targetTabImgs[t] = tabImg;
            var tabBtn = tabGO.AddComponent<Button>();
            tabBtn.targetGraphic = tabImg;
            tabBtn.onClick.AddListener(() => SelectTarget(t));

            var lGO = new GameObject("L");
            lGO.transform.SetParent(tabGO.transform, false);
            var lt = lGO.AddComponent<Text>();
            lt.text = label;
            lt.font = font;
            lt.fontSize = 24;
            lt.fontStyle = FontStyle.Bold;
            lt.color = new Color(0.18f, 0.08f, 0.04f);
            lt.alignment = TextAnchor.MiddleCenter;
            lt.raycastTarget = false;
            var lr = lGO.AddComponent<RectTransform>();
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.sizeDelta = Vector2.zero;
        }
    }

    void BuildMainArea()
    {
        // タイトルバー(72) + タブ(64) = 136px 下
        var area = new GameObject("MainArea");
        area.transform.SetParent(panelRoot.transform, false);
        var areaRT = area.AddComponent<RectTransform>();
        areaRT.anchorMin = Vector2.zero;
        areaRT.anchorMax = Vector2.one;
        areaRT.offsetMin = Vector2.zero;
        areaRT.offsetMax = new Vector2(0f, -136f);

        var hl = area.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.spacing = 0f;

        BuildPalettePanel(area.transform);
        BuildDivider(area.transform);
        BuildEditorPanel(area.transform);
    }

    // ─── 左：スプライトパレット ─────────────────────

    void BuildPalettePanel(Transform parent)
    {
        var go = new GameObject("Palette");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
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
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRT;
        scroll.viewport = vpRT;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 40f;

        // ヘッダー
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var headerGO = new GameObject("Header");
        headerGO.transform.SetParent(contentGO.transform, false);
        headerGO.AddComponent<Image>().color = new Color(0.82f, 0.68f, 1f);
        var headerLE = headerGO.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 36f;
        headerLE.flexibleWidth = 1f;
        var ht = new GameObject("T").AddComponent<Text>();
        ht.transform.SetParent(headerGO.transform, false);
        ht.text = "スプライト一覧";
        ht.font = font;
        ht.fontSize = 18;
        ht.fontStyle = FontStyle.Bold;
        ht.color = new Color(0.15f, 0.08f, 0.08f);
        ht.alignment = TextAnchor.MiddleLeft;
        ht.raycastTarget = false;
        var htRT = ht.GetComponent<RectTransform>();
        htRT.anchorMin = Vector2.zero;
        htRT.anchorMax = Vector2.one;
        htRT.offsetMin = new Vector2(12f, 0f);
        htRT.offsetMax = Vector2.zero;

        // スプライトグリッド
        var gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(contentGO.transform, false);
        var glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(80f, 80f);
        glg.spacing = new Vector2(6f, 6f);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.padding = new RectOffset(8, 8, 8, 8);
        gridGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        gridGO.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var sprites = CharacterStyleManager.Instance?.GetAllSprites() ?? new List<Sprite>();
        foreach (var sprite in sprites)
        {
            var s = sprite;
            var cell = new GameObject(s.name);
            cell.transform.SetParent(gridGO.transform, false);
            var cellImg = cell.AddComponent<Image>();
            cellImg.sprite = s;
            cellImg.preserveAspect = true;
            cellImg.color = Color.white;
            var cellBg = cell.AddComponent<Image>();
            cellBg.color = new Color(0.85f, 0.82f, 0.78f);
            // cellImg needs to be child, cellBg is background
            // Fix: use background image on cell, sprite on child
            Destroy(cellImg); // remove cellImg from root
            Destroy(cellBg);

            // Rebuild cleanly
            var bgImg = cell.AddComponent<Image>();
            bgImg.color = new Color(0.85f, 0.82f, 0.78f);
            var cellBtn = cell.AddComponent<Button>();
            cellBtn.targetGraphic = bgImg;
            var spriteGO = new GameObject("Sprite");
            spriteGO.transform.SetParent(cell.transform, false);
            var spriteImg = spriteGO.AddComponent<Image>();
            spriteImg.sprite = s;
            spriteImg.preserveAspect = true;
            spriteImg.color = Color.white;
            spriteImg.raycastTarget = false;
            var spriteRT = spriteGO.AddComponent<RectTransform>();
            spriteRT.anchorMin = new Vector2(0.05f, 0.05f);
            spriteRT.anchorMax = new Vector2(0.95f, 0.95f);
            spriteRT.sizeDelta = Vector2.zero;
            cellBtn.onClick.AddListener(() => AddFrame(s.name));
        }
    }

    // ─── 区切り ─────────────────────────────────────

    void BuildDivider(Transform parent)
    {
        var go = new GameObject("Div");
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = new Color(0.72f, 0.68f, 0.60f);
        go.AddComponent<LayoutElement>().preferredWidth = 2f;
    }

    // ─── 右：アニメーションエディター ──────────────

    void BuildEditorPanel(Transform parent)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var go = new GameObject("Editor");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1.2f;
        le.flexibleHeight = 1f;
        go.AddComponent<Image>().color = new Color(0.92f, 0.89f, 0.83f);

        var scroll = go.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 40f;

        var vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(go.transform, false);
        var vpRT = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.sizeDelta = Vector2.zero;
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;

        var contentGO = new GameObject("EditorContent");
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = Vector2.zero;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vl = contentGO.AddComponent<VerticalLayoutGroup>();
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        vl.spacing = 12f;
        vl.padding = new RectOffset(12, 12, 16, 16);

        scroll.content = contentRT;
        scroll.viewport = vpRT;

        // 【コマ列】セクションヘッダー
        BuildEditorSectionHeader(contentGO.transform, "コマ（タップで追加 / ✕で削除）",
            new Color(0.68f, 0.88f, 1f), font);

        // コマ横スクロール
        var frameScrollGO = new GameObject("FrameScroll");
        frameScrollGO.transform.SetParent(contentGO.transform, false);
        var fsLE = frameScrollGO.AddComponent<LayoutElement>();
        fsLE.preferredHeight = 100f;
        var frameScroll = frameScrollGO.AddComponent<ScrollRect>();
        frameScroll.vertical = false;
        frameScroll.horizontal = true;
        frameScroll.scrollSensitivity = 30f;

        var fsVP = new GameObject("FsVP");
        fsVP.transform.SetParent(frameScrollGO.transform, false);
        var fsVPRT = fsVP.AddComponent<RectTransform>();
        fsVPRT.anchorMin = Vector2.zero;
        fsVPRT.anchorMax = Vector2.one;
        fsVPRT.sizeDelta = Vector2.zero;
        fsVP.AddComponent<Image>().color = Color.clear;
        fsVP.AddComponent<Mask>().showMaskGraphic = false;

        var fsContent = new GameObject("FrameListContent");
        fsContent.transform.SetParent(fsVP.transform, false);
        var fsContentRT = fsContent.AddComponent<RectTransform>();
        fsContentRT.anchorMin = new Vector2(0f, 0f);
        fsContentRT.anchorMax = new Vector2(0f, 1f);
        fsContentRT.pivot = new Vector2(0f, 0.5f);
        fsContentRT.sizeDelta = new Vector2(0f, 0f);
        var fsHL = fsContent.AddComponent<HorizontalLayoutGroup>();
        fsHL.childForceExpandWidth = false;
        fsHL.childForceExpandHeight = true;
        fsHL.spacing = 8f;
        fsHL.padding = new RectOffset(8, 8, 4, 4);
        fsContent.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        frameScroll.content = fsContentRT;
        frameScroll.viewport = fsVPRT;
        frameListContent = fsContent.transform;

        // 【FPS】（魔法以外）
        BuildEditorSectionHeader(contentGO.transform, "再生速度 (FPS)", new Color(1f, 0.92f, 0.58f), font);
        BuildFpsControl(contentGO.transform, font);

        // 【プレビュー】
        BuildEditorSectionHeader(contentGO.transform, "プレビュー", new Color(0.68f, 0.94f, 0.68f), font);
        BuildPreviewArea(contentGO.transform, font);

        // 【保存】
        BuildSaveButton(contentGO.transform, font);
    }

    void BuildEditorSectionHeader(Transform parent, string label, Color color, Font font)
    {
        var go = new GameObject("SH_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        go.AddComponent<LayoutElement>().preferredHeight = 36f;

        var tGO = new GameObject("T");
        tGO.transform.SetParent(go.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 18;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.15f, 0.08f, 0.06f);
        t.alignment = TextAnchor.MiddleLeft;
        t.raycastTarget = false;
        var tr = tGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(10f, 0f);
        tr.offsetMax = Vector2.zero;
    }

    void BuildFpsControl(Transform parent, Font font)
    {
        var row = new GameObject("FpsRow");
        row.transform.SetParent(parent, false);
        row.AddComponent<LayoutElement>().preferredHeight = 64f;
        var hl = row.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandHeight = true;
        hl.childForceExpandWidth = false;
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.spacing = 16f;

        MakeSimpleBtn(row.transform, "−", new Color(0.95f, 0.52f, 0.52f), font, () =>
        {
            editFps = Mathf.Max(1f, editFps - 1f);
            if (fpsText != null) fpsText.text = ((int)editFps).ToString();
        });

        var valGO = new GameObject("Val");
        valGO.transform.SetParent(row.transform, false);
        valGO.AddComponent<LayoutElement>().preferredWidth = 80f;
        fpsText = valGO.AddComponent<Text>();
        fpsText.text = ((int)editFps).ToString();
        fpsText.font = font;
        fpsText.fontSize = 30;
        fpsText.fontStyle = FontStyle.Bold;
        fpsText.color = new Color(0.15f, 0.08f, 0.06f);
        fpsText.alignment = TextAnchor.MiddleCenter;
        fpsText.raycastTarget = false;

        MakeSimpleBtn(row.transform, "＋", new Color(0.52f, 0.82f, 0.52f), font, () =>
        {
            editFps = Mathf.Min(30f, editFps + 1f);
            if (fpsText != null) fpsText.text = ((int)editFps).ToString();
        });
    }

    void BuildPreviewArea(Transform parent, Font font)
    {
        var area = new GameObject("PreviewArea");
        area.transform.SetParent(parent, false);
        var areaLE = area.AddComponent<LayoutElement>();
        areaLE.preferredHeight = 160f;
        var areaHL = area.AddComponent<HorizontalLayoutGroup>();
        areaHL.childForceExpandHeight = true;
        areaHL.childForceExpandWidth = false;
        areaHL.childAlignment = TextAnchor.MiddleCenter;
        areaHL.spacing = 20f;

        // プレビュー画像
        var previewGO = new GameObject("Preview");
        previewGO.transform.SetParent(area.transform, false);
        var previewLE = previewGO.AddComponent<LayoutElement>();
        previewLE.preferredWidth = 140f;
        previewLE.preferredHeight = 140f;
        previewGO.AddComponent<Image>().color = new Color(0.75f, 0.72f, 0.68f);

        var previewSpriteGO = new GameObject("PreviewSprite");
        previewSpriteGO.transform.SetParent(previewGO.transform, false);
        previewImage = previewSpriteGO.AddComponent<Image>();
        previewImage.color = Color.clear;
        previewImage.preserveAspect = true;
        previewImage.raycastTarget = false;
        var psRT = previewSpriteGO.AddComponent<RectTransform>();
        psRT.anchorMin = new Vector2(0.05f, 0.05f);
        psRT.anchorMax = new Vector2(0.95f, 0.95f);
        psRT.sizeDelta = Vector2.zero;

        // ▶ ボタン
        MakeSimpleBtn(area.transform, "▶", new Color(0.4f, 0.65f, 1f), font, StartPreview, 60f);
    }

    void BuildSaveButton(Transform parent, Font font)
    {
        var spacer = new GameObject("Sp");
        spacer.transform.SetParent(parent, false);
        spacer.AddComponent<LayoutElement>().preferredHeight = 8f;

        var btnGO = new GameObject("SaveBtn");
        btnGO.transform.SetParent(parent, false);
        btnGO.AddComponent<LayoutElement>().preferredHeight = 72f;

        var hl = btnGO.AddComponent<HorizontalLayoutGroup>();
        hl.childForceExpandHeight = true;
        hl.childForceExpandWidth = false;
        hl.childAlignment = TextAnchor.MiddleCenter;

        var btn = MakeSimpleBtn(btnGO.transform, "💾  保存", new Color(1f, 0.71f, 0.71f), font, () =>
        {
            SaveToTarget();
        }, 240f);
    }

    Button MakeSimpleBtn(Transform parent, string label, Color color, Font font,
                         System.Action onClick, float width = 64f)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = 56f;
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());

        var tGO = new GameObject("T");
        tGO.transform.SetParent(go.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = width >= 120f ? 24 : 28;
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

    // ─── フレームリスト更新 ─────────────────────────

    void RefreshFrameList()
    {
        if (frameListContent == null) return;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        foreach (Transform child in frameListContent) Destroy(child.gameObject);

        for (int i = 0; i < editFrameNames.Count; i++)
        {
            int idx = i;
            var name = editFrameNames[i];
            var sprite = CharacterStyleManager.Instance?.FindSprite(name);

            var cell = new GameObject("Frame_" + i);
            cell.transform.SetParent(frameListContent, false);
            var cellLE = cell.AddComponent<LayoutElement>();
            cellLE.preferredWidth = 88f;
            cellLE.preferredHeight = 88f;
            cell.AddComponent<Image>().color = FRAME_BG;

            if (sprite != null)
            {
                var sprGO = new GameObject("Spr");
                sprGO.transform.SetParent(cell.transform, false);
                var sprImg = sprGO.AddComponent<Image>();
                sprImg.sprite = sprite;
                sprImg.preserveAspect = true;
                sprImg.raycastTarget = false;
                var sprRT = sprGO.AddComponent<RectTransform>();
                sprRT.anchorMin = new Vector2(0.05f, 0.2f);
                sprRT.anchorMax = new Vector2(0.95f, 1f);
                sprRT.sizeDelta = Vector2.zero;
            }

            // ✕ 削除ボタン
            var delGO = new GameObject("Del");
            delGO.transform.SetParent(cell.transform, false);
            var delRT = delGO.AddComponent<RectTransform>();
            delRT.anchorMin = new Vector2(0f, 0f);
            delRT.anchorMax = new Vector2(1f, 0.22f);
            delRT.sizeDelta = Vector2.zero;
            var delImg = delGO.AddComponent<Image>();
            delImg.color = new Color(0.85f, 0.3f, 0.3f);
            var delBtn = delGO.AddComponent<Button>();
            delBtn.targetGraphic = delImg;
            delBtn.onClick.AddListener(() => RemoveFrame(idx));
            var delTGO = new GameObject("T");
            delTGO.transform.SetParent(delGO.transform, false);
            var delT = delTGO.AddComponent<Text>();
            delT.text = "✕";
            delT.font = font;
            delT.fontSize = 14;
            delT.color = Color.white;
            delT.alignment = TextAnchor.MiddleCenter;
            delT.raycastTarget = false;
            var delTRT = delTGO.AddComponent<RectTransform>();
            delTRT.anchorMin = Vector2.zero;
            delTRT.anchorMax = Vector2.one;
            delTRT.sizeDelta = Vector2.zero;
        }

        // コマ数ラベル
        var countGO = new GameObject("Count");
        countGO.transform.SetParent(frameListContent, false);
        countGO.AddComponent<LayoutElement>().preferredWidth = 60f;
        var countT = countGO.AddComponent<Text>();
        countT.text = editFrameNames.Count == 0 ? "← スプライトをタップして追加" : $"{editFrameNames.Count}コマ";
        countT.font = font;
        countT.fontSize = 14;
        countT.color = new Color(0.5f, 0.45f, 0.4f);
        countT.alignment = TextAnchor.MiddleLeft;
        countT.raycastTarget = false;

        UpdatePreviewStatic();
    }
}
