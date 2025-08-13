using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Helpers;
using CenterWindow.Interop;
using CenterWindow.Models;
using WinRT.Interop;
using static CenterWindow.Interop.Win32;

namespace CenterWindow.Services;
internal partial class TrayIconService : ITrayIconService, IDisposable
{
    private readonly IIconLoaderFactory _iconFactory;

    private readonly IntPtr _hwnd;
    private readonly IntPtr _hIcon;
    private IntPtr _prevWndProc;
    private Win32.NOTIFYICONDATA _nid;
    private readonly Win32.WndProc _wndProcDelegate;
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
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Tray icon - 16x16.png");
        //_hIcon = NativeMethods.LoadImage(
        //    IntPtr.Zero, iconPath,
        //    NativeMethods.IMAGE_ICON, 0, 0,
        //    NativeMethods.LR_LOADFROMFILE);
        _hIcon = _iconFactory
                     .GetLoader(IconLoaderType.GdiPlus)
                     .LoadIconAsync(iconPath, 16)
                     .GetAwaiter()
                     .GetResult();

        // Configure the NOTIFYICONDATA structure
        _nid = new Win32.NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<Win32.NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,  // identificador único
            uFlags = Win32.NIF_MESSAGE
                   | Win32.NIF_ICON
                   | Win32.NIF_TIP,
            uCallbackMessage = Win32.WM_TRAYICON,
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
        Win32.Shell_NotifyIcon(Win32.NIM_ADD, ref _nid);

        // Subclass the window to listen the WM_TRAYICON messages
        var newProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _prevWndProc = Win32.GetWindowLongPtr(_hwnd, Win32.GWL_WNDPROC);
        _ = Win32.SetWindowLongPtr(_hwnd, Win32.GWL_WNDPROC, newProcPtr);
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
            _ = Win32.DeleteObject(oldBmp);
        }
        _menuBitmaps.Clear();

        // Ask subscribers if they want to customize the menu before showing it
        var openingArgs = new TrayMenuOpeningEventArgs();
        OnTrayMenuOpening(openingArgs);

        // Create the context menu
        var hMenu = Win32.CreatePopupMenu();
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
        Win32.GetCursorPos(out var pt);
        //Win32.SetForegroundWindow(_hwnd);

        // Retrieve the command selected by the user: the TPM_RETURNCMD returns the command ID and doesn't post it to the WndProc method.
        var cmd = Win32.TrackPopupMenu(
            hMenu,
            Win32.TPM_RETURNCMD | Win32.TPM_LEFTALIGN | Win32.TPM_RIGHTBUTTON,
            pt.x, pt.y, 0,
            _hwnd,
            IntPtr.Zero);

        // If a command was selected, invoke the event handler
        if (cmd >= 0)
        {
            OnMenuItemClicked((int)cmd);
        }

        // Clean up the menu
        Win32.DestroyMenu(hMenu);
    }

    private void AppendItems(IntPtr parent, IEnumerable<TrayMenuItemDefinition> trayMenuItems)
    {
        foreach (var menuItemDefinition in trayMenuItems)
        {
            if (menuItemDefinition.IsSeparator)
            {
                Win32.AppendMenu(
                    parent,
                    Win32.MF_SEPARATOR,
                    (uint)UIntPtr.Zero,
                    string.Empty);
                continue;
            }

            // Set the flag for the menu item based on whether it has children or not
            var flags = Win32.MF_STRING | (menuItemDefinition.IsEnabled ? 0u: Win32.MF_GRAYED);

            var idOrSub = (UIntPtr)menuItemDefinition.Id;
            if (menuItemDefinition.Children.Count !=0 )
            {
                var menuHandle = Win32.CreatePopupMenu();
                AppendItems(menuHandle, menuItemDefinition.Children);
                idOrSub = (UIntPtr)menuHandle.ToInt64();
                flags = Win32.MF_POPUP;
            }


            Win32.AppendMenu(parent, flags, (uint)idOrSub, menuItemDefinition.Text);
            // If there is an icon and no children, set the bitmap for the menu item
            if (!string.IsNullOrEmpty(menuItemDefinition.IconPath) && menuItemDefinition.Children.Count == 0)
            {
                // Assume the icon size is 16x16 pixels,otherwise adjust as needed
                //var hBmp = CreateBitmapFromIcon(menuItemDefinition.IconPath, 16, 16);
                var hBmp = CreateBitmapWithAlphaFromIcon(menuItemDefinition.IconPath, 16, 16);
                //var hBmp = CreateBitmapFromImage(menuItemDefinition.IconPath, 16, 16);
                _menuBitmaps.Add(hBmp); // Keep track of the bitmap to release it later

                var itemInfo = new Win32.MENUITEMINFO
                {
                    cbSize  = (uint)Marshal.SizeOf<Win32.MENUITEMINFO>(),
                    fMask   = Win32.MIIM_BITMAP,
                    hbmpItem = hBmp
                };

                // Set the menu item info for the icon. Use false for command and true for position
                Win32.SetMenuItemInfo(parent, (uint)menuItemDefinition.Id, false, ref itemInfo);
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

        // Clean up the bitmaps used in the menu items
        foreach (var oldBmp in _menuBitmaps)
        {
            Win32.DeleteObject(oldBmp);
        }

        // Remove the icon from the system tray
        Win32.Shell_NotifyIcon(Win32.NIM_DELETE, ref _nid);

        // Restore original window procedure
        Win32.SetWindowLongPtr(_hwnd, Win32.GWL_WNDPROC, _prevWndProc);
    }

    /// <summary>
    /// Processes Windows messages sent to the application window.
    /// </summary>
    /// <remarks>This method handles specific messages such as <see cref="Win32.WM_TRAYICON"/> and 
    /// <see cref="Win32.WM_COMMAND"/> to provide custom behavior for tray icon interactions and menu commands.
    /// All other messages are forwarded to the original window procedure.</remarks>
    /// <param name="hWnd">A handle to the window receiving the message.</param>
    /// <param name="msg">The message identifier indicating the type of message being sent.</param>
    /// <param name="wParam">Additional message-specific information, typically used to pass data or flags.</param>
    /// <param name="lParam">Additional message-specific information, typically used to pass data or flags.</param>
    /// <returns>The result of the message processing, which depends on the message.  For unhandled messages, the result of the
    /// original window procedure is returned.</returns>
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == Win32.WM_TRAYICON)
        {
            switch ((uint)lParam)
            {
                case Win32.WM_RBUTTONDOWN:
                    ShowContextMenu();
                    break;
                case Win32.WM_LBUTTONDBLCLK:
                    OnTrayIconDoubleClicked();
                    break;
            }
        }

        // Forward all other messages to the original window procedure
        return Win32.CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
    }

    private IntPtr CreateBitmapFromImage(string path, int width, int height)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext == ".ico")
        {
            return CreateBitmapFromIcon(path, width, height);
        }
        else
        {
            return WinUiImageHelper.CreateBitmapFromFile(path, width, height);
        }
    }

    private IntPtr CreateBitmapFromIcon(string iconPath, int width, int height)
    {
        // Load the icon from the specified path
        var hIcon = Win32.LoadImage(
            IntPtr.Zero,
            iconPath,
            Win32.IMAGE_ICON,
            width,
            height,
            Win32.LR_LOADFROMFILE);

        // Get the device context for the screen and create a compatible DC and bitmap
        var screenDC = Win32.GetDC(IntPtr.Zero);
        var memDC = Win32.CreateCompatibleDC(screenDC);
        var hBitmap = Win32.CreateCompatibleBitmap(screenDC, width, height);
        var oldBmp = Win32.SelectObject(memDC, hBitmap);

        // Draw the icon onto the bitmap
        Win32.DrawIconEx(memDC, 0, 0, hIcon, width, height, 0, IntPtr.Zero, Win32.DI_NORMAL);

        // Clean up resources
        Win32.SelectObject(memDC, oldBmp);
        Win32.DeleteDC(memDC);
        _ = Win32.ReleaseDC(IntPtr.Zero, screenDC);
        DestroyIcon(hIcon);

        return hBitmap;
    }

    /// <summary>
    /// Creates a 32-bit bitmap with alpha transparency from an icon file.
    /// </summary>
    /// <remarks>This method loads an icon from the specified file path and draws it onto a 32-bit top-down
    /// bitmap with alpha transparency. The resulting bitmap can be used in GDI operations or other graphics
    /// contexts.</remarks>
    /// <param name="iconPath">The file path of the icon to load. The file must exist and be a valid icon file.</param>
    /// <param name="width">The width, in pixels, of the resulting bitmap. Must be greater than 0.</param>
    /// <param name="height">The height, in pixels, of the resulting bitmap. Must be greater than 0.</param>
    /// <returns>A handle to the created bitmap (HBITMAP). The caller is responsible for releasing the bitmap using <see
    /// cref="DeleteObject(IntPtr)"/> when it is no longer needed.</returns>
    public static IntPtr CreateBitmapWithAlphaFromIcon(string iconPath, int width, int height)
    {
        // Prepare the 32bpp top-down bitmap info
        var hdr = new BITMAPINFOHEADER
        {
            biSize        = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
            biWidth       = width,
            biHeight      = -height, // negativo = top-down
            biPlanes      = 1,
            biBitCount    = 32,
            biCompression = BI_RGB,
            biSizeImage   = (uint)(width * height * 4)
        };
        var info = new BITMAPINFO { bmiHeader = hdr };

        // Create DCs and DIB section
        var screenDC = GetDC(IntPtr.Zero);
        var memDC = CreateCompatibleDC(screenDC);
        IntPtr ppvBits;
        var hBmp = CreateDIBSection(memDC, ref info, DIB_RGB_COLORS, out ppvBits, IntPtr.Zero, 0);

        // Load the icon from the specified path
        var hIcon = LoadImage(
            IntPtr.Zero,
            iconPath,
            IMAGE_ICON,
            width,
            height,
            LR_LOADFROMFILE);

        // Draw the icon onto the bitmap with alpha
        var oldBmp = SelectObject(memDC, hBmp);
        DrawIconEx(memDC, 0, 0, hIcon, width, height, 0, IntPtr.Zero, Win32.DI_NORMAL); // DI_NORMAL|DI_COMPAT
        SelectObject(memDC, oldBmp);

        // Clean up resources
        DeleteDC(memDC);
        _ = ReleaseDC(IntPtr.Zero, screenDC);
        DestroyIcon(hIcon);

        // hBmp should be destroyed by the caller when no longer needed
        return hBmp;
    }
}
