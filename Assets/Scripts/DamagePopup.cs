using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    static Canvas _canvas;
    static RectTransform _canvasRT;

    static void EnsureCanvas()
    {
        if (_canvas != null) return;
        var go = new GameObject("[DmgCanvas]");
        DontDestroyOnLoad(go);
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        go.AddComponent<GraphicRaycaster>().enabled = false;
        _canvasRT = go.GetComponent<RectTransform>();
    }

    public static void Show(Vector3 worldPos, float damage, Color color)
    {
        EnsureCanvas();
        var go = new GameObject("Dmg");
        go.transform.SetParent(_canvas.transform, false);
        go.AddComponent<DamagePopup>().Init(worldPos, damage, color);
    }

    Text label;
    Vector3 origin;
    float age;
    const float LIFETIME = 1.0f;

    void Init(Vector3 wp, float dmg, Color col)
    {
        origin = wp;
        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 44f);
        rt.pivot = new Vector2(0.5f, 0f);

        label = gameObject.AddComponent<Text>();
        label.text = Mathf.RoundToInt(dmg).ToString();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 34;
        label.fontStyle = FontStyle.Bold;
        label.color = col;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;

        var shadow = gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= LIFETIME) { Destroy(gameObject); return; }

        float t = age / LIFETIME;

        // ワールド座標 → スクリーン → キャンバスローカル
        var cam = Camera.main;
        if (cam == null || _canvasRT == null) return;

        Vector3 screenPt = cam.WorldToScreenPoint(origin);
        screenPt.y += Mathf.Lerp(0f, 80f, t);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRT, screenPt, null, out Vector2 lp);
        GetComponent<RectTransform>().localPosition = lp;

        // 出現スケール: 小→大→通常
        float scale = t < 0.15f ? Mathf.Lerp(0.4f, 1.2f, t / 0.15f)
                    : t < 0.3f  ? Mathf.Lerp(1.2f, 1.0f, (t - 0.15f) / 0.15f)
                    : 1.0f;
        transform.localScale = Vector3.one * scale;

        // 後半フェードアウト
        float alpha = t > 0.6f ? Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f) : 1f;
        if (label != null) { var c = label.color; c.a = alpha; label.color = c; }
    }
}
