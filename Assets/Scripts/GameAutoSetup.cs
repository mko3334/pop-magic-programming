using UnityEngine;
using UnityEngine.EventSystems;

// シーンに何も置かなくてもUIが全部自動生成される
public static class GameAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Setup()
    {
        EnsureEventSystem();
        Ensure<GameMenuDrawer>("[GameMenuDrawer]");
        Ensure<MagicProgrammerUI>("[MagicProgrammerUI]");
        Ensure<SettingsUI>("[SettingsUI]");
        Ensure<CharacterEditorUI>("[CharacterEditorUI]");
        Ensure<HUDManager>("[HUDManager]");
    }

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("[EventSystem]");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    static void Ensure<T>(string name) where T : MonoBehaviour
    {
        if (Object.FindObjectOfType<T>() != null) return;
        new GameObject(name).AddComponent<T>();
    }
}
