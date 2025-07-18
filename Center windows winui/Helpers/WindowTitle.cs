namespace CenterWindow.Helpers;
internal class WindowTitle
{
    /// <summary>
    /// Sets the window title
    /// </summary>
    /// <param name="windowText">Default (base) text</param>
    /// <param name="strFileName">Suffix to be added to <paramref name="windowText"/></param>
    /// <param name="strTitleUnion">Separator between <paramref name="windowText"/> and <paramref name="strFileName"/>. It defaults to "—"</param>
    /// If <see langword="null"/>, no string is added.
    /// If <see cref="string.Empty"/>, the current added text is mantained.
    /// Other values are added to the default title.</param>
    /// <returns>The composed string to be displayed in the window title</returns>
    public static string SetWindowTitle(string windowText, string? strFileName = null, string strTitleUnion = "—")
    {
        //https://xamlbrewer.wordpress.com/category/winui-3/

        var strText = string.Empty;
        var strSep = $" {strTitleUnion} ";
        if (strFileName is not null)
        {
            if (strFileName.Length > 0)
            {
                strText = $"{strSep}{strFileName}";
            }
            else
            {
                var index = windowText.IndexOf(strSep) > -1 ? windowText.IndexOf(strSep) : windowText.Length;
                strText = windowText[index..];
            }
        }

        return windowText + strText;
    }
}
