using System.Diagnostics;
using System.Runtime.InteropServices;
using CenterWindow.Contracts.Services;
using CenterWindow.Interop;
using WinRT.Interop;

namespace CenterWindow.Services;
internal class TrayIconService : ITrayIconService, IDisposable
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
        // 1. Obtener handle de la ventana WinUI
        _hwnd = WindowNative.GetWindowHandle(mainWindow);

        // 2. Cargar el icono .ico desde fichero
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        _hIcon = NativeMethods.LoadImage(
            IntPtr.Zero, iconPath,
            NativeMethods.IMAGE_ICON, 0, 0,
            NativeMethods.LR_LOADFROMFILE);

        // 3. Configurar NOTIFYICONDATA
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

        // 4. Delegate para el nuevo WndProc
        _wndProcDelegate = WndProc;
    }

    public void Initialize()
    {
        // Añadir icono
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, ref _nid);

        // Subclasificar la ventana para escuchar WM_TRAYICON
        var newProcPtr =
            Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _prevWndProc = NativeMethods.SetWindowLongPtr(
            _hwnd, NativeMethods.GWL_WNDPROC, newProcPtr);
    }

    public void ShowContextMenu()
    {
        // Crea el menú nativo
        var hMenu = NativeMethods.CreatePopupMenu();
        if (hMenu == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 1, "&Abrir");
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 2, "&Preferencias");
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_SEPARATOR, 0, string.Empty);
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, 3, "S&alir");

        // 3. Posicionar el menú en el cursor
        NativeMethods.GetCursorPos(out var pt);
        NativeMethods.SetForegroundWindow(_hwnd);

        // 4. Mostrar y obtener el comando seleccionado
        uint cmd = NativeMethods.TrackPopupMenu(
            hMenu,
            NativeMethods.TPM_RETURNCMD | NativeMethods.TPM_LEFTALIGN | NativeMethods.TPM_RIGHTBUTTON,
            pt.x, pt.y, 0,
            _hwnd,
            IntPtr.Zero);

        // 5. Si el usuario pulsó algo, lanza el evento
        if (cmd != 0)
        {
            OnMenuItemClicked((int)cmd);
        }

        // 6. Limpia el menú
        NativeMethods.DestroyMenu(hMenu);
    }

    public void Dispose()
    {
        // Eliminar icono
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, ref _nid);

        // Restaurar WndProc original
        NativeMethods.SetWindowLongPtr(
            _hwnd, NativeMethods.GWL_WNDPROC, _prevWndProc);
    }

    // Nuevo procedure para capturar clicks
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == NativeMethods.WM_TRAYICON)
        {
            Debug.WriteLine($"WM_TRAYICON: {wParam} {lParam}");
            switch ((uint)lParam)
            {
                case NativeMethods.WM_RBUTTONDOWN:  // WM_RBUTTONDOWN
                              // tu lógica al click izquierdo
                    ShowContextMenu();
                    break;
                case 0x0203:  // WM_LBUTTONDBLCLK
                              // doble click
                    break;
            }
        }

        if (msg == NativeMethods.WM_COMMAND)
        {
            var id = (int)wParam;
            TrayMenuItemClicked?.Invoke(this, new TrayMenuItemEventArgs(id));
        }

        // Forward al original
        return NativeMethods.CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
    }
}
