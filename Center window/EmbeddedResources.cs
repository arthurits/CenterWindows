using System.Diagnostics;
using System.Reflection;

namespace Center_window;

/// <summary>
/// Load graphics resources from disk
/// </summary>
public class EmbeddedResources
{
    public const string AppLogo = @"images\centerwindow.ico";
    public const string FinderHome = "Center_window.images.FinderHome.bmp";
	public const string FinderGone = "Center_window.images.FinderGone.bmp";
	public const string Finder = "Center_window.images.Finder.cur";

	/// <summary>
	/// Loads an image from an embbedded resource
	/// </summary>
	/// <param name="Name"></param>
	/// <returns></returns>
	public static Image LoadImage(string Name)
	{
		try
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Name);
			return Image.FromStream(stream);
		}
		catch (Exception ex)
		{
			MessageBox.Show(
				$"Unexpected error while loading the {Name} image.{Environment.NewLine}{ex.Message}",
				"Error loading image",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}
		return null;
	}

	/// <summary>
	/// Loads a cursor from an embedded resource
	/// </summary>
	/// <param name="Name"></param>
	/// <returns></returns>
	public static Cursor LoadCursor(string Name)
	{
		try
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Name);
			return new Cursor(stream);
        }
		catch(Exception ex)
		{
			MessageBox.Show(
				$"Unexpected error while loading the {Name} cursor.{Environment.NewLine}{ex.Message}",
				"Error loading cursor",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}
		return null;
	}

    /// <summary>
    /// Loads a graphics resource from a disk location
    /// </summary>
    /// <typeparam name="T">Type of resource to be loaded</typeparam>
    /// <param name="fileName">File name (absolute or relative to the working directory) to load resource from</param>
    /// <returns>The graphics resource</returns>
    public static T? Load<T>(string fileName)
    {
        T? resource = default;
        try
        {
            if (File.Exists(fileName))
            {
                if (typeof(T).Equals(typeof(System.Drawing.Image)))
                    resource = (T)(object)Image.FromFile(fileName);
                else if (typeof(T).Equals(typeof(Icon)))
                    resource = (T)(object)new Icon(fileName);
                else if (typeof(T).Equals(typeof(Cursor)))
                    resource = (T)(object)new Cursor(fileName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unexpected error while loading the {fileName} graphics resource.{Environment.NewLine}{ex.Message}",
                "Loading error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        return resource;
    }

}
