using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using CenterWindow.Models;
using WinRT.Interop;

namespace CenterWindow.Services;
internal partial class TrayIconService : ITrayIconService, IDisposable
{
    private readonly IIconLoaderFactory _iconFactory;

    private readonly IntPtr _hwnd;
    private readonly IntPtr _hIcon;
    private IntPtr _prevWndProc;
    private NativeMethods.NOTIFYICONDATA _nid;
    private readonly NativeMethods.WndProc _wndProcDelegate;
    private bool _isInitialized;

    public event EventHandler<TrayMenuItemEventArgs>? TrayMenuItemClicked;
    protected virtual void OnMenuItemClicked(int id) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemEventArgs(id));

    public event EventHandler? TrayMenuIconDoubleClicked;
    protected virtual void OnTrayIconDoubleClicked() => TrayMenuIconDoubleClicked?.Invoke(this, EventArgs.Empty);

    public event EventHandler<TrayMenuOpeningEventArgs>? TrayMenuOpening;
    protected virtual void OnTrayMenuOpening(TrayMenuOpeningEventArgs e) => TrayMenuOpening?.Invoke(this, e);

    // This keeps track of the bitmaps used in the menu items, so that they can be released later
    private readonly List<IntPtr> _menuBitmaps = [];

    public TrayIconService(WindowEx mainWindow, IIconLoaderFactory iconFactory)
    {
        // Get the icon factory from the dependency injection container
        _iconFactory = iconFactory;

        // Get the window handle of the WinUI window
        _hwnd = WindowNative.GetWindowHandle(mainWindow);

        // Load the icon from the Assets folder
        //var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        //_hIcon = NativeMethods.LoadImage(
        //    IntPtr.Zero, iconPath,
        //    NativeMethods.IMAGE_ICON, 0, 0,
        //    NativeMethods.LR_LOADFROMFILE);
        _hIcon = _iconFactory
                     .GetLoader(IconLoaderType.GdiPlus)
                     .LoadIconAsync("Assets\\Tray icon - 16x16.png", 16)
                     .GetAwaiter()
                     .GetResult();

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
            szTip = "Center windows"
        };

        // Create the delegate for the new window procedure
        _wndProcDelegate = WndProc;
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        // Add the icon to the system tray
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, ref _nid);

        // Subclass the window to listen the WM_TRAYICON messages
        var newProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _prevWndProc = NativeMethods.GetWindowLongPtr(_hwnd, NativeMethods.GWL_WNDPROC);
        _ = NativeMethods.SetWindowLongPtr(_hwnd, NativeMethods.GWL_WNDPROC, newProcPtr);
    }

    /// <summary>
    /// Displays a context menu at the current cursor position and handles the user's selection.
    /// </summary>
    /// <remarks>The context menu includes predefined options such as "Open," "Preferences," and "Exit."  When
    /// a menu item is selected, the corresponding action is triggered by raising an event  with the selected command
    /// identifier. If no selection is made, the method exits without  performing any action.</remarks>
    public void ShowContextMenu()
    {
        // Make sure the bitmaps list is empty before creating a new menu
        foreach (var oldBmp in _menuBitmaps)
        {
            NativeMethods.DeleteObject(oldBmp);
        }
        _menuBitmaps.Clear();

        // Ask subscribers if they want to customize the menu before showing it
        var openingArgs = new TrayMenuOpeningEventArgs();
        OnTrayMenuOpening(openingArgs);

        // Create the context menu
        var hMenu = NativeMethods.CreatePopupMenu();
        if (hMenu == IntPtr.Zero)
        {
            return;
        }

        // Populate the menu with items
        //NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 1, "&Abrir");
        //NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 2, "&Preferencias");
        //NativeMethods.AppendMenu(hMenu, NativeMethods.MF_SEPARATOR, 0, string.Empty);
        //NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 3, "S&alir");
        AppendItems(hMenu, openingArgs.Items);

        // Set the menu at the cursor position
        NativeMethods.GetCursorPos(out var pt);
        NativeMethods.SetForegroundWindow(_hwnd);

        // Retrieve the command selected by the user: the TPM_RETURNCMD returns the command ID and doesn't post it to the WndProc method.
        var cmd = NativeMethods.TrackPopupMenu(
            hMenu,
            NativeMethods.TPM_RETURNCMD | NativeMethods.TPM_LEFTALIGN | NativeMethods.TPM_RIGHTBUTTON,
            pt.x, pt.y, 0,
            _hwnd,
            IntPtr.Zero);

        // If a command was selected, invoke the event handler
        if (cmd != 0)
        {
            OnMenuItemClicked((int)cmd);
        }

        // Clean up the menu
        NativeMethods.DestroyMenu(hMenu);
    }

    private void AppendItems(IntPtr parent, IEnumerable<TrayMenuItemDefinition> trayMenuItems)
    {
        foreach (var menuItemDefinition in trayMenuItems)
        {
            if (menuItemDefinition.IsSeparator)
            {
                NativeMethods.AppendMenu(
                    parent,
                    NativeMethods.MF_SEPARATOR,
                    (uint)UIntPtr.Zero,
                    string.Empty);
                continue;
            }

            // Set the flag for the menu item based on whether it has children or not
            var flags = NativeMethods.MF_STRING | (menuItemDefinition.IsEnabled ? 0u: NativeMethods.MF_GRAYED);

            var idOrSub = (UIntPtr)menuItemDefinition.Id;
            if (menuItemDefinition.Children.Count !=0 )
            {
                var menuHandle = NativeMethods.CreatePopupMenu();
                AppendItems(menuHandle, menuItemDefinition.Children);
                idOrSub = (UIntPtr)menuHandle.ToInt64();
                flags = NativeMethods.MF_POPUP;
            }


            NativeMethods.AppendMenu(parent, flags, (uint)idOrSub, menuItemDefinition.Text);
            // If there is an icon and no children, set the bitmap for the menu item
            if (!string.IsNullOrEmpty(menuItemDefinition.IconPath) && menuItemDefinition.Children.Count == 0)
            {
                // Assume the icon size is 16x16 pixels,otherwise adjust as needed
                var hBmp = CreateBitmapFromIcon(menuItemDefinition.IconPath, 16, 16);
                _menuBitmaps.Add(hBmp); // Keep track of the bitmap to release it later

                var itemInfo = new NativeMethods.MENUITEMINFO
                {
                    cbSize  = (uint)Marshal.SizeOf<NativeMethods.MENUITEMINFO>(),
                    fMask   = NativeMethods.MIIM_BITMAP,
                    hbmpItem = hBmp
                };

                // Set the menu item info for the icon. Use false for command and true for position
                NativeMethods.SetMenuItemInfo(parent, (uint)menuItemDefinition.Id, false, ref itemInfo);
            }
        }
    }

    public void Dispose()
    {
        if (!_isInitialized)
        {
            return;
        }

        _isInitialized = false;

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
                    OnTrayIconDoubleClicked();
                    break;
            }
        }

        // Forward all other messages to the original window procedure
        return NativeMethods.CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
    }

    private IntPtr CreateBitmapFromIcon(string iconPath, int width, int height)
    {
        // Load the icon from the specified path
        var hIcon = NativeMethods.LoadImage(
            IntPtr.Zero,
            iconPath,
            NativeMethods.IMAGE_ICON,
            width,
            height,
            NativeMethods.LR_LOADFROMFILE);

        // Get the device context for the screen and create a compatible DC and bitmap
        var screenDC = NativeMethods.GetDC(IntPtr.Zero);
        var memDC = NativeMethods.CreateCompatibleDC(screenDC);
        var hBitmap = NativeMethods.CreateCompatibleBitmap(screenDC, width, height);
        var oldBmp = NativeMethods.SelectObject(memDC, hBitmap);

        // Draw the icon onto the bitmap
        NativeMethods.DrawIconEx(memDC, 0, 0, hIcon, width, height, 0, IntPtr.Zero, NativeMethods.DI_NORMAL);

        // Clean up resources
        NativeMethods.SelectObject(memDC, oldBmp);
        NativeMethods.DeleteDC(memDC);
        _ = NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);

        return hBitmap;
    }
}
