using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameMenuDrawer : MonoBehaviour
{
    public static GameMenuDrawer Instance { get; private set; }

    Canvas canvas;
    GameObject overlayGO;
    GameObject drawerPanelGO;
    bool isOpen;

    void Awake()
    {
        Instance = this;
        BuildUI();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle() { if (isOpen) Close(); else Open(); }

    public void Open()
    {
        isOpen = true;
        overlayGO.SetActive(true);
        drawerPanelGO.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        overlayGO.SetActive(false);
        drawerPanelGO.SetActive(false);
        MagicProgrammerUI.Instance?.Close();
    }

    void BuildUI()
    {
        canvas = gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        if (gameObject.GetComponent<CanvasScaler>() == null)
        {
            var cs = gameObject.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
            cs.matchWidthOrHeight = 0.5f;
        }
        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // 半透明オーバーレイ
        overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(transform, false);
        var overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;
        var overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.45f);
        var overlayBtn = overlayGO.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImg;
        overlayBtn.onClick.AddListener(Close);

        // 右スライドパネル
        drawerPanelGO = new GameObject("DrawerPanel");
        drawerPanelGO.transform.SetParent(transform, false);
        var drawerRT = drawerPanelGO.AddComponent<RectTransform>();
        drawerRT.anchorMin = new Vector2(1f, 0f);
        drawerRT.anchorMax = new Vector2(1f, 1f);
        drawerRT.pivot = new Vector2(1f, 0.5f);
        drawerRT.sizeDelta = new Vector2(320f, 0f);
        drawerRT.anchoredPosition = Vector2.zero;
        var drawerBg = drawerPanelGO.AddComponent<Image>();
        drawerBg.color = new Color(0.96f, 0.93f, 0.87f);

        var vl = drawerPanelGO.AddComponent<VerticalLayoutGroup>();
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = true;
        vl.spacing = 16f;
        vl.padding = new RectOffset(24, 24, 40, 40);

        MakeTab("魔導書", new Color(1f, 0.71f, 0.71f),   () => { drawerPanelGO.SetActive(false); MagicProgrammerUI.Instance?.Open(); });
        MakeTab("アイテム", new Color(0.68f, 0.94f, 0.68f), () => { });
        MakeTab("図鑑",   new Color(0.68f, 0.88f, 1f),   () => { });
        MakeTab("設定",   new Color(1f, 0.84f, 0.68f),   () => { });

        overlayGO.SetActive(false);
        drawerPanelGO.SetActive(false);
    }

    void MakeTab(string label, Color color, Action onClick)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject(label);
        go.transform.SetParent(drawerPanelGO.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = new Color(color.r * 0.9f, color.g * 0.9f, color.b * 0.9f);
        btn.colors = cb;
        btn.onClick.AddListener(() => onClick());

        var tGO = new GameObject("T");
        tGO.transform.SetParent(go.transform, false);
        var t = tGO.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 40;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.15f, 0.08f, 0.08f);
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var tr = tGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;
    }
}
