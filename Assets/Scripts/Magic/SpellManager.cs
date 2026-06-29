using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance { get; private set; }

    public List<SpellProgram> spells = new List<SpellProgram>
    {
        new SpellProgram { slotKey = "Z" },
        new SpellProgram { slotKey = "X" },
        new SpellProgram { slotKey = "C" },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("[SpellManager]");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<SpellManager>();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public SpellProgram GetSpell(string key) => spells.Find(s => s.slotKey == key);

    public void Save()
    {
        foreach (var spell in spells)
        {
            var joined = string.Join(",", spell.blocks.ConvertAll(b => ((int)b).ToString()));
            PlayerPrefs.SetString("Spell_" + spell.slotKey, joined);
        }
        PlayerPrefs.Save();
    }

    public void Load()
    {
        foreach (var spell in spells)
        {
            var raw = PlayerPrefs.GetString("Spell_" + spell.slotKey, "");
            spell.blocks.Clear();
            if (string.IsNullOrEmpty(raw)) continue;
            foreach (var s in raw.Split(','))
                if (int.TryParse(s, out int v))
                    spell.blocks.Add((BlockType)v);
        }
    }
}
