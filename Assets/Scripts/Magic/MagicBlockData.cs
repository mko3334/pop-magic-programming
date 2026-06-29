using System.Collections.Generic;
using UnityEngine;

public enum BlockCategory { Generate, Vector, Trigger, Action, Control }

public enum BlockType
{
    // Generate
    Fire, Lightning, Water, Wood, Earth, Light,
    // Vector
    Forward, Homing, Zigzag, Rotate, Return, Bounce,
    // Trigger
    OnHit, WaitTap, Wait,
    // Action
    Explode, Attract, Split, Grow, Fortify, Stop, Slow, Speed,
    // Control
    Repeat
}

[System.Serializable]
public class MagicBlock
{
    public BlockType type;
    public string label;
    public string emoji;
    public BlockCategory category;
    public Color color;
    public Sprite icon;

    public MagicBlock(BlockType t, string emoji, string label, BlockCategory cat, Color col)
    {
        type = t; this.emoji = emoji; this.label = label; category = cat; color = col;
    }
}

public static class MagicBlockLibrary
{
    public static readonly List<MagicBlock> All = new List<MagicBlock>
    {
        // Generate
        new MagicBlock(BlockType.Fire,      "🔥", "炎",   BlockCategory.Generate, new Color(1f,0.35f,0.1f)),
        new MagicBlock(BlockType.Lightning, "⚡", "雷",   BlockCategory.Generate, new Color(1f,0.95f,0.1f)),
        new MagicBlock(BlockType.Water,     "💧", "水",   BlockCategory.Generate, new Color(0.2f,0.6f,1f)),
        new MagicBlock(BlockType.Wood,      "🌿", "木",   BlockCategory.Generate, new Color(0.2f,0.8f,0.2f)),
        new MagicBlock(BlockType.Earth,     "🪨", "土",   BlockCategory.Generate, new Color(0.6f,0.45f,0.2f)),
        new MagicBlock(BlockType.Light,     "✨", "光",   BlockCategory.Generate, new Color(1f,0.98f,0.65f)),
        // Vector
        new MagicBlock(BlockType.Forward,   "➡", "前へ",   BlockCategory.Vector,  new Color(0.4f,0.7f,1f)),
        new MagicBlock(BlockType.Homing,    "🎯", "ホーミング", BlockCategory.Vector, new Color(1f,0.4f,0.8f)),
        new MagicBlock(BlockType.Zigzag,    "〰", "ジグザグ",  BlockCategory.Vector, new Color(0.5f,1f,0.9f)),
        new MagicBlock(BlockType.Rotate,    "🌀", "回転",   BlockCategory.Vector,  new Color(0.6f,0.8f,1f)),
        new MagicBlock(BlockType.Return,    "↩", "戻る",   BlockCategory.Vector,  new Color(0.9f,0.6f,1f)),
        new MagicBlock(BlockType.Bounce,    "🏐", "跳ねる", BlockCategory.Vector,  new Color(0.8f,1f,0.4f)),
        // Trigger
        new MagicBlock(BlockType.OnHit,     "👆", "触れたら", BlockCategory.Trigger, new Color(1f,0.7f,0.3f)),
        new MagicBlock(BlockType.WaitTap,   "👇", "タップ待ち", BlockCategory.Trigger, new Color(1f,0.85f,0.5f)),
        new MagicBlock(BlockType.Wait,      "⏱", "待機",   BlockCategory.Trigger, new Color(0.8f,0.8f,0.8f)),
        // Action
        new MagicBlock(BlockType.Explode,   "💥", "爆発",   BlockCategory.Action,  new Color(1f,0.3f,0.3f)),
        new MagicBlock(BlockType.Attract,   "🧲", "吸引",   BlockCategory.Action,  new Color(0.7f,0.3f,1f)),
        new MagicBlock(BlockType.Split,     "🔱", "分裂",   BlockCategory.Action,  new Color(0.3f,1f,0.8f)),
        new MagicBlock(BlockType.Grow,      "🔍", "成長",   BlockCategory.Action,  new Color(1f,0.75f,0.3f)),
        new MagicBlock(BlockType.Fortify,   "🛡", "強化",   BlockCategory.Action,  new Color(0.9f,0.85f,0.4f)),
        new MagicBlock(BlockType.Stop,      "⛔", "停止",   BlockCategory.Action,  new Color(0.9f,0.3f,0.4f)),
        new MagicBlock(BlockType.Slow,      "🐢", "スロー", BlockCategory.Action,  new Color(0.5f,0.9f,0.5f)),
        new MagicBlock(BlockType.Speed,     "🐇", "加速",   BlockCategory.Action,  new Color(1f,0.9f,0.2f)),
        // Control
        new MagicBlock(BlockType.Repeat,    "🔁", "くり返し", BlockCategory.Control, new Color(0.9f,0.5f,0.9f)),
    };

    public static MagicBlock Get(BlockType t) => All.Find(b => b.type == t);
}

[System.Serializable]
public class SpellProgram
{
    public string slotKey; // "Z", "X", "C"
    public List<BlockType> blocks = new List<BlockType>();
}
