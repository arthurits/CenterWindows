namespace Center_window;

public partial class Options : Form
{
    public ClassSettings Settings { get; private set; } = new();

    // Constructor por defecto
    public Options()
    {
        InitializeComponent();

        // Set form icon
        if (System.IO.File.Exists(@"images\centerwindow.ico")) this.Icon = new Icon(@"images\centerwindow.ico");
    }

    // Overloaded constructor
    public Options(ClassSettings settings)
        :this()
    {
        //_settings = new(settings);
        UpdateControls(settings);
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
        Settings.OnlyParentWnd = chkParent.Checked;
        Settings.RectangleWidth = (Int32)updWidth.Value;
        Settings.RectangleColor = officeColorPicker.Color.ToArgb();
    }

    /// <summary>
    /// Updates the form's controls with values from the settings class
    /// </summary>
    /// <param name="settings">Class containing the values to show on the form's controls</param>
    private void UpdateControls(ClassSettings settings)
    {
        Settings = settings;

        try
        {
            chkParent.Checked = Settings.OnlyParentWnd;
            updWidth.Value = Settings.RectangleWidth;
            officeColorPicker.Color = Color.FromArgb(Settings.RectangleColor);
        }
        catch (Exception)
        {
            using (new CenterWinDialog(this))
                MessageBox.Show(this, "Unexpected error while applying settings.\nPlease report the error to the engineer.", "Settings error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
