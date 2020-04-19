using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace Center_window
{
    public partial class frmMain : Form
    {
        
        #region Variables de la clase
        
        private bool _capturing;        // Es TRUE cuando estamos capturando con el ratón
        private Image _finderHome;
        private Image _finderGone;
        private Cursor _cursorDefault;
        private Cursor _cursorFinder;
        //private IntPtr _hPreviousWindow;
        private IntPtr _hActualWindow;  // Puntero a la ventana que está bajo el ratón
        private IntPtr _hParentWindow;  // Ventana padre de la que está bajo el raón

        // For loading and saving program settings.
        private Settings _settings = new Settings();
        private ProgramSettings _programSettings;
        private static readonly string _programSettingsFileName = "CenterWindow.xml";

        #endregion Variables de la clase

        /// <summary>
        /// Summary description for frmMain.
        /// </summary>
        public frmMain()
        {
            InitializeComponent();

            // Set form icon
            var path = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (System.IO.File.Exists(path + @"\images\centerwindow.ico")) this.Icon = new Icon(path + @"\images\centerwindow.ico");

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
            _cursorDefault = Cursor.Current;
            _cursorFinder = EmbeddedResources.LoadCursor(EmbeddedResources.Finder);
            _finderHome = EmbeddedResources.LoadImage(EmbeddedResources.FinderHome);
            _finderGone = EmbeddedResources.LoadImage(EmbeddedResources.FinderGone);
            _pictureBox.Image = _finderHome;

            // Establecer los eventos
            _pictureBox.MouseDown += new MouseEventHandler(OnFinderToolMouseDown);

            // Load any saved program settings.
            _programSettings = new ProgramSettings(_programSettingsFileName);
            LoadProgramSettings();
            //this.ClientSize;
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
                    this.frmMain_Close();
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
                Win32.ReleaseCapture();

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
        private void HandleMouseMovements(IntPtr handle= default(IntPtr))
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
                if (_hParentWindow != IntPtr.Zero && _settings.bOnlyParents == true)
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
                    Win32.Rect ventana = new Win32.Rect();
                    Win32.GetWindowRect(hWnd, ref ventana);

                    // handle
                    txtHandle.Text = String.Format("{0}", hWnd.ToInt32().ToString());

                    // class
                    txtClass.Text = Win32.GetClassName(hWnd);
                    
                    // caption
                    txtCaption.Text = Win32.GetWindowText(hWnd);
                    
                    // rect
                    txtRectangle.Text = String.Format("[{0} x {1}], ({2},{3})-({4},{5})", ventana.right - ventana.left, ventana.bottom - ventana.top, ventana.left, ventana.top, ventana.right, ventana.bottom);

                    // highlight the window
                    WindowHighlighter.Highlight(hWnd, _settings.cRectColor, _settings.nRectWidth);
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
            if (_settings.bCenterWindow == true)
            {
                // Obtener las dimensiones de la pantalla
                Win32.Rect pantalla = new Win32.Rect();
                Win32.GetWindowRect(Win32.GetDesktopWindow(), ref pantalla);

                // Obtener las dimensiones de la ventana
                Win32.Rect ventana = new Win32.Rect();
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
            if (_settings.bTransparency == true)
            {
                Win32.SetWindowLong(hWnd,
                    Win32.GWL_EXSTYLE,
                    Win32.GetWindowLongPtr(hWnd, Win32.GWL_EXSTYLE) | (int)Win32.WindowStyles.WS_EX_LAYERED);

                Win32.SetLayeredWindowAttributes(hWnd,
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
            int size = Win32.GetWindowTextLength(hWnd);
            if (size++ > 0 && Win32.IsWindowVisible(hWnd))
            {
                // Retrieves the window's text
                String strWindowText;
                strWindowText = Win32.GetWindowText(hWnd);
                //strWindowModule = Win32.GetClassName(hWnd);
                //strWindowModule = Win32.GetWindowModuleFileName(hWnd);

                // Retrieves the name of the windows's executable
                String strWindowModule;
                IntPtr handle;
                uint uHandle;
                Win32.GetWindowThreadProcessId(hWnd, out uHandle);
                handle = Win32.OpenProcess((uint)(Win32.SECURITY_INFORMATION.PROCESS_QUERY_INFORMATION | Win32.SECURITY_INFORMATION.PROCESS_VM_READ), false, uHandle);
                strWindowModule = Win32.GetModuleBaseName(handle);
                //windowModule = Win32.GetModuleFileNameEx(handle); //Gets the full path
                    /* Gets the same results but using the .NET framework
                    Process p = Process.GetProcessById((int)uHandle);
                    windowModule = p.MainModule.ModuleName.ToString();
                    */

                // Gets additional window info: we are interested in the border width
                Win32.WindowInfo winInfo = new Win32.WindowInfo();
                Win32.GetWindowInfo(hWnd, ref winInfo);

                //if (windowText != "Microsoft Edge" && windowText != "Program Manager")
                ListViewItem item = new ListViewItem();
                if (winInfo.xWindowBorders > 0 && winInfo.xWindowBorders > 0)
                {
                    item = lstWindows.Items.Add(strWindowText);
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
        private void frmMain_Load(object sender, EventArgs e)
        {
            // Set the controls in the main form
            chkCenter.Checked = _settings.bCenterWindow;
            trkTransparency.Value = _settings.nTransparencyValue;
            lblTransparencyValue.Enabled = _settings.bTransparency;
            trkTransparency.Enabled = _settings.bTransparency;
            chkTransparency.Checked = _settings.bTransparency;

            // Creates the fade in animation of the form
            Win32.AnimateWindow(this.Handle, 500, Win32.AnimateWindowFlags.AW_BLEND);
        }

        /// <summary>
        /// Occurs after the form is loaded and shown for the first time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Shown(object sender, EventArgs e)
        {
            // Rellenar el control lstWindows
            this.btnApp_Click(null, null);
        }

        /// <summary>
        /// Occurs when the user closes the form
        /// </summary>
        private void frmMain_Close()
        {
            // Pass the control values (same as load) to the _settings class
            // _settings.bTransparency = chkTransparency.Checked;
            // _settings.bCenterWindow = chkCenter.Checked;
            _settings.nTransparencyValue = (byte) trkTransparency.Value;

            // Save the current program settings.
            this.SaveProgramSettings();

            // Creates the fade out animation of the form
            Win32.AnimateWindow(this.Handle, 500, Win32.AnimateWindowFlags.AW_BLEND | Win32.AnimateWindowFlags.AW_HIDE);
        }

        /// <summary>
        /// Resize en reposition the textboxes and their associated labels
        /// The remaining controls reposition themselves automatically
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Resize(object sender, EventArgs e)
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
        private void OnFinderToolMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.CaptureMouse(true);
        }

        /// <summary>
        /// Fill the lstWindows with all the applications found
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnApp_Click(object sender, EventArgs e)
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
        private void btnAply_Click(object sender, EventArgs e)
        {
            // Activar el control de errores en la llamada a la API
            try
            {
                // Si sólo se pueden modificar las ventanas padre
                if (_hParentWindow != IntPtr.Zero && _settings.bOnlyParents == true)
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

        private void btnOptions_Click(object sender, EventArgs e)
        {
            Options frm = new Options(_settings);
            frm.ShowDialog(this);
            if (frm.DialogResult == DialogResult.OK)
            {
                _settings = frm.Settings;
                return;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trkTransparency_Changed(object sender, EventArgs e)
        {
            int dPorcentaje;
            dPorcentaje = 100 * trkTransparency.Value/trkTransparency.Maximum;      
            lblTransparencyValue.Text = dPorcentaje.ToString () + "%";
        }

        private void chkTransparency_Changed(object sender, EventArgs e)
        {
            trkTransparency.Enabled = chkTransparency.Checked;
            lblTransparencyValue.Enabled = chkTransparency.Checked;
            _settings.bTransparency = chkTransparency.Checked;
        }

        private void chkCenter_CheckedChanged(object sender, EventArgs e)
        {
            _settings.bCenterWindow = chkCenter.Checked;
        }

        private void txtCaption_TextChanged(object sender, EventArgs e)
        {
            // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
            if (((Control)sender).Text == String.Empty)
                ClearSelection();
        }

        private void txtHandle_TextChanged(object sender, EventArgs e)
        {
            // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
            if (((Control)sender).Text == String.Empty)
                ClearSelection();
        }

        private void txtClass_TextChanged(object sender, EventArgs e)
        {
            // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
            if (((Control)sender).Text == String.Empty)
                ClearSelection();
        }

        private void txtRectangle_TextChanged(object sender, EventArgs e)
        {
            // Si se borra el texto, entonces borrar los otros controles y las variables hWnd
            if (((Control)sender).Text == String.Empty)
                ClearSelection();
        }

        #endregion Form controls events

        #region Program settings

        /// <summary>
        /// Loads any saved program settings.
        /// </summary>
        private void LoadProgramSettings()
        {
            // Load the saved window settings and resize the window.
            try
            {
                // Load the saved window settings.
                Int32 left = System.Int32.Parse(_programSettings.GetValue("Window", "Left"));
                Int32 top = System.Int32.Parse(_programSettings.GetValue("Window", "Top"));
                Int32 width = System.Int32.Parse(_programSettings.GetValue("Window", "Width"));
                Int32 height = System.Int32.Parse(_programSettings.GetValue("Window", "Height"));

                // Reposition and resize the window.
                this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                this.DesktopLocation = new Point(left, top);
                this.Size = new Size(width, height);

                // Load saved options.
                _settings.bCenterWindow = Boolean.Parse(_programSettings.GetValue("Options", "Center_window"));
                _settings.bTransparency = Boolean.Parse(_programSettings.GetValue("Options", "Transparency"));
                _settings.nTransparencyValue = Byte.Parse(_programSettings.GetValue("Options", "Transparency_value"));
                _settings.bOnlyParents = Boolean.Parse(_programSettings.GetValue("Options", "Only_parent_windows"));
                _settings.cRectColor = Color.FromArgb(Int32.Parse(_programSettings.GetValue("Options", "Rectangle_color")));
                _settings.nRectWidth = Int32.Parse(_programSettings.GetValue("Options", "Rectangle_width"));
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Saves the current program settings.
        /// </summary>
        private void SaveProgramSettings()
        {
            // Save window settings.      
            _programSettings.SetValue("Window", "Left", this.DesktopLocation.X.ToString());
            _programSettings.SetValue("Window", "Top", this.DesktopLocation.Y.ToString());
            _programSettings.SetValue("Window", "Width", this.Size.Width.ToString());
            _programSettings.SetValue("Window", "Height", this.Size.Height.ToString());

            // Save options.
            _programSettings.SetValue("Options", "Center_window", _settings.bCenterWindow.ToString());
            _programSettings.SetValue("Options", "Transparency", _settings.bTransparency.ToString());
            _programSettings.SetValue("Options", "Transparency_value", _settings.nTransparencyValue.ToString());
            _programSettings.SetValue("Options", "Only_parent_windows", _settings.bOnlyParents.ToString());
            _programSettings.SetValue("Options", "Rectangle_color", _settings.cRectColor.ToArgb().ToString());
            _programSettings.SetValue("Options", "Rectangle_width", _settings.nRectWidth.ToString());

            // Save the program settings.
            _programSettings.Save();
        }


        #endregion Program settings

    }
}