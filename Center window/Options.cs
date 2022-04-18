namespace Center_window;

public partial class Options : Form
{
    // Definición de variables
    private Settings _settings;

    // Public property
    public Settings Settings => _settings;

    // Constructor por defecto
    public Options()
    {
        InitializeComponent();

        // Set form icon
        var path = System.IO.Path.GetDirectoryName(Environment.ProcessPath);
        if (System.IO.File.Exists(path + @"\images\centerwindow.ico")) this.Icon = new Icon(path + @"\images\centerwindow.ico");
    }

    // Overloaded constructor
    public Options(Settings settings):this()
    {
        _settings = new Settings(settings);
    }

    private void Options_Load(object sender, EventArgs e)
    {
        chkParent.Checked = _settings.bOnlyParents;
        updWidth.Value = _settings.nRectWidth;
        officeColorPicker.Color = _settings.cRectColor;
    }

    private void Options_FormClosing(object sender, FormClosingEventArgs e)
    {
        _settings.bOnlyParents = chkParent.Checked;
        _settings.nRectWidth = (Int32)updWidth.Value;
        _settings.cRectColor = officeColorPicker.Color;
    }
}
