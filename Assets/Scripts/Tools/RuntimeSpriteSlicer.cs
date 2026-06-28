using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RuntimeSpriteSlicer : MonoBehaviour
{
    public static RuntimeSpriteSlicer Instance { get; private set; }

    private string targetAssetPath = "Assets/Sprites/magic_icons2.png";
    private Texture2D texture;
    private List<SliceRect> slices = new List<SliceRect>();

    // UI
    private GameObject panel;
    private RawImage texDisplay;
    private Transform sliceListRoot;
    private GameObject selectionBox;
    private Text statusText;

    private bool isDragging;
    private Vector2 dragStart;
    private RectTransform displayRect;
    private Canvas rootCanvas;

    [System.Serializable]
    public class SliceRect
    {
        public string name;
        public Rect rect;
    }

    void Awake() { Instance = this; }

    void Start()
    {
        rootCanvas = FindObjectOfType<Canvas>();
        BuildUI();
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("SlicerCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        rootCanvas = canvas;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        panel = new GameObject("SlicerPanel");
        panel.AddComponent<RectTransform>();
        panel.transform.SetParent(canvasGO.transform, false);
        var pi = panel.AddComponent<Image>(); pi.color = new Color(0.07f,0.07f,0.1f,0.97f);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one; pr.sizeDelta = Vector2.zero;
        panel.SetActive(false);

        // ヘッダー
        var header = new GameObject("Header"); header.AddComponent<RectTransform>(); header.transform.SetParent(panel.transform, false);
        var hImg = header.AddComponent<Image>(); hImg.color = new Color(0.15f,0.15f,0.22f);
        var hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0,1); hRect.anchorMax = Vector2.one;
        hRect.offsetMin = new Vector2(0,-48); hRect.offsetMax = Vector2.zero;
        var hLayout = header.AddComponent<HorizontalLayoutGroup>();
        hLayout.padding = new RectOffset(10,10,8,8); hLayout.spacing = 8; hLayout.childControlHeight = true; hLayout.childForceExpandHeight = true;

        MakeHeaderText(header.transform, "🔪 スプライトスライサー", font, 16, Color.white);

        // ステータス
        var statusGO = new GameObject("Status"); statusGO.AddComponent<RectTransform>(); statusGO.transform.SetParent(header.transform, false);
        statusText = statusGO.AddComponent<Text>(); statusText.font = font; statusText.fontSize = 12; statusText.color = new Color(0.8f,0.8f,0.5f);
        statusGO.AddComponent<LayoutElement>().flexibleWidth = 1;

        MakeBtn(header.transform, "Auto", new Color(0.3f,0.5f,0.3f), font, AutoDetect);
        MakeBtn(header.transform, "Clear", new Color(0.5f,0.3f,0.3f), font, ClearAll);
        MakeBtn(header.transform, "✅ Apply", new Color(0.2f,0.6f,0.3f), font, ApplyToUnity);
        MakeBtn(header.transform, "✕ 閉じる", new Color(0.5f,0.2f,0.2f), font, () => panel.SetActive(false));

        // メインエリア（左：テクスチャ、右：スライス一覧）
        var main = new GameObject("Main"); main.AddComponent<RectTransform>(); main.transform.SetParent(panel.transform, false);
        var mRect = main.GetComponent<RectTransform>();
        mRect.anchorMin = Vector2.zero; mRect.anchorMax = new Vector2(1,1);
        mRect.offsetMin = new Vector2(0,0); mRect.offsetMax = new Vector2(0,-48);
        var mLayout = main.AddComponent<HorizontalLayoutGroup>();
        mLayout.childControlWidth = true; mLayout.childControlHeight = true; mLayout.childForceExpandWidth = false; mLayout.childForceExpandHeight = true;

        // テクスチャ表示エリア
        var texArea = new GameObject("TexArea"); texArea.AddComponent<RectTransform>(); texArea.transform.SetParent(main.transform, false);
        texArea.AddComponent<Image>().color = new Color(0.3f,0.3f,0.35f);
        texArea.AddComponent<LayoutElement>().flexibleWidth = 3;

        texDisplay = new GameObject("TexDisplay").AddComponent<RawImage>();
        texDisplay.transform.SetParent(texArea.transform, false);
        texDisplay.color = Color.white;
        displayRect = texDisplay.GetComponent<RectTransform>();
        displayRect.anchorMin = new Vector2(0.02f,0.02f); displayRect.anchorMax = new Vector2(0.98f,0.98f); displayRect.sizeDelta = Vector2.zero;

        // 選択ボックス
        selectionBox = new GameObject("SelectionBox"); selectionBox.AddComponent<RectTransform>(); selectionBox.transform.SetParent(texArea.transform, false);
        var sbImg = selectionBox.AddComponent<Image>(); sbImg.color = new Color(0,1,0,0.25f);
        var outline = selectionBox.AddComponent<Outline>(); outline.effectColor = Color.green; outline.effectDistance = new Vector2(2,2);
        selectionBox.SetActive(false);

        // 右側：スライス一覧
        var listArea = new GameObject("ListArea"); listArea.AddComponent<RectTransform>(); listArea.transform.SetParent(main.transform, false);
        listArea.AddComponent<Image>().color = new Color(0.1f,0.1f,0.15f);
        listArea.AddComponent<LayoutElement>().preferredWidth = 220;

        var listVL = listArea.AddComponent<VerticalLayoutGroup>();
        listVL.padding = new RectOffset(6,6,6,6); listVL.spacing = 3; listVL.childControlWidth = true; listVL.childControlHeight = false; listVL.childForceExpandWidth = true;

        var listTitle = new GameObject("Title"); listTitle.AddComponent<RectTransform>(); listTitle.transform.SetParent(listArea.transform, false);
        var lt = listTitle.AddComponent<Text>(); lt.text = "スライス一覧"; lt.font = font; lt.fontSize = 13; lt.color = Color.white;
        listTitle.AddComponent<LayoutElement>().preferredHeight = 24;

        var scrollGO = new GameObject("Scroll"); scrollGO.AddComponent<RectTransform>(); scrollGO.transform.SetParent(listArea.transform, false);
        scrollGO.AddComponent<LayoutElement>().flexibleHeight = 1;
        var scroll = scrollGO.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true;

        var vp = new GameObject("VP"); vp.AddComponent<RectTransform>(); vp.transform.SetParent(scrollGO.transform, false);
        vp.AddComponent<Image>().color = new Color(0,0,0,0.01f); vp.AddComponent<Mask>().showMaskGraphic = false;
        var vpR = vp.GetComponent<RectTransform>(); vpR.anchorMin = Vector2.zero; vpR.anchorMax = Vector2.one; vpR.sizeDelta = Vector2.zero;
        scroll.viewport = vpR;

        var content = new GameObject("Content"); content.AddComponent<RectTransform>(); content.transform.SetParent(vp.transform, false);
        var cR = content.GetComponent<RectTransform>(); cR.anchorMin = new Vector2(0,1); cR.anchorMax = new Vector2(1,1); cR.pivot = new Vector2(0.5f,1f); cR.sizeDelta = Vector2.zero;
        var cvl = content.AddComponent<VerticalLayoutGroup>(); cvl.spacing = 3; cvl.childControlWidth = true; cvl.childControlHeight = false; cvl.childForceExpandWidth = true;
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = cR;
        sliceListRoot = content.transform;

        // テクスチャを読み込む
        LoadTexture(targetAssetPath);
    }

    void MakeHeaderText(Transform parent, string text, Font font, int size, Color col)
    {
        var go = new GameObject("T"); go.AddComponent<RectTransform>(); go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>(); t.text = text; t.font = font; t.fontSize = size; t.color = col; t.alignment = TextAnchor.MiddleLeft;
        go.AddComponent<LayoutElement>().preferredWidth = 220;
    }

    void MakeBtn(Transform parent, string label, Color col, Font font, System.Action action)
    {
        var go = new GameObject(label); go.AddComponent<RectTransform>(); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = col;
        var le = go.AddComponent<LayoutElement>(); le.preferredWidth = 80; le.preferredHeight = 32;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(() => action());
        var tGO = new GameObject("T"); tGO.AddComponent<RectTransform>(); tGO.transform.SetParent(go.transform, false);
        var t = tGO.AddComponent<Text>(); t.text = label; t.font = font; t.fontSize = 12; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter;
        var tr = tGO.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
    }

    public void Open(string assetPath)
    {
        targetAssetPath = assetPath;
        LoadTexture(assetPath);
        panel.SetActive(true);
    }

    void LoadTexture(string path)
    {
#if UNITY_EDITOR
        texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texDisplay != null && texture != null)
        {
            texDisplay.texture = texture;
            // 既存スライスを読み込む
            slices.Clear();
            var imp = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            if (imp?.spritesheet != null)
                foreach (var s in imp.spritesheet)
                    slices.Add(new SliceRect { name = s.name, rect = s.rect });
            UpdateSliceList();
            SetStatus($"読み込み: {path} ({texture.width}x{texture.height}) / {slices.Count}スライス");
        }
#endif
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf) return;
        if (Mouse.current == null) return;

        var mousePos = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(displayRect, mousePos, null, out local))
            {
                if (displayRect.rect.Contains(local))
                {
                    isDragging = true;
                    dragStart = local;
                }
            }
        }

        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(displayRect, mousePos, null, out local))
            {
                selectionBox.SetActive(true);
                var sbRect = selectionBox.GetComponent<RectTransform>();
                Vector2 min = Vector2.Min(dragStart, local);
                Vector2 max = Vector2.Max(dragStart, local);
                sbRect.anchorMin = sbRect.anchorMax = Vector2.zero;
                sbRect.anchoredPosition = min + displayRect.rect.min;
                sbRect.sizeDelta = max - min;
            }
        }

        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            selectionBox.SetActive(false);

            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(displayRect, mousePos, null, out local))
            {
                AddSliceFromUI(dragStart, local);
            }
        }
    }

    void AddSliceFromUI(Vector2 start, Vector2 end)
    {
        if (texture == null) return;
        var dRect = displayRect.rect;
        float scaleX = texture.width / dRect.width;
        float scaleY = texture.height / dRect.height;

        float x1 = (Mathf.Min(start.x, end.x) - dRect.xMin) * scaleX;
        float y1 = (Mathf.Min(start.y, end.y) - dRect.yMin) * scaleY;
        float w = Mathf.Abs(end.x - start.x) * scaleX;
        float h = Mathf.Abs(end.y - start.y) * scaleY;

        if (w < 4 || h < 4) return;

        var newSlice = new SliceRect
        {
            name = "sprite_" + slices.Count,
            rect = new Rect(x1, y1, w, h)
        };
        slices.Add(newSlice);
        UpdateSliceList();
        SetStatus($"スライス追加: {newSlice.name} ({x1:F0},{y1:F0} {w:F0}x{h:F0})");
    }

    void AutoDetect()
    {
#if UNITY_EDITOR
        if (texture == null) return;
        slices.Clear();
        int W = texture.width, H = texture.height;

        System.Func<int,int,bool> isContent = (x,y) => {
            var p = texture.GetPixel(x,y);
            return p.a > 0.1f && !(p.r > 0.92f && p.g > 0.92f && p.b > 0.92f);
        };

        var rowBands = new List<int[]>();
        bool inRow = false; int rowStart2 = 0;
        for (int y = 0; y < H; y++) {
            bool has = false;
            for (int x = 0; x < W; x++) if (isContent(x,y)) { has = true; break; }
            if (has && !inRow) { inRow = true; rowStart2 = y; }
            else if (!has && inRow) { inRow = false; rowBands.Add(new int[]{rowStart2,y}); }
        }
        if (inRow) rowBands.Add(new int[]{rowStart2,H});

        int idx = 0;
        for (int ri = 0; ri < rowBands.Count; ri++) {
            int y0 = rowBands[ri][0], y1 = rowBands[ri][1];
            bool inCol = false; int cStart = 0;
            for (int x = 0; x <= W; x++) {
                bool has = false;
                if (x < W) for (int y = y0; y < y1; y++) if (isContent(x,y)) { has = true; break; }
                if (has && !inCol) { inCol = true; cStart = x; }
                else if (!has && inCol) {
                    inCol = false;
                    slices.Add(new SliceRect { name = "sprite_" + idx++, rect = new Rect(cStart, y0, x-cStart, y1-y0) });
                }
            }
        }
        UpdateSliceList();
        SetStatus($"自動検出: {slices.Count}枚");
#endif
    }

    void UpdateSliceList()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        foreach (Transform child in sliceListRoot) Destroy(child.gameObject);
        for (int i = 0; i < slices.Count; i++)
        {
            int captureI = i;
            var s = slices[i];
            var row = new GameObject("Row"); row.AddComponent<RectTransform>(); row.transform.SetParent(sliceListRoot, false);
            var rowImg = row.AddComponent<Image>(); rowImg.color = new Color(0.15f,0.15f,0.22f);
            var hl = row.AddComponent<HorizontalLayoutGroup>(); hl.padding = new RectOffset(4,4,2,2); hl.spacing = 4; hl.childControlHeight = true; hl.childForceExpandHeight = true;
            row.AddComponent<LayoutElement>().preferredHeight = 28;

            var nameGO = new GameObject("N"); nameGO.AddComponent<RectTransform>(); nameGO.transform.SetParent(row.transform, false);
            var nt = nameGO.AddComponent<Text>(); nt.text = s.name; nt.font = font; nt.fontSize = 10; nt.color = Color.white;
            nameGO.AddComponent<LayoutElement>().flexibleWidth = 1;

            var infoGO = new GameObject("I"); infoGO.AddComponent<RectTransform>(); infoGO.transform.SetParent(row.transform, false);
            var it = infoGO.AddComponent<Text>(); it.text = $"{s.rect.x:F0},{s.rect.y:F0} {s.rect.width:F0}x{s.rect.height:F0}"; it.font = font; it.fontSize = 9; it.color = new Color(0.7f,0.7f,0.7f);
            infoGO.AddComponent<LayoutElement>().preferredWidth = 100;

            var delGO = new GameObject("D"); delGO.AddComponent<RectTransform>(); delGO.transform.SetParent(row.transform, false);
            var di = delGO.AddComponent<Image>(); di.color = new Color(0.6f,0.2f,0.2f);
            var dle = delGO.AddComponent<LayoutElement>(); dle.preferredWidth = 24;
            var db = delGO.AddComponent<Button>(); db.targetGraphic = di; db.onClick.AddListener(() => { slices.RemoveAt(captureI); UpdateSliceList(); });
            var dt = new GameObject("T"); dt.AddComponent<RectTransform>(); dt.transform.SetParent(delGO.transform, false);
            var dtt = dt.AddComponent<Text>(); dtt.text = "✕"; dtt.font = font; dtt.fontSize = 11; dtt.color = Color.white; dtt.alignment = TextAnchor.MiddleCenter;
            var dtr = dt.GetComponent<RectTransform>(); dtr.anchorMin = Vector2.zero; dtr.anchorMax = Vector2.one; dtr.sizeDelta = Vector2.zero;
        }
    }

    void ClearAll()
    {
        slices.Clear();
        UpdateSliceList();
        SetStatus("スライスをクリアしました");
    }

    void ApplyToUnity()
    {
#if UNITY_EDITOR
        if (slices.Count == 0) { SetStatus("⚠ スライスがありません"); return; }
        var imp = UnityEditor.AssetImporter.GetAtPath(targetAssetPath) as UnityEditor.TextureImporter;
        if (imp == null) { SetStatus("⚠ インポーターが見つかりません"); return; }

        var metaDatas = new UnityEditor.SpriteMetaData[slices.Count];
        for (int i = 0; i < slices.Count; i++)
            metaDatas[i] = new UnityEditor.SpriteMetaData {
                name = slices[i].name,
                rect = slices[i].rect,
                pivot = new Vector2(0.5f, 0.5f)
            };
        imp.spriteImportMode = UnityEditor.SpriteImportMode.Multiple;
        imp.spritesheet = metaDatas;
        imp.SaveAndReimport();
        SetStatus($"✅ Unity に適用完了！ {slices.Count}スライス → {targetAssetPath}");
#endif
    }

    void SetStatus(string msg) { if (statusText) statusText.text = msg; }

    public void OpenForAsset(string path)
    {
        targetAssetPath = path;
        LoadTexture(path);
        panel.SetActive(true);
    }
}
