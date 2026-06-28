using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// OnDrop は SlotDropZone 側だけで処理する。
// DraggableBlock は ゴーストの表示管理のみ担当。
public class DraggableBlock : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public BlockType blockType;
    public bool isInSlot;
    public int slotIndex;
    public string slotKey;

    private static GameObject ghost;
    private Canvas rootCanvas;
    private CanvasGroup cg;
    private bool wasDragged;

    void Start()
    {
        // ソート順が一番高いrootCanvasを探す
        int maxOrder = int.MinValue;
        foreach (var c in FindObjectsOfType<Canvas>())
            if (c.isRootCanvas && c.sortingOrder > maxOrder)
            { rootCanvas = c; maxOrder = c.sortingOrder; }

        cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (wasDragged) { wasDragged = false; return; } // ドラッグ後のクリックは無視

        if (isInSlot)
            MagicProgrammerUI.Instance.RemoveBlock(slotIndex, slotKey);
        else
            MagicProgrammerUI.Instance.AddBlock((int)blockType);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        wasDragged = true;
        if (ghost != null) Destroy(ghost);

        ghost = new GameObject("DragGhost");
        ghost.transform.SetParent(rootCanvas.transform, false);
        ghost.transform.SetAsLastSibling();

        ghost.AddComponent<RectTransform>().sizeDelta = new Vector2(72, 72);
        var img = ghost.AddComponent<Image>();
        img.color = GetComponent<Image>()?.color ?? Color.white;
        img.raycastTarget = false;

        var lGO = new GameObject("T");
        lGO.transform.SetParent(ghost.transform, false);
        var txt = lGO.AddComponent<Text>();
        txt.text = GetComponentInChildren<Text>()?.text ?? "";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 13; txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter; txt.raycastTarget = false;
        var tr = lGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;

        if (cg != null) { cg.alpha = 0.35f; cg.blocksRaycasts = false; }
    }

    public void OnDrag(PointerEventData e)
    {
        if (ghost == null) return;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(), e.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out Vector3 wp);
        ghost.transform.position = wp;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }
        if (ghost != null) { Destroy(ghost); ghost = null; }
        // ドロップ先への追加は SlotDropZone.OnDrop が行う
    }
}
