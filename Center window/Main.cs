using System.Diagnostics;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
// https://stackoverflow.com/questions/56844233/additional-probing-paths-for-net-core-3-migration
// https://www.google.es/search?ei=pba3X-OAINaDhbIPlOil2A0&q=.net+5+%22probing+privatePath%22&oq=.net+5+%22probing+privatePath%22&gs_lcp=CgZwc3ktYWIQAzIICCEQFhAdEB5QmyNYiypggy1oAHAAeACAAXaIAdMCkgEDMC4zmAEAoAEBqgEHZ3dzLXdpesABAQ&sclient=psy-ab&ved=0ahUKEwij59z1j5HtAhXWQUEAHRR0CdsQ4dUDCA0&uact=5
// https://www.google.es/search?q=deps.json+relative+path&ie=UTF-8&oe=

namespace Center_window;

public partial class FrmMain : Form
{
    
    #region Variables de la clase
    
    private bool _capturing;        // Es TRUE cuando estamos capturando con el ratón
    private readonly Image _finderHome;
    private readonly Image _finderGone;
    private readonly Cursor _cursorDefault;
    private readonly Cursor _cursorFinder;
    //private IntPtr _hPreviousWindow;
    private IntPtr _hActualWindow;  // Puntero a la ventana que está bajo el ratón
    private IntPtr _hParentWindow;  // Ventana padre de la que está bajo el raón

    private ClassSettings Settings = new();
    private readonly System.Resources.ResourceManager StringsRM = new("Center_window.localization.strings", typeof(FrmMain).Assembly);

    #endregion Variables de la clase

    /// <summary>
    /// Summary description for frmMain.
    /// </summary>
    public FrmMain()
    {
        InitializeComponent();
        
        // Set form icon
        this.Icon = EmbeddedResources.Load<Icon>(EmbeddedResources.AppLogo);

        // Escribir el texto de la etiqueta lblInfo
        //this.lblInfo.Text = new String(
        this.lblInfo.Text = "Drag the Finder Tool over a window to select it, then release the mouse button.";
        this.lblInfo.Text += " Or enter a window handle (in hexadecimal). Or simply select the applications";
        this.lblInfo.Text += " from the list below.";

        // Inicializar el control lstWindows
        lstWindows.View = System.Windows.Forms.View.Details;
        lstWindows.Columns.Add("Window caption");
        lstWindows.Columns.Add("Window handle");
        lstWindows.Columns.Add("Module name");
        lstWindows.Columns[0].Width = 360;
        lstWindows.Columns[1].Width = 125;
        lstWindows.Columns[2].Width = 190;

        // Inicializar las variables
        _cursorDefault = Cursor.Current ?? Cursors.Default;
        //_cursorFinder = EmbeddedResources.LoadGraphicsResource<Cursor>(@"images\Finder.cur");
        //_finderHome = EmbeddedResources.LoadGraphicsResource<Image>(@"images\FinderHome.bmp");
        //_finderGone = EmbeddedResources.LoadGraphicsResource<Image>(@"images\FinderGone.bmp");
        _cursorFinder = EmbeddedResources.LoadCursor(EmbeddedResources.Finder);
        _finderHome = EmbeddedResources.LoadImage(EmbeddedResources.FinderHome);
        _finderGone = EmbeddedResources.LoadImage(EmbeddedResources.FinderGone);
        _pictureBox.Image = _finderHome;

        // Establecer los eventos
        _pictureBox.MouseDown += new MouseEventHandler(OnFinderToolMouseDown);

        // Load and apply the program settings
        bool result = LoadProgramSettingsJSON();
        if (result)
            ApplySettingsJSON(Settings.WindowPosition);
        else
            ApplySettingsJSON();

    }

    /// <summary>
    /// Processes window messages sent to the Spy Window
    /// </summary>
    /// <param name="m"></param>
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            // Stop capturing events as soon as the user releases the left mouse button
            case (int)Win32.WindowMessages.WM_LBUTTONUP:
                this.CaptureMouse(false);
                break;
            
            // Handle all the mouse movements
            case (int)Win32.WindowMessages.WM_MOUSEMOVE:
                this.HandleMouseMovements();
                break;

            // Occurs when the user closes the form
            case (int) Win32.WindowMessages.WM_CLOSE:
                this.FrmMain_Close();
                break;
        };
        
        // Dispatch the message
        base.WndProc(ref m);
    }

    #region Private routines

    /// <summary>
    /// Captures or releases the mouse
    /// </summary>
    /// <param name="captured"></param>
    private void CaptureMouse(bool captured)
    {
        // if we're supposed to capture the window
        if (captured)
        {
            // capture the mouse movements and send them to ourself
            Win32.SetCapture(this.Handle);

            // set the mouse cursor to our finder cursor
            Cursor.Current = _cursorFinder;

            // change the image to the finder gone image
            _pictureBox.Image = _finderGone;
        }
        // otherwise we're supposed to release the mouse capture
        else
        {
            // so release it
            _ = Win32.ReleaseCapture();

            // put the default cursor back
            Cursor.Current = _cursorDefault;

            // change the image back to the finder at home image
            _pictureBox.Image = _finderHome;

            // and finally refresh any window that we were highlighting
            if (_hActualWindow != IntPtr.Zero)
                WindowHighlighter.Refresh(_hActualWindow);                 
            /*if (_hPreviousWindow != IntPtr.Zero)
            {
                WindowHighlighter.Refresh(_hPreviousWindow);
                _hPreviousWindow = IntPtr.Zero;
            }*/
        }

        // save our capturing state
        _capturing = captured;
    }

    /// <summary>
    /// Handles all mouse move messages sent to the Spy Window
    /// </summary>
    private void HandleMouseMovements(IntPtr handle = default)
    {
        // if we're not capturing, then bail out
        if (!_capturing)
            return;

        try
        {
            // capture the window under the cursor's position
            IntPtr hWnd = Win32.WindowFromPoint(Cursor.Position);
            //hWnd = handle;

            // Get the parent
            _hParentWindow = Win32.GetParent(hWnd);

            // Si es una ventana child y se ha seleccionado sólo Parents entonces salir
            if (_hParentWindow != IntPtr.Zero && Settings.OnlyParentWnd == true)
                return;

            // if the window we're over, is not the same as the one before, and we had one before, refresh it
            if (_hActualWindow != IntPtr.Zero && _hActualWindow != hWnd)
                WindowHighlighter.Refresh(_hActualWindow);

            //if (_hPreviousWindow != IntPtr.Zero && _hPreviousWindow != hWnd)
            //    WindowHighlighter.Refresh(_hPreviousWindow);

            // if we didn't find a window.. that's pretty hard to imagine. lol
            if (hWnd == IntPtr.Zero)
            {
                // Borrar los controles de texto y las variables
                ClearSelection();
            }
            else
            {
                // save the window we're over
                //_hPreviousWindow = hWnd;
                _hActualWindow = hWnd;

                // Get the rectangle
                Win32.Rect RectWindow = new();
                Win32.GetWindowRect(hWnd, ref RectWindow);

                // handle
                txtHandle.Text = String.Format("{0}", hWnd.ToInt32().ToString());

                // class
                txtClass.Text = Win32.GetClassName(hWnd);
                
                // caption
                txtCaption.Text = Win32.GetWindowText(hWnd);
                
                // Show rectangle dimensions
                txtRectangle.Text = $"[{RectWindow.right - RectWindow.left} x {RectWindow.bottom - RectWindow.top}], ({RectWindow.left},{RectWindow.top})-({RectWindow.right},{RectWindow.bottom})";

                // highlight the window
                WindowHighlighter.Highlight(hWnd, Color.FromArgb(Settings.RectangleColor), Settings.RectangleWidth);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// Centra una ventana en la pantalla
    /// </summary>
    /// <param name="hWnd">Puntero al handle de la ventana que se quiere centrar</param>
    private void MoveWindow(IntPtr hWnd)
    {
        // Si está activada la casilla de verificación
        if (Settings.CenterWindow == true)
        {
            // Obtener las dimensiones de la pantalla
            Win32.Rect pantalla = new();
            Win32.GetWindowRect(Win32.GetDesktopWindow(), ref pantalla);

            // Obtener las dimensiones de la ventana
            Win32.Rect ventana = new();
            Win32.GetWindowRect(hWnd, ref ventana);

            // Centrar la ventana en la pantalla
            Win32.MoveWindow(hWnd, (pantalla.Width - ventana.Width) / 2,
                (pantalla.Height - ventana.Height) / 2,
                ventana.Width,
                ventana.Height,
                true);
        }

    }

    /// <summary>
    /// Establece un valor de transparencia a una ventana
    /// </summary>
    /// <param name="hWnd">Puntero al "handle" de la ventana a la que se quiere dar transparencia</param>
    private void TransparentWindow(IntPtr hWnd)
    {
        // Si está activada la casilla de verificación
        if (Settings.Transparency == true)
        {
            _ = Win32.SetWindowLong(hWnd,
                Win32.GWL_EXSTYLE,
                Win32.GetWindowLongPtr(hWnd, Win32.GWL_EXSTYLE) | (int)Win32.WindowStylesEx.WS_EX_LAYERED);

            _ = Win32.SetLayeredWindowAttributes(hWnd,
                (uint)Color.Black.ToArgb(),
                (byte)(255 - trkTransparency.Value),
                Win32.LWA_ALPHA);
        }
    }

    /// <summary>
    /// Borra el contenido de los controles de texto y las variables hWnd
    /// </summary>
    private void ClearSelection()
    {
        // Borrar el contenido de los controles de texto
        this.txtCaption.Text = String.Empty;
        this.txtHandle.Text = String.Empty;
        this.txtClass.Text = String.Empty;
        this.txtRectangle.Text = String.Empty;

        // Borrar las variables hWnd
        _hActualWindow = IntPtr.Zero;
        _hParentWindow = IntPtr.Zero;
        //_hPreviousWindow = IntPtr.Zero;
    }

    #endregion Private routines

    #region Callback EnumWindows

    /// <summary>
    /// Provides the callback function for the EnumWindows user32.dll function
    /// </summary>
    /// <param name="hWnd">(Pointer to the) window handle</param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private bool EnumerateWindows(IntPtr hWnd, IntPtr lParam)
    {
        bool WindowVisible = Win32.IsWindowVisible(hWnd);
        string WindowText = Win32.GetWindowText(hWnd);
        string WindowClassName = Win32.GetClassName(hWnd);
        if (WindowClassName == "Program" || WindowClassName == "Button") WindowClassName = string.Empty;

        if (!string.IsNullOrEmpty(WindowText) && WindowVisible && !string.IsNullOrEmpty(WindowClassName))
        {
            // Retrieves the name of the windows's executable
            String strWindowModule;
            IntPtr handle;
            _ = Win32.GetWindowThreadProcessId(hWnd, out uint uHandle);
            handle = Win32.OpenProcess((uint)(Win32.PROCESS_ACCESS_TYPES.PROCESS_QUERY_INFORMATION | Win32.PROCESS_ACCESS_TYPES.PROCESS_VM_READ), false, uHandle);
            strWindowModule = Win32.GetModuleBaseName(handle);
            //windowModule = Win32.GetModuleFileNameEx(handle); //Gets the full path
                /* Gets the same results but using the .NET framework
                Process p = Process.GetProcessById((int)uHandle);
                windowModule = p.MainModule.ModuleName.ToString();
                */

            // Gets additional window info: we are interested in the border width
            Win32.WindowInfo winInfo = new();
            Win32.GetWindowInfo(hWnd, ref winInfo);

            //if (windowText != "Microsoft Edge" && windowText != "Program Manager")
            ListViewItem item;
            if (winInfo.xWindowBorders > 0 && winInfo.xWindowBorders > 0 && winInfo.window.Width > 0 && winInfo.window.Height > 0)
            {
                item = lstWindows.Items.Add(WindowText);
                item.SubItems.Add(hWnd.ToString());
                item.SubItems.Add(strWindowModule);
            }

        }
        return true;
    }

    #endregion Callback EnumWindows
    
    #region Form events

    /// <summary>
    /// Pass the settings values to the controls so they display the correct values
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FrmMain_Load(object sender, EventArgs e)
    {
        // Creates the fade in animation of the form
        Win32.AnimateWindow(this.Handle, 500, Win32.AnimateWindowFlags.AW_BLEND);
    }

    /// <summary>
    /// Occurs after the form is loaded and shown for the first time
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FrmMain_Shown(object sender, EventArgs e)
    {
        // Close the splash screen
        using var closeSplashEvent = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset, "CloseSplashScreenEvent");
        closeSplashEvent.Set();

        // Rellenar el control lstWindows
        BtnApp.PerformClick();  // Much better alternative than the old trick BtnApp_Click(null, null)
    }

    /// <summary>
    /// Occurs when the user closes the form
    /// </summary>
    private void FrmMain_Close()
    {
        // Save settings data
        SaveProgramSettingsJSON();

        // Creates the fade out animation of the form
        Win32.AnimateWindow(this.Handle, 500, Win32.AnimateWindowFlags.AW_BLEND | Win32.AnimateWindowFlags.AW_HIDE);
    }

    /// <summary>
    /// Resize en reposition the textboxes and their associated labels
    /// The remaining controls reposition themselves automatically
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FrmMain_Resize(object sender, EventArgs e)
    {
        // Resize and reposition the textboxes and labels
        Int32 nAncho = (this.Size.Width - 15 - 20 - 32)/2;
        this.txtCaption.Width = nAncho;
        this.txtHandle.Width = nAncho;
        this.txtClass.Width = nAncho;
        this.txtRectangle.Width = nAncho;

        this.txtClass.Left = 15 + nAncho + 20;
        this.txtRectangle.Left = 15 + nAncho + 20;

        this.lblClass.Left = 15 + nAncho + 17;
        this.lblRectangle.Left = 15 + nAncho + 17;
    }

    #endregion Form events
    
    #region Form controls events

    /// <summary>
    /// Processes the mouse down events for the finder tool 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnFinderToolMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            this.CaptureMouse(true);
    }

    /// <summary>
    /// Fill the lstWindows with all the applications found
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnApp_Click(object sender, EventArgs e)
    {
        // Clear any contents
        this.lstWindows.Items.Clear();

        // Use the Win32 api to enumerate all the windows
        Win32.EnumWindows(new Win32.EnumWindowEventHandler(EnumerateWindows), IntPtr.Zero);

        // Use the .NET framework to enumerate all the windows
        /*ListViewItem item = new ListViewItem();
        
        foreach (Process p in Process.GetProcesses(System.Environment.MachineName).Where(p=> p.MainWindowHandle != IntPtr.Zero))
        {
            item = lstWindows.Items.Add(p.MainWindowTitle);
            item.SubItems.Add(p.MainWindowHandle.ToString());
            item.SubItems.Add(p.MainModule.ModuleName);
            /*string nombre = Win32.GetModuleBaseName(p.Handle);
            if (p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle != String.Empty)
            {
                item = lstWindows.Items.Add(p.MainWindowTitle);
                item.SubItems.Add(p.MainWindowHandle.ToString());
                item.SubItems.Add(p.ProcessName);
            }
        }*/

    }

    /// <summary>
    /// Centrar la ventana y establecer el nivel de transparencia.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnAply_Click(object sender, EventArgs e)
    {
        // Activar el control de errores en la llamada a la API
        try
        {
            // Si sólo se pueden modificar las ventanas padre
            if (_hParentWindow != IntPtr.Zero && Settings.OnlyParentWnd == true)
                return;
            else
            {
                // Cuando se ha utilizado la herramienta Finder
                MoveWindow(_hActualWindow);
                TransparentWindow(_hActualWindow);
            }

            // Para cada uno de las aplicaciones en el lstWindows
            IntPtr hWnd = IntPtr.Zero;
            foreach (ListViewItem item in lstWindows.CheckedItems)
            {                    
                hWnd = (IntPtr) Convert.ToInt32(item.SubItems[1].Text);
                //WindowHighlighter.Refresh(hWnd);
                //HandleMouseMovements(hWnd);
                MoveWindow(hWnd);
                TransparentWindow(hWnd);
            }

            //public static uint GetColor(Color C)
            //{
            //    return ((uint)C.B<<16) + ((uint)C.G<<8) + (uint)C.R;
            //}

            // Remove WS_EX_LAYERED from this window styles
            //SetWindowLong(hwnd, GWL_EXSTYLE,
            //GetWindowLong(hwnd, GWL_EXSTYLE) & ~WS_EX_LAYERED);

            // Ask the window and its children to repaint
            //RedrawWindow(hwnd, 
            //    NULL, 
            //    NULL, 
            //    RDW_ERASE | RDW_INVALIDATE | RDW_FRAME | RDW_ALLCHILDREN);
        }
        catch (Exception er)
        {
            MessageBox.Show(er.Message);
        }
    }

    private void BtnOptions_Click(object sender, EventArgs e)
    {
        Options frm = new(Settings);
        frm.ShowDialog(this);
        if (frm.DialogResult == DialogResult.OK)
            Settings = frm.Settings;
    }

    private void BtnClose_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void Transparency_Changed(object sender, EventArgs e)
    {
        int Percentage;
        Percentage = 100 * trkTransparency.Value/trkTransparency.Maximum;      
        lblTransparencyValue.Text = Percentage.ToString () + "%";
    }

    private void CheckTransparency_Changed(object sender, EventArgs e)
    {
        trkTransparency.Enabled = chkTransparency.Checked;
        lblTransparencyValue.Enabled = chkTransparency.Checked;
        Settings.Transparency = chkTransparency.Checked;
    }

    private void Center_CheckedChanged(object sender, EventArgs e)
    {
        Settings.CenterWindow = chkCenter.Checked;
    }

    private void Caption_TextChanged(object sender, EventArgs e)
    {
        // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
        if (((Control)sender).Text == String.Empty)
            ClearSelection();
    }

    private void Handle_TextChanged(object sender, EventArgs e)
    {
        // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
        if (((Control)sender).Text == String.Empty)
            ClearSelection();
    }

    private void Class_TextChanged(object sender, EventArgs e)
    {
        // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
        if (((Control)sender).Text == String.Empty)
            ClearSelection();
    }

    private void Rectangle_TextChanged(object sender, EventArgs e)
    {
        // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
        if (((Control)sender).Text == String.Empty)
            ClearSelection();
    }

    private void AppChecked(object sender, ItemCheckedEventArgs e)
    {
        // if it is checked, then show the border around
        if (e.Item.Checked)
        {
            // Highlight the window
            var hWnd = Win32.GetHandleWindow(e.Item.Text);
            if (hWnd != IntPtr.Zero)
                WindowHighlighter.Highlight(hWnd, Color.FromArgb(Settings.RectangleColor), Settings.RectangleWidth);
        }
    }

    #endregion Form controls events


}
