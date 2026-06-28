using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotDropZone : MonoBehaviour, IDropHandler
{
    public string slotKey;

    public void OnDrop(PointerEventData e)
    {
        var draggable = e.pointerDrag?.GetComponent<DraggableBlock>();
        if (draggable == null) return;
        if (draggable.isInSlot)
            MagicProgrammerUI.Instance.RemoveBlock(draggable.slotIndex, draggable.slotKey);
        MagicProgrammerUI.Instance.AddBlockToSlot((int)draggable.blockType, slotKey);
    }
}
