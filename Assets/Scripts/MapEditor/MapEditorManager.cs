using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapEditorManager : MonoBehaviour
{
    public static MapEditorManager Instance { get; private set; }
    public static bool IsEditorMode { get; private set; }

    public enum EditorTab { Sticker, Ground, Background }
    private EditorTab currentTab = EditorTab.Sticker;

    [System.Serializable]
    public class StickerDef { public string name; public Sprite sprite; public Color tint = Color.white; public string tag = "Untagged"; public float scale = 1f; public bool isSolid = false; public string category = "その他"; }

    public List<StickerDef> stickerDefs = new List<StickerDef>();
    public Material outlineMaterial;
    public GameObject editorPanel;
    public Transform paletteContent;
    public Transform groundPaletteContent;

    private StickerDef selectedDef;
    private Image selectedBtnImg;
    private Camera mainCam;

    // タブUI参照
    private GameObject stickerTabContent;
    private GameObject groundTabContent;
    private GameObject bgTabContent;

void Awake() { Instance = this; mainCam = Camera.main; if (GetComponent<GroundPainter>() == null) gameObject.AddComponent<GroundPainter>(); if (GetComponent<BackgroundManager>() == null) gameObject.AddComponent<BackgroundManager>(); if (GetComponent<SpritePicker>() == null) gameObject.AddComponent<SpritePicker>(); BuildUI(); BuildPaletteUI(); }

void BuildUI() { var oldCanvas = GameObject.Find("MapEditor_Canvas"); if (oldCanvas != null) Destroy(oldCanvas); var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); var canvasGO = new GameObject("MapEditor_Canvas"); var canvas = canvasGO.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 5; canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>(); canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>(); var panel = new GameObject("Panel"); panel.AddComponent<RectTransform>(); panel.transform.SetParent(canvasGO.transform, false); panel.AddComponent<UnityEngine.UI.Image>().color = new Color(.08f,.1f,.15f,.97f); var pr = panel.GetComponent<RectTransform>(); pr.anchorMin = Vector2.zero; pr.anchorMax = new Vector2(.25f, 1f); pr.offsetMin = pr.offsetMax = Vector2.zero; panel.SetActive(false); var vl = panel.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); vl.padding = new RectOffset(8,8,8,8); vl.spacing = 5; vl.childControlWidth = true; vl.childControlHeight = false; vl.childForceExpandWidth = true; editorPanel = panel; var titleGO = new GameObject("T"); titleGO.AddComponent<RectTransform>(); titleGO.transform.SetParent(panel.transform, false); var tt = titleGO.AddComponent<UnityEngine.UI.Text>(); tt.text = "🗺 マップエディタ  [M]"; tt.font = font; tt.fontSize = 13; tt.color = Color.white; titleGO.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 22; var tabRow = new GameObject("Tabs"); tabRow.AddComponent<RectTransform>(); tabRow.transform.SetParent(panel.transform, false); var th = tabRow.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>(); th.spacing = 3; th.childForceExpandWidth = true; th.childControlHeight = false; tabRow.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 30; System.Action<Transform,string,Color,System.Action> addBtn = (p,txt,col,act) => { var bg = new GameObject("B"); bg.AddComponent<RectTransform>(); bg.transform.SetParent(p, false); var bi = bg.AddComponent<UnityEngine.UI.Image>(); bi.color = col; bg.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 34; var b = bg.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = bi; b.onClick.AddListener(() => act()); var lg = new GameObject("T"); lg.AddComponent<RectTransform>(); lg.transform.SetParent(bg.transform, false); var lt = lg.AddComponent<UnityEngine.UI.Text>(); lt.text = txt; lt.font = font; lt.fontSize = 11; lt.color = Color.white; lt.alignment = TextAnchor.MiddleCenter; var lr = lg.GetComponent<RectTransform>(); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.sizeDelta = Vector2.zero; }; System.Action<Transform,string> addLbl = (p,txt) => { var g = new GameObject("L"); g.AddComponent<RectTransform>(); g.transform.SetParent(p, false); var t = g.AddComponent<UnityEngine.UI.Text>(); t.text = txt; t.font = font; t.fontSize = 10; t.color = new Color(.75f,.75f,.75f); g.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 18; }; var sTab = new GameObject("ST"); sTab.AddComponent<RectTransform>(); sTab.transform.SetParent(panel.transform, false); sTab.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing = 4; sTab.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childControlWidth = true; sTab.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childForceExpandWidth = true; sTab.AddComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1; stickerTabContent = sTab; addBtn(sTab.transform, "＋ 通り抜けOBJ追加", new Color(.25f,.45f,.7f), () => ImportCustomSticker(false)); addBtn(sTab.transform, "＋ 壁OBJ追加", new Color(.55f,.25f,.15f), () => ImportCustomSticker(true)); addLbl(sTab.transform, "▼ スティッカー一覧（クリックで配置）"); var sGO = new GameObject("Sc"); sGO.AddComponent<RectTransform>(); sGO.transform.SetParent(sTab.transform, false); sGO.AddComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1; var scroll = sGO.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; var vp = new GameObject("VP"); vp.AddComponent<RectTransform>(); vp.transform.SetParent(sGO.transform, false); vp.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,.01f); vp.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false; var vpR = vp.GetComponent<RectTransform>(); vpR.anchorMin = Vector2.zero; vpR.anchorMax = Vector2.one; vpR.sizeDelta = Vector2.zero; scroll.viewport = vpR; var con = new GameObject("Con"); con.AddComponent<RectTransform>(); con.transform.SetParent(vp.transform, false); var conR = con.GetComponent<RectTransform>(); conR.anchorMin = new Vector2(0,1); conR.anchorMax = new Vector2(1,1); conR.pivot = new Vector2(.5f,1f); conR.sizeDelta = Vector2.zero; var vl2 = con.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); vl2.spacing = 3; vl2.childControlWidth = true; vl2.childControlHeight = false; vl2.childForceExpandWidth = true; vl2.padding = new RectOffset(4,4,4,4); con.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize; scroll.content = conR; paletteContent = con.transform; var gTab = new GameObject("GT"); gTab.AddComponent<RectTransform>(); gTab.transform.SetParent(panel.transform, false); gTab.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing = 6; gTab.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childControlWidth = true; gTab.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childForceExpandWidth = true; gTab.AddComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1; gTab.SetActive(false); groundTabContent = gTab; addLbl(gTab.transform, "ドラッグで範囲指定→地面配置 / 端に白縁"); addBtn(gTab.transform, "🌿 草原", new Color(.22f,.58f,.18f), () => SetGroundPreset("Assets/Sprites/ground_grass.png")); addBtn(gTab.transform, "📂 外部画像", new Color(.3f,.5f,.3f), () => ImportGroundSprite()); addBtn(gTab.transform, "🗑 地面をすべて削除", new Color(.5f,.2f,.2f), () => { foreach(var go in FindObjectsOfType<MapEditor.GroundObject>()) Destroy(go.gameObject); }); var bTab = new GameObject("BT"); bTab.AddComponent<RectTransform>(); bTab.transform.SetParent(panel.transform, false); bTab.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing = 6; bTab.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childControlWidth = true; bTab.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childForceExpandWidth = true; bTab.AddComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1; bTab.SetActive(false); bgTabContent = bTab; addLbl(bTab.transform, "画像1枚をカメラ全体に表示"); addBtn(bTab.transform, "☁ 青空", new Color(.22f,.48f,.82f), () => SetBackgroundPreset("Assets/Sprites/bg_sky.png")); addBtn(bTab.transform, "📂 外部画像", new Color(.4f,.3f,.65f), () => ImportBackground()); addBtn(bTab.transform, "🗑 背景を削除", new Color(.45f,.2f,.2f), () => BackgroundManager.Instance?.ClearBackground()); System.Action<string,EditorTab,Color> makeTab = (txt,tab,col) => { var bg = new GameObject("TB"); bg.AddComponent<RectTransform>(); bg.transform.SetParent(tabRow.transform, false); var bi = bg.AddComponent<UnityEngine.UI.Image>(); bi.color = col; bg.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 30; var b = bg.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = bi; b.onClick.AddListener(() => SwitchTab(tab)); var lg = new GameObject("T"); lg.AddComponent<RectTransform>(); lg.transform.SetParent(bg.transform, false); var lt = lg.AddComponent<UnityEngine.UI.Text>(); lt.text = txt; lt.font = font; lt.fontSize = 11; lt.color = Color.white; lt.alignment = TextAnchor.MiddleCenter; var lr = lg.GetComponent<RectTransform>(); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.sizeDelta = Vector2.zero; }; makeTab("スティッカー", EditorTab.Sticker, new Color(.3f,.5f,.8f)); makeTab("地面", EditorTab.Ground, new Color(.3f,.6f,.3f)); makeTab("背景", EditorTab.Background, new Color(.5f,.3f,.7f)); }


    void Update()
    {
        if (Keyboard.current?.mKey.wasPressedThisFrame == true) ToggleMode();
        if (!IsEditorMode) return;
        if (Mouse.current == null) return;
        if (UnityEngine.EventSystems.EventSystem.current?.IsPointerOverGameObject() == true) return;

        if (currentTab == EditorTab.Sticker && Mouse.current.leftButton.wasPressedThisFrame && selectedDef != null)
        {
            Vector3 wp = mainCam.ScreenToWorldPoint(new Vector3(
                Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 10f));
            wp.z = 0f;
            if (Physics2D.OverlapPoint(wp) == null) PlaceSticker(wp);
        }
    }

    public void ToggleMode()
    {
        IsEditorMode = !IsEditorMode;
        if (editorPanel) editorPanel.SetActive(IsEditorMode);
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) { var pm = player.GetComponent<PopMagicPlayer>(); if (pm) pm.enabled = !IsEditorMode; }
        var gp = GetComponent<GroundPainter>();
        if (gp) gp.IsActive = IsEditorMode && currentTab == EditorTab.Ground;
        StickerItem.DeselectAll();
    }

    public void SwitchTab(EditorTab tab)
    {
        currentTab = tab;
        if (stickerTabContent) stickerTabContent.SetActive(tab == EditorTab.Sticker);
        if (groundTabContent)  groundTabContent.SetActive(tab == EditorTab.Ground);
        if (bgTabContent)      bgTabContent.SetActive(tab == EditorTab.Background);
        var gp = GetComponent<GroundPainter>();
        if (gp) gp.IsActive = IsEditorMode && tab == EditorTab.Ground;
    }

    public void SelectStickerDef(StickerDef def, Image btnImg)
    {
        selectedDef = def;
        if (selectedBtnImg != null) selectedBtnImg.color = selectedBtnImg.color; // reset handled below
        selectedBtnImg = btnImg;
        if (btnImg != null) btnImg.color = Color.yellow;
        Debug.Log($"スティッカー選択: {def?.name}");
    }

void PlaceSticker(Vector3 position) { if (selectedDef == null) return; var go = new GameObject(selectedDef.name); go.transform.position = position; go.transform.localScale = Vector3.one * selectedDef.scale; go.tag = selectedDef.tag; var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = selectedDef.sprite != null ? selectedDef.sprite : CreateCircleSprite(selectedDef.tint); sr.color = Color.white; sr.sortingOrder = 2; AddOutlineRing(go, sr.sprite); if (selectedDef.isSolid) { go.AddComponent<BoxCollider2D>(); } else { var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; } go.AddComponent<StickerItem>().stickerName = selectedDef.name; if (selectedDef.tag == "Enemy") { go.AddComponent<EnemyStats>(); go.AddComponent<EnemyAI>(); var rb = go.GetComponent<Rigidbody2D>() ?? go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.freezeRotation = true; } }

void AddOutlineRing(GameObject parent, Sprite sprite) { var outline = new GameObject("Outline"); outline.transform.SetParent(parent.transform, false); outline.transform.localScale = Vector3.one * 1.18f; var sr = outline.AddComponent<SpriteRenderer>(); sr.sprite = sprite != null ? sprite : CreateCircleSprite(Color.white); sr.color = Color.white; sr.sortingOrder = 1; }


    // 画像ファイルをインポートしてスティッカー追加
public void ImportCustomSticker(bool isSolid) { bool solid = isSolid; UnityEditor.EditorApplication.delayCall += () => { var sprite = PickFileAsSprite(); if (sprite != null) AddCustomSticker(sprite.name, sprite, solid); }; }

    // 背景画像をインポート
public void ImportBackground() { UnityEditor.EditorApplication.delayCall += () => { var sprite = PickFileAsSprite(); if (sprite != null) BackgroundManager.Instance?.SetBackground(sprite); }; } public void SetBackgroundPreset(string assetPath) { var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath); if (sp == null) return; BackgroundManager.Instance?.SetBackground(sp); Debug.Log("背景設定: " + sp.name); }

    // 地面画像インポート
public void ImportGroundSprite() { UnityEditor.EditorApplication.delayCall += () => { var sprite = PickFileAsSprite(); if (sprite != null) { var gp = GetComponent<GroundPainter>(); if (gp) gp.groundSprite = sprite; } }; } public void SetGroundPreset(string assetPath) { var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath); if (sp == null) return; var gp = GetComponent<GroundPainter>(); if (gp) gp.groundSprite = sp; Debug.Log("地面テクスチャ設定: " + sp.name); }

Sprite PickFileAsSprite() { string path = UnityEditor.EditorUtility.OpenFilePanel("画像を選択", "", "png,jpg,jpeg"); if (string.IsNullOrEmpty(path)) return null; string fileName = System.IO.Path.GetFileName(path); string destDir = "Assets/Sprites/Custom"; System.IO.Directory.CreateDirectory(destDir); string dest = destDir + "/" + fileName; System.IO.File.Copy(path, dest, true); UnityEditor.AssetDatabase.ImportAsset(dest); var imp = UnityEditor.AssetImporter.GetAtPath(dest) as UnityEditor.TextureImporter; if (imp != null) { imp.textureType = UnityEditor.TextureImporterType.Sprite; imp.filterMode = FilterMode.Point; imp.SaveAndReimport(); } return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(dest); }


public void AddCustomSticker(string name, Sprite sprite, bool isSolid) { var def = new StickerDef { name=name, sprite=sprite, tint=Color.white, isSolid=isSolid, scale=1f }; stickerDefs.Add(def); BuildPaletteUI(); }


    Sprite CreateCircleSprite(Color col)
    {
        var tex = new Texture2D(64,64,TextureFormat.RGBA32,false);
        var pixels = new Color[64*64];
        var c = new Vector2(31.5f,31.5f);
        for(int y=0;y<64;y++) for(int x=0;x<64;x++)
            pixels[y*64+x] = Vector2.Distance(new Vector2(x,y),c)<30f ? col : Color.clear;
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex,new Rect(0,0,64,64),new Vector2(0.5f,0.5f),64f);
    }

public void BuildPaletteUI() { if (paletteContent == null) return; for (int i = paletteContent.childCount - 1; i >= 0; i--) DestroyImmediate(paletteContent.GetChild(i).gameObject); var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); var categories = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<StickerDef>>(); var order = new System.Collections.Generic.List<string>(); foreach (var def in stickerDefs) { if (!categories.ContainsKey(def.category)) { categories[def.category] = new System.Collections.Generic.List<StickerDef>(); order.Add(def.category); } categories[def.category].Add(def); } foreach (var catKey in order) { var headerGO = new GameObject("H"); headerGO.AddComponent<RectTransform>(); headerGO.transform.SetParent(paletteContent, false); var ht = headerGO.AddComponent<UnityEngine.UI.Text>(); ht.text = "── " + catKey + " ──"; ht.font = font; ht.fontSize = 11; ht.color = new Color(.9f,.8f,.3f); headerGO.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 20; foreach (var def in categories[catKey]) { var d = def; var row = new GameObject(def.name); row.AddComponent<RectTransform>(); row.transform.SetParent(paletteContent, false); var bg = row.AddComponent<UnityEngine.UI.Image>(); bg.color = new Color(.12f,.14f,.2f,.9f); row.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 58; var hl = row.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>(); hl.padding = new RectOffset(4,4,4,4); hl.spacing = 8; hl.childControlHeight = true; hl.childForceExpandHeight = true; var imgCell = new GameObject("I"); imgCell.AddComponent<RectTransform>(); imgCell.transform.SetParent(row.transform, false); var img = imgCell.AddComponent<UnityEngine.UI.Image>(); img.color = def.sprite != null ? Color.white : def.tint; if (def.sprite != null) img.sprite = def.sprite; img.preserveAspect = true; var iLE = imgCell.AddComponent<UnityEngine.UI.LayoutElement>(); iLE.preferredWidth = 50; iLE.minWidth = 50; iLE.flexibleWidth = 0; var txtCell = new GameObject("T"); txtCell.AddComponent<RectTransform>(); txtCell.transform.SetParent(row.transform, false); var txt = txtCell.AddComponent<UnityEngine.UI.Text>(); txt.text = def.name + (def.isSolid ? " [壁]" : ""); txt.font = font; txt.fontSize = 12; txt.color = Color.white; txt.alignment = TextAnchor.MiddleLeft; var btn = row.AddComponent<UnityEngine.UI.Button>(); btn.targetGraphic = bg; var colors = btn.colors; colors.normalColor = new Color(.12f,.14f,.2f,.9f); colors.highlightedColor = new Color(.2f,.25f,.4f,.9f); colors.pressedColor = Color.yellow; btn.colors = colors; btn.onClick.AddListener(() => SelectStickerDef(d, bg)); } } }
}
