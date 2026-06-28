using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpritePicker : MonoBehaviour
{
    public static SpritePicker Instance { get; private set; }

    public enum PickMode { Sticker_Passable, Sticker_Solid, Ground, Background }

    private GameObject panel;
    private Transform grid;
    private PickMode currentMode;

    void Awake() { Instance = this; }

    public void Open(PickMode mode)
    {
        currentMode = mode;
        if (panel == null) Build();
        Populate();
        panel.SetActive(true);
    }

    void Build()
    {
        var canvas = GameObject.Find("MapEditor_Canvas");
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        panel = new GameObject("SpritePicker");
        panel.AddComponent<RectTransform>();
        panel.transform.SetParent(canvas.transform, false);
        panel.transform.SetAsLastSibling();

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.12f, 0.98f);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.2f, 0.05f);
        pr.anchorMax = new Vector2(0.8f, 0.95f);
        pr.offsetMin = pr.offsetMax = Vector2.zero;

        var vl = panel.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(10,10,10,10); vl.spacing = 8;
        vl.childControlWidth = true; vl.childControlHeight = false; vl.childForceExpandWidth = true;

        // ヘッダー
        var hdr = new GameObject("H"); hdr.AddComponent<RectTransform>(); hdr.transform.SetParent(panel.transform, false);
        var hh = hdr.AddComponent<HorizontalLayoutGroup>(); hh.childForceExpandWidth = true; hh.childControlHeight = false;
        hdr.AddComponent<LayoutElement>().preferredHeight = 36;

        var titleGO = new GameObject("T"); titleGO.AddComponent<RectTransform>(); titleGO.transform.SetParent(hdr.transform, false);
        var tt = titleGO.AddComponent<Text>(); tt.text = "📁 画像を選択（Assets/Sprites 内）"; tt.font = font; tt.fontSize = 14; tt.color = Color.white;

        var closeGO = new GameObject("X"); closeGO.AddComponent<RectTransform>(); closeGO.transform.SetParent(hdr.transform, false);
        var ci = closeGO.AddComponent<Image>(); ci.color = new Color(0.7f, 0.15f, 0.15f);
        closeGO.AddComponent<LayoutElement>().preferredWidth = 80;
        var cb = closeGO.AddComponent<Button>(); cb.targetGraphic = ci;
        cb.onClick.AddListener(() => panel.SetActive(false));
        var clt = new GameObject("T"); clt.AddComponent<RectTransform>(); clt.transform.SetParent(closeGO.transform, false);
        var ctt = clt.AddComponent<Text>(); ctt.text = "✕ 閉じる"; ctt.font = font; ctt.fontSize = 12; ctt.color = Color.white; ctt.alignment = TextAnchor.MiddleCenter;
        var ctr = clt.GetComponent<RectTransform>(); ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one; ctr.sizeDelta = Vector2.zero;

        // スクロール
        var sGO = new GameObject("S"); sGO.AddComponent<RectTransform>(); sGO.transform.SetParent(panel.transform, false);
        sGO.AddComponent<LayoutElement>().flexibleHeight = 1;
        var scroll = sGO.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true;

        var vp = new GameObject("VP"); vp.AddComponent<RectTransform>(); vp.transform.SetParent(sGO.transform, false);
        vp.AddComponent<Image>().color = new Color(0,0,0,0.01f);
        vp.AddComponent<Mask>().showMaskGraphic = false;
        var vpR = vp.GetComponent<RectTransform>(); vpR.anchorMin = Vector2.zero; vpR.anchorMax = Vector2.one; vpR.sizeDelta = Vector2.zero;
        scroll.viewport = vpR;

        var con = new GameObject("Con"); con.AddComponent<RectTransform>(); con.transform.SetParent(vp.transform, false);
        var conR = con.GetComponent<RectTransform>();
        conR.anchorMin = new Vector2(0,1); conR.anchorMax = new Vector2(1,1);
        conR.pivot = new Vector2(0.5f,1f); conR.sizeDelta = Vector2.zero;
        var g = con.AddComponent<GridLayoutGroup>();
        g.cellSize = new Vector2(96,96); g.spacing = new Vector2(6,6);
        g.padding = new RectOffset(6,6,6,6);
        con.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = conR;
        grid = con.transform;
    }

    void Populate()
    {
        if (grid == null) return;
        for (int i = grid.childCount-1; i >= 0; i--) Destroy(grid.GetChild(i).gameObject);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

#if UNITY_EDITOR
        var guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[]{"Assets/Sprites"});
        foreach (var guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;
            string nm = System.IO.Path.GetFileNameWithoutExtension(path);

            var go = new GameObject(nm); go.AddComponent<RectTransform>(); go.transform.SetParent(grid, false);
            var bg = go.AddComponent<Image>(); bg.color = new Color(0.15f,0.15f,0.2f);

            // スプライト表示
            var imgGO = new GameObject("I"); imgGO.AddComponent<RectTransform>(); imgGO.transform.SetParent(go.transform, false);
            var img = imgGO.AddComponent<Image>(); img.sprite = sprite; img.preserveAspect = true;
            var ir = imgGO.GetComponent<RectTransform>(); ir.anchorMin = new Vector2(0,0.2f); ir.anchorMax = Vector2.one; ir.sizeDelta = Vector2.zero;

            var lbl = new GameObject("L"); lbl.AddComponent<RectTransform>(); lbl.transform.SetParent(go.transform, false);
            var t = lbl.AddComponent<Text>(); t.text = nm; t.font = font; t.fontSize = 9; t.color = Color.white; t.alignment = TextAnchor.LowerCenter;
            var tr = lbl.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = new Vector2(1,0.25f); tr.sizeDelta = Vector2.zero;

            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            var s = sprite; var n = nm;
            btn.onClick.AddListener(() => OnPick(s, n));
        }
#endif
    }

    void OnPick(Sprite sprite, string name)
    {
        var mgr = MapEditorManager.Instance;
        switch (currentMode)
        {
            case PickMode.Sticker_Passable: mgr.AddCustomSticker(name, sprite, false); break;
            case PickMode.Sticker_Solid:    mgr.AddCustomSticker(name, sprite, true);  break;
            case PickMode.Ground:
                var gp = mgr.GetComponent<GroundPainter>();
                if (gp) gp.groundSprite = sprite;
                break;
            case PickMode.Background:
                BackgroundManager.Instance?.SetBackground(sprite);
                break;
        }
        panel.SetActive(false);
    }
}
