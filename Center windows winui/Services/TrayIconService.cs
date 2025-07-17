using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using WinRT.Interop;

namespace CenterWindow.Services;
internal partial class TrayIconService : ITrayIconService, IDisposable
{
    private readonly IntPtr _hwnd;
    private readonly IntPtr _hIcon;
    private IntPtr _prevWndProc;
    private NativeMethods.NOTIFYICONDATA _nid;
    private readonly NativeMethods.WndProc _wndProcDelegate;

    public event EventHandler<TrayMenuItemEventArgs>? TrayMenuItemClicked;
    protected virtual void OnMenuItemClicked(int id) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemEventArgs(id));

    public TrayIconService(WindowEx mainWindow)
    {
        // Get the window handle of the WinUI window
        _hwnd = WindowNative.GetWindowHandle(mainWindow);

        // Load the icon from the Assets folder
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        _hIcon = NativeMethods.LoadImage(
            IntPtr.Zero, iconPath,
            NativeMethods.IMAGE_ICON, 0, 0,
            NativeMethods.LR_LOADFROMFILE);

        // Configure the NOTIFYICONDATA structure
        _nid = new NativeMethods.NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,  // identificador único
            uFlags = NativeMethods.NIF_MESSAGE
                   | NativeMethods.NIF_ICON
                   | NativeMethods.NIF_TIP,
            uCallbackMessage = NativeMethods.WM_TRAYICON,
            hIcon = _hIcon,
            szTip = "Mi App WinUI"
        };

        // Create the delegate for the new window procedure
        _wndProcDelegate = WndProc;
    }

    public void Initialize()
    {
        // Add the icon to the system tray
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, ref _nid);

        // Subclass the window to listen the WM_TRAYICON messages
        var newProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _prevWndProc = NativeMethods.SetWindowLongPtr(_hwnd, NativeMethods.GWL_WNDPROC, newProcPtr);
    }

    /// <summary>
    /// Displays a context menu at the current cursor position and handles the user's selection.
    /// </summary>
    /// <remarks>The context menu includes predefined options such as "Open," "Preferences," and "Exit."  When
    /// a menu item is selected, the corresponding action is triggered by raising an event  with the selected command
    /// identifier. If no selection is made, the method exits without  performing any action.</remarks>
    public void ShowContextMenu()
    {
        // Create the context menu
        var hMenu = NativeMethods.CreatePopupMenu();
        if (hMenu == IntPtr.Zero)
        {
            return;
        }

        // Populate the menu with items
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 1, "&Abrir");
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 2, "&Preferencias");
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_SEPARATOR, 0, string.Empty);
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 3, "S&alir");

        // Set the menu at the cursor position
        NativeMethods.GetCursorPos(out var pt);
        NativeMethods.SetForegroundWindow(_hwnd);

        // Retrieve the command selected by the user
        uint cmd = NativeMethods.TrackPopupMenu(
            hMenu,
            NativeMethods.TPM_RETURNCMD | NativeMethods.TPM_LEFTALIGN | NativeMethods.TPM_RIGHTBUTTON,
            pt.x, pt.y, 0,
            _hwnd,
            IntPtr.Zero);

        // Fire the event for the selected command
        if (cmd != 0)
        {
            OnMenuItemClicked((int)cmd);
        }

        // Clean up the menu
        NativeMethods.DestroyMenu(hMenu);
    }

    public void Dispose()
    {
        // Remove the icon from the system tray
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, ref _nid);

        // Restore original window procedure
        NativeMethods.SetWindowLongPtr(_hwnd, NativeMethods.GWL_WNDPROC, _prevWndProc);
    }

    /// <summary>
    /// Processes Windows messages sent to the application window.
    /// </summary>
    /// <remarks>This method handles specific messages such as <see cref="NativeMethods.WM_TRAYICON"/> and 
    /// <see cref="NativeMethods.WM_COMMAND"/> to provide custom behavior for tray icon interactions and menu commands.
    /// All other messages are forwarded to the original window procedure.</remarks>
    /// <param name="hWnd">A handle to the window receiving the message.</param>
    /// <param name="msg">The message identifier indicating the type of message being sent.</param>
    /// <param name="wParam">Additional message-specific information, typically used to pass data or flags.</param>
    /// <param name="lParam">Additional message-specific information, typically used to pass data or flags.</param>
    /// <returns>The result of the message processing, which depends on the message.  For unhandled messages, the result of the
    /// original window procedure is returned.</returns>
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == NativeMethods.WM_TRAYICON)
        {
            switch ((uint)lParam)
            {
                case NativeMethods.WM_RBUTTONDOWN:
                    ShowContextMenu();
                    break;
                case NativeMethods.WM_LBUTTONDBLCLK:
                    break;
            }
        }

        if (msg == NativeMethods.WM_COMMAND)
        {
            var id = checked((int)wParam);
            OnMenuItemClicked(id);
        }

        // Forward all other messages to the original window procedure
        return NativeMethods.CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
    }
}
