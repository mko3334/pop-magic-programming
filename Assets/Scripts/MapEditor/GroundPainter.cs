using UnityEngine;
using UnityEngine.InputSystem;

public class GroundPainter : MonoBehaviour
{
    public static GroundPainter Instance { get; private set; }
    public bool IsActive { get; set; }
    public Sprite groundSprite;

    private Camera mainCam;
    private bool dragging;
    private Vector3 dragStart;
    private GameObject preview;

    void Awake() { Instance = this; mainCam = Camera.main; }

    void Update()
    {
        if (!MapEditorManager.IsEditorMode || !IsActive) return;
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (UnityEngine.EventSystems.EventSystem.current?.IsPointerOverGameObject() == true) return;

        Vector3 wp = mainCam.ScreenToWorldPoint(new Vector3(mouse.position.ReadValue().x, mouse.position.ReadValue().y, 10f));
        wp.z = 0f;

        if (mouse.leftButton.wasPressedThisFrame) { dragging = true; dragStart = wp; CreatePreview(wp); }
        if (dragging && mouse.leftButton.isPressed) UpdatePreview(dragStart, wp);
        if (dragging && mouse.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
            if (preview != null) Destroy(preview);
            PlaceGround(dragStart, wp);
        }
    }

    void CreatePreview(Vector3 pos)
    {
        preview = new GameObject("GroundPreview");
        var sr = preview.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.5f, 0.8f, 0.3f, 0.4f);
        sr.sprite = groundSprite != null ? groundSprite : CreateDefaultSprite(Color.green);
        sr.sortingOrder = -1;
    }

    void UpdatePreview(Vector3 start, Vector3 end)
    {
        if (preview == null) return;
        Vector3 center = (start + end) / 2f;
        preview.transform.position = center;
        float w = Mathf.Abs(end.x - start.x);
        float h = Mathf.Abs(end.y - start.y);
        preview.transform.localScale = new Vector3(Mathf.Max(w, 0.1f), Mathf.Max(h, 0.1f), 1f);
    }

    void PlaceGround(Vector3 start, Vector3 end)
    {
        float w = Mathf.Abs(end.x - start.x);
        float h = Mathf.Abs(end.y - start.y);
        if (w < 0.2f || h < 0.2f) return;

        Vector3 center = (start + end) / 2f;

        var go = new GameObject("Ground");
        go.transform.position = center;
        go.tag = "Untagged";

        // 地面本体
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = groundSprite != null ? groundSprite : CreateDefaultSprite(new Color(0.35f, 0.62f, 0.25f));
        sr.color = Color.white;
        sr.sortingOrder = -2;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(w, h);
        go.transform.localScale = Vector3.one;

        // 白縁（4辺）
        float bw = 0.12f;
        Color borderCol = Color.white;
        string[] dirs = {"Top","Bottom","Left","Right"};
        Vector3[] positions = {
            new Vector3(0, h/2f + bw/2f, 0),
            new Vector3(0, -h/2f - bw/2f, 0),
            new Vector3(-w/2f - bw/2f, 0, 0),
            new Vector3(w/2f + bw/2f, 0, 0)
        };
        Vector2[] sizes = {
            new Vector2(w + bw*2f, bw),
            new Vector2(w + bw*2f, bw),
            new Vector2(bw, h),
            new Vector2(bw, h)
        };
        for (int i = 0; i < 4; i++)
        {
            var border = new GameObject("Border_" + dirs[i]);
            border.transform.SetParent(go.transform, false);
            border.transform.localPosition = positions[i];
            var bsr = border.AddComponent<SpriteRenderer>();
            bsr.sprite = CreateDefaultSprite(borderCol);
            bsr.color = borderCol;
            bsr.sortingOrder = -1;
            bsr.drawMode = SpriteDrawMode.Tiled;
            bsr.size = sizes[i];
        }

        go.AddComponent<MapEditor.GroundObject>();
    }

    Sprite CreateDefaultSprite(Color col)
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = col;
        tex.SetPixels(pixels); tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0,0,4,4), new Vector2(0.5f,0.5f), 4f);
    }
}

namespace MapEditor { public class GroundObject : MonoBehaviour {} }
