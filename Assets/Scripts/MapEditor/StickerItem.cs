using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StickerItem : MonoBehaviour
{
    public string stickerName;
    public bool isSelected;

    private static StickerItem selectedSticker;
    private Camera mainCam;
    private Vector3 dragOffset;
    private bool dragging;
    private SpriteRenderer sr;

    void Start()
    {
        mainCam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!MapEditorManager.IsEditorMode) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(
            mouse.position.ReadValue().x,
            mouse.position.ReadValue().y, 10f));
        worldPos.z = 0f;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            // UI上のクリックは無視
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.gameObject == gameObject)
            {
                Select();
                dragOffset = transform.position - worldPos;
                dragging = true;
            }
        }

        if (dragging && mouse.leftButton.isPressed)
            transform.position = worldPos + dragOffset;

        if (mouse.leftButton.wasReleasedThisFrame)
            dragging = false;

        if (isSelected && (Keyboard.current?.deleteKey.wasPressedThisFrame == true
                        || Keyboard.current?.backspaceKey.wasPressedThisFrame == true))
            Destroy(gameObject);
    }

    void Select()
    {
        if (selectedSticker != null) selectedSticker.Deselect();
        selectedSticker = this;
        isSelected = true;
        if (sr) sr.color = new Color(1f, 1f, 0.6f, 1f);
    }

    void Deselect()
    {
        isSelected = false;
        if (sr) sr.color = Color.white;
    }

    public static void DeselectAll()
    {
        if (selectedSticker != null) selectedSticker.Deselect();
        selectedSticker = null;
    }
}
