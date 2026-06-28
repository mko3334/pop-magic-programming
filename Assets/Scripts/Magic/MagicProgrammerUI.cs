using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MagicProgrammerUI : MonoBehaviour
{
    public static MagicProgrammerUI Instance { get; private set; }

    public GameObject panelRoot;
    public Transform blockPalette;
    public Transform[] slotRows;

    private string activeSlot = "Z";
    private bool isOpen = false;

    void Awake() { Instance = this; }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            Toggle();
    }

public void Toggle() { isOpen = !isOpen; panelRoot.SetActive(isOpen); if (isOpen) { EnsureSlicerButton(); BuildPalette(); RefreshSlotUI(); } } void EnsureSlicerButton() { if (panelRoot.transform.Find("SlicerBtn") != null) return; var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); var go = new GameObject("SlicerBtn"); go.AddComponent<RectTransform>(); go.transform.SetParent(panelRoot.transform, false); go.transform.SetSiblingIndex(0); var img = go.AddComponent<UnityEngine.UI.Image>(); img.color = new Color(0.2f,0.4f,0.6f); go.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 28; var btn = go.AddComponent<UnityEngine.UI.Button>(); btn.targetGraphic = img; btn.onClick.AddListener(() => { var slicer = FindObjectOfType<RuntimeSpriteSlicer>(); if (slicer != null) slicer.OpenForAsset("Assets/Sprites/magic_icons2.png"); }); var tGO = new GameObject("T"); tGO.AddComponent<RectTransform>(); tGO.transform.SetParent(go.transform, false); var t = tGO.AddComponent<UnityEngine.UI.Text>(); t.text = "🔪 アイコンをスライス"; t.font = font; t.fontSize = 12; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter; var tr = tGO.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero; }

    public void SelectSlot(string key)
    {
        activeSlot = key;
        RefreshSlotUI();
    }

    public void AddBlock(int blockTypeInt)
    {
        AddBlockToSlot(blockTypeInt, activeSlot);
    }

    public void AddBlockToSlot(int blockTypeInt, string key)
    {
        var spell = SpellManager.Instance.GetSpell(key);
        if (spell.blocks.Count >= 8) return;
        spell.blocks.Add((BlockType)blockTypeInt);
        SpellManager.Instance.Save();
        RefreshSlotUI();
    }

    public void RemoveBlock(int index, string key)
    {
        var spell = SpellManager.Instance.GetSpell(key);
        if (index >= 0 && index < spell.blocks.Count)
            spell.blocks.RemoveAt(index);
        SpellManager.Instance.Save();
        RefreshSlotUI();
    }

    public void ClearSlot()
    {
        SpellManager.Instance.GetSpell(activeSlot).blocks.Clear();
        SpellManager.Instance.Save();
        RefreshSlotUI();
    }

    void RefreshSlotUI()
    {
        if (slotRows == null || slotRows.Length < 3) return;
        string[] keys = { "Z", "X", "C" };
        for (int i = 0; i < 3; i++)
        {
            var row = slotRows[i];
            // 既存ブロックUIを削除
            foreach (Transform child in row)
                Destroy(child.gameObject);

            var spell = SpellManager.Instance.GetSpell(keys[i]);
            string slotKey = keys[i];

            // ドロップゾーン設定
            var dz = row.GetComponent<SlotDropZone>() ?? row.gameObject.AddComponent<SlotDropZone>();
            dz.slotKey = slotKey;
            var dzImg = row.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
            dzImg.color = new Color(0.15f, 0.15f, 0.25f, 0.5f);

            for (int j = 0; j < spell.blocks.Count; j++)
            {
                var block = MagicBlockLibrary.Get(spell.blocks[j]);
                int captureJ = j;
                string captureKey = slotKey;
                var bgo = CreateBlockButton(row, block);
                var db = bgo.AddComponent<DraggableBlock>();
                db.blockType = block.type;
                db.isInSlot = true;
                db.slotIndex = captureJ;
                db.slotKey = captureKey;
            }
        }
    }

public static GameObject CreateBlockButton(Transform parent, MagicBlock block) { var go = new GameObject(block.type.ToString()); go.AddComponent<RectTransform>(); go.transform.SetParent(parent, false); var le = go.AddComponent<LayoutElement>(); le.preferredWidth = 68; le.preferredHeight = 68; le.minWidth = 68; le.minHeight = 68; var bg = go.AddComponent<Image>(); bg.color = block.color; if (block.icon != null) { var iconGO = new GameObject("Icon"); iconGO.AddComponent<RectTransform>(); iconGO.transform.SetParent(go.transform, false); var img = iconGO.AddComponent<Image>(); img.sprite = block.icon; img.preserveAspect = true; img.color = Color.white; img.raycastTarget = false; var ir = iconGO.GetComponent<RectTransform>(); ir.anchorMin = new Vector2(0.05f,0.05f); ir.anchorMax = new Vector2(0.95f,0.95f); ir.sizeDelta = Vector2.zero; } else { var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); var tGO = new GameObject("L"); tGO.AddComponent<RectTransform>(); tGO.transform.SetParent(go.transform, false); var t = tGO.AddComponent<Text>(); t.text = block.emoji + "\n" + block.label; t.alignment = TextAnchor.MiddleCenter; t.fontSize = 12; t.color = Color.white; t.font = font; t.raycastTarget = false; var tr = tGO.GetComponent<RectTransform>(); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero; } return go; }

public void BuildPalette() { if (blockPalette == null) return; foreach (Transform child in blockPalette) Destroy(child.gameObject); var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); foreach (var block in MagicBlockLibrary.All) { var b = block; var bgo = CreateBlockButton(blockPalette, b); var db = bgo.AddComponent<DraggableBlock>(); db.blockType = b.type; db.isInSlot = false; if (b.icon != null) { var fileLabel = new GameObject("FileLabel"); fileLabel.AddComponent<RectTransform>(); fileLabel.transform.SetParent(bgo.transform, false); var ft = fileLabel.AddComponent<Text>(); string path = UnityEditor.AssetDatabase.GetAssetPath(b.icon); string fname = System.IO.Path.GetFileName(path); ft.text = fname; ft.font = font; ft.fontSize = 7; ft.color = new Color(1,1,1,0.6f); ft.alignment = TextAnchor.LowerCenter; ft.raycastTarget = false; var fr = fileLabel.GetComponent<RectTransform>(); fr.anchorMin = new Vector2(0,0); fr.anchorMax = new Vector2(1,0.25f); fr.sizeDelta = Vector2.zero; } } }
}
