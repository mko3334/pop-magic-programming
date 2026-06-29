using UnityEngine;
using UnityEngine.UI;

public class LevelUpEffect : MonoBehaviour
{
    static Canvas _canvas;

    static void EnsureCanvas()
    {
        if (_canvas != null) return;
        var go = new GameObject("[LvUpCanvas]");
        DontDestroyOnLoad(go);
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 190;
        go.AddComponent<GraphicRaycaster>().enabled = false;
    }

    public static void Show(int level)
    {
        EnsureCanvas();
        var go = new GameObject("LvUp", typeof(RectTransform));
        go.transform.SetParent(_canvas.transform, false);
        go.AddComponent<LevelUpEffect>().Init(level);
    }

    Text label;
    float age;
    const float LIFETIME = 2.5f;

    void Init(int lv)
    {
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(520f, 120f);
        rt.anchoredPosition = new Vector2(0f, 80f);

        label = gameObject.AddComponent<Text>();
        label.text = $"LEVEL UP!  Lv.{lv}";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 52;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(1f, 0.95f, 0.2f);
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;

        var shadow = gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.4f, 0.2f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= LIFETIME) { Destroy(gameObject); return; }

        float t = age / LIFETIME;

        // ポップイン → ホールド → フェードアウト
        float scale = t < 0.1f ? Mathf.Lerp(0f, 1.25f, t / 0.1f)
                    : t < 0.2f ? Mathf.Lerp(1.25f, 1.0f, (t - 0.1f) / 0.1f)
                    : 1.0f;
        transform.localScale = Vector3.one * scale;

        var rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, 80f + t * 50f);

        float alpha = t > 0.7f ? Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f) : 1f;
        if (label != null) { var c = label.color; c.a = alpha; label.color = c; }
    }
}
