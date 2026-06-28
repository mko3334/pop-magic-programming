using UnityEngine;

public class MagicIconLoader : MonoBehaviour
{
    [System.Serializable]
    public class IconEntry
    {
        public BlockType blockType;
        public Sprite icon;
    }

    public IconEntry[] entries;

    void Awake()
    {
        if (entries == null) return;
        foreach (var entry in entries)
        {
            var block = MagicBlockLibrary.Get(entry.blockType);
            if (block != null && entry.icon != null)
                block.icon = entry.icon;
        }
    }
}
