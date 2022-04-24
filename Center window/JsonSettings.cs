using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Center_window;

partial class FrmMain
{
    /// <summary>
    /// Loads all settings from file _sett.FileName into class instance _settings
    /// Shows MessageBox error if unsuccessful
    /// </summary>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool LoadProgramSettingsJSON()
    {
        bool result = false;
        try
        {
            var jsonString = File.ReadAllText(Settings.FileName);
            Settings = JsonSerializer.Deserialize<ClassSettings>(jsonString) ?? Settings;
            result = true;
        }
        catch (FileNotFoundException)
        {
        }
        catch (Exception ex)
        {
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(this,
                    StringsRM.GetString("strErrorDeserialize", Settings.AppCulture) ?? $"Error loading settings file.\n\n{ex.Message}\n\nDefault values will be used instead.",
                    StringsRM.GetString("strErrorDeserializeTitle", Settings.AppCulture) ?? "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        return result;
    }

    /// <summary>
    /// Saves data from class instance _sett into _sett.FileName
    /// </summary>
    private void SaveProgramSettingsJSON()
    {
        Settings.WindowLeft = DesktopLocation.X;
        Settings.WindowTop = DesktopLocation.Y;
        Settings.WindowWidth = ClientSize.Width;
        Settings.WindowHeight = ClientSize.Height;

        Settings.TransparencyValue = Convert.ToByte(trkTransparency.Value);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var jsonString = JsonSerializer.Serialize(Settings, options);
        File.WriteAllText(Settings.FileName, jsonString);
    }

    /// <summary>
    /// Update UI with settings
    /// </summary>
    /// <param name="WindowSettings"><see langword="True"/> if the window position and size should be applied. <see langword="False"/> if omitted</param>
    private void ApplySettingsJSON(bool WindowPosition = false)
    {
        if (WindowPosition)
        {
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.DesktopLocation = new Point(Settings.WindowLeft, Settings.WindowTop);
            this.ClientSize = new Size(Settings.WindowWidth, Settings.WindowHeight);
        }

        this.chkCenter.Checked = Settings.CenterWindow;
        this.chkTransparency.Checked= Settings.Transparency;
        this.trkTransparency.Value = Settings.TransparencyValue;
        this.lblTransparencyValue.Enabled = Settings.Transparency;
        this.trkTransparency.Enabled = Settings.Transparency;
    }
}