using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class SceneSetupGuide : EditorWindow
{
    static SceneSetupGuide()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool("PopMagic_SetupDone", false))
                ShowWindow();
        };
    }

    [MenuItem("PopMagic/シーン設定ガイド を開く")]
    public static void ShowWindow()
    {
        var w = GetWindow<SceneSetupGuide>("PopMagic シーン設定ガイド");
        w.minSize = new Vector2(500, 520);
        w.Show();
    }

    Vector2 scroll;

    static readonly GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
    {
        fontSize = 15,
        normal = { textColor = new Color(0.2f, 0.5f, 1f) }
    };

    static readonly GUIStyle stepStyle = new GUIStyle(EditorStyles.label)
    {
        wordWrap = true,
        fontSize = 12
    };

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
        EditorGUILayout.Space(12);

        GUILayout.Label("Pop Magic Programming — シーン設定ガイド", EditorStyles.largeLabel);
        EditorGUILayout.HelpBox("プロジェクトをPCで開いたら以下の設定を行ってください。完了したら一番下のボタンを押すと次回から表示されなくなります。", MessageType.Info);
        EditorGUILayout.Space(8);

        // ── 1. CharacterStyleManager ──
        DrawSection("① CharacterStyleManager をシーンに追加",
            "Hierarchy で空の GameObject を作成し\n" +
            "「CharacterStyleManager」と名前を付けて\n" +
            "Add Component → CharacterStyleManager をアタッチ。\n\n" +
            "Inspector の【Available Sprites】に\n" +
            "使いたい画像（スライム・宝箱など）をドラッグして登録。\n" +
            "（magic_iconフォルダのアイコンはコードで自動登録されます）");

        // ── 2. CharacterEditorUI ──
        DrawSection("② CharacterEditorUI をシーンに追加",
            "同様に空の GameObject を「CharacterEditorUI」と名付けて\n" +
            "Add Component → CharacterEditorUI をアタッチ。\n" +
            "（設定画面の「キャラクター」ボタンから開けるようになります）");

        // ── 3. HUDManager ──
        DrawSection("③ HUDManager の参照を設定",
            "シーン上の HUDManager オブジェクトを選択し\n" +
            "Inspector で以下を設定：\n" +
            "  • Player Stats → プレイヤーオブジェクトをドラッグ\n" +
            "  • Heart Sprite → ハートのSprite\n" +
            "  • Moon Sprite  → 月のSprite\n" +
            "  • Potion Sprite → ポーションのSprite（任意）");

        // ── 4. PopMagicPlayer ──
        DrawSection("④ PopMagicPlayer の Projectile Prefab を確認",
            "Player オブジェクトの PopMagicPlayer コンポーネントで\n" +
            "【Projectile Prefab】に魔法弾のPrefabが設定されているか確認。");

        // ── 5. SpriteAnimator ──
        DrawSection("⑤ SpriteAnimator をキャラに追加（アニメしたい場合）",
            "プレイヤーや敵の GameObject に\n" +
            "Add Component → SpriteAnimator をアタッチ。\n" +
            "（CharacterStyleManager から自動的にアニメが適用されます）");

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox("これらの設定が完了するとゲームが正常に起動します。", MessageType.None);
        EditorGUILayout.Space(4);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("✅  設定完了 — 次回から表示しない", GUILayout.Height(38)))
        {
            EditorPrefs.SetBool("PopMagic_SetupDone", true);
            Close();
        }

        if (GUILayout.Button("後で確認", GUILayout.Width(100), GUILayout.Height(38)))
            Close();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(6);
    }

    void DrawSection(string title, string body)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label(title, EditorStyles.boldLabel);
        EditorGUILayout.Space(2);
        GUILayout.Label(body, stepStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6);
    }
}
