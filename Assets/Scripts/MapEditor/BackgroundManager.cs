using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance { get; private set; }
    private GameObject bgObject;

    void Awake() { Instance = this; }

    public void SetBackground(Sprite sprite)
    {
        if (bgObject != null) Destroy(bgObject);
        if (sprite == null) return;

        bgObject = new GameObject("Background");
        var sr = bgObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -10;

        // カメラ範囲に合わせてスケール
        var cam = Camera.main;
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        bgObject.transform.position = cam.transform.position + Vector3.forward * 10f;
        bgObject.transform.localScale = new Vector3(
            width / sprite.bounds.size.x,
            height / sprite.bounds.size.y, 1f);
    }

    public void ClearBackground()
    {
        if (bgObject != null) Destroy(bgObject);
        bgObject = null;
    }
}
