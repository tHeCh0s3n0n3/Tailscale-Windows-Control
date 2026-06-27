using System.Runtime.InteropServices;

namespace Tailscale_Windows_Control;

public static class TaskbarService
{
    [ComImport]
    [Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        // ITaskbarList
        void HrInit();
        void AddTab(IntPtr hwnd);
        void DeleteTab(IntPtr hwnd);
        void ActivateTab(IntPtr hwnd);
        void SetActiveAlt(IntPtr hwnd);

        // ITaskbarList2
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        void SetProgressState(IntPtr hwnd, int tbpFlags);
        void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        void UnregisterTab(IntPtr hwndTab);
        void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, int tbpFlags);
        void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, IntPtr pButton);
        void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, IntPtr pButton);
        void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);

        // This is the method responsible for the overlay icon
        void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
    }

    [ComImport]
    [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
    private class TaskbarList { }

    private static readonly ITaskbarList3? _taskbarList;

    static TaskbarService()
    {
        try
        {
            // Safely instantiate the Windows Taskbar COM object
            _taskbarList = (ITaskbarList3)new TaskbarList();
            _taskbarList.HrInit();
        }
        catch
        {
            _taskbarList = null; // Fallback if OS doesn't support it
        }
    }

    public static void SetOverlay(IntPtr windowHandle, System.Drawing.Icon? icon, string description)
    {
        // hIcon is a pointer to the icon resource; use IntPtr.Zero if clearing it
        IntPtr hIcon = icon?.Handle ?? IntPtr.Zero;
        _taskbarList?.SetOverlayIcon(windowHandle, hIcon, description);
    }
}
