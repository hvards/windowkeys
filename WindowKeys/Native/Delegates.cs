namespace WindowKeys.Native;

public delegate nint LowLevelKeyboardProc(int nCode, nint wParam, ref KeyboardInput lParam);
