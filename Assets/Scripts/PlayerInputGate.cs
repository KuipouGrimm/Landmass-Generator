using System;

public static class PlayerInputGate
{
    static bool uiOpen = true;

    public static bool IsUIOpen => uiOpen;
    public static bool CameraControlsEnabled => !uiOpen;

    public static event Action<bool> OnUIOpenChanged;

    public static void SetUIOpen(bool open)
    {
        if (uiOpen == open) {
            return;
        }

        uiOpen = open;
        OnUIOpenChanged?.Invoke(open);
    }
}
