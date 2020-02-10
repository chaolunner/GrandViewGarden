using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;
using UnityEngine;
using System;

public class VirtualKeyboard
{
    [DllImport("user32")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32", EntryPoint = "FindWindow")]
    static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

    [DllImport("user32")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    private static Process OnScreenKeyboardProcess = null;

    private static int GWL_STYLE = -16; // Window styles
    private static int WS_BORDER = 0x00800000; // The window has a thin-line border
    private static int WS_DLGFRAME = 0x00400000; // The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar
    private static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; // 0x00C00000 - The window has a title bar (includes the WS_BORDER style)
    private static int WS_THICKFRAME = 0x00040000; // The window has a sizing border. Same as the WS_SIZEBOX style

    // About SetWindowPos uFlags: The window sizing and positioning flags. This parameter can be a combination of the following values
    //private static int SWP_NOSIZE = 0x0001; // Retains the current size (ignores the cx and cy parameters)
    //private static int SWP_NOMOVE = 0x0002; // Retains the current position(ignores X and Y parameters)
    private static int SWP_FRAMECHANGED = 0x0020; // Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed

    public void ShowOnScreenKeyboard()
    {
        if (OnScreenKeyboardProcess == null || OnScreenKeyboardProcess.HasExited)
        {
            OnScreenKeyboardProcess = ExternalCall("Osk", null);
        }
    }

    public void HideOnScreenKeyboard()
    {
        if (OnScreenKeyboardProcess != null && !OnScreenKeyboardProcess.HasExited)
        {
            OnScreenKeyboardProcess.Kill();
        }
    }

    public void RepositionOnScreenKeyboard(Rect rect)
    {
        // Run as administrator don't work, unless the CurrentUser is an administrator
        var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Osk", true);

        registryKey.SetValue("WindowLeft", rect.x, RegistryValueKind.DWord);
        registryKey.SetValue("WindowTop", rect.y, RegistryValueKind.DWord);
        registryKey.SetValue("WindowWidth", rect.width, RegistryValueKind.DWord);
        registryKey.SetValue("WindowHeight", rect.height, RegistryValueKind.DWord);
    }

    // Administrator authority is required
    public IEnumerator AsynRepositionOnScreenKeyboard(Rect rect)
    {
        var currentWindow = GetForegroundWindow();

        // Wait until window openned
        var osk = IntPtr.Zero;
        while (osk == IntPtr.Zero)
        {
            osk = FindWindowByCaption(IntPtr.Zero, "On-Screen Keyboard");
            yield return null;
        }

        var style = GetWindowLong(osk, GWL_STYLE);
        SetWindowLong(osk, GWL_STYLE, style & ~(WS_CAPTION | WS_THICKFRAME));
        // You can use the following method to change the position of the window,
        // But note that the following method requires administrator privileges,
        // So if you just need to set position and size, the safest way is to use the registry.
        SetWindowPos(osk, IntPtr.Zero, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, SWP_FRAMECHANGED);

        SetForegroundWindow(currentWindow);
    }

    private static Process ExternalCall(string filename, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = filename;
        startInfo.Arguments = arguments;

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        return process;
    }
}
