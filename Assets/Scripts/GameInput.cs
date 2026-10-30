using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Ескі (Input Manager) және жаңа (Input System) енгізу жүйесінің екеуімен де істейді.
// Сондықтан Unity 6-да "Active Input Handling" баптауын өзгертпесеңіз де қате шықпайды.
public static class GameInput
{
    public static float Horizontal()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxis("Horizontal");
#elif ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return 0f;
        float x = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1f;
        return x;
#else
        return 0f;
#endif
    }

    public static bool JumpPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Space);
#elif ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return false;
#endif
    }

    public static bool EscapePressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#elif ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return false;
#endif
    }
}
