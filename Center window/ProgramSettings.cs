using System;
using System.Drawing;
using System.Xml;

namespace Center_window
{
	/// <summary>
	/// Summary description for ProgramSettings.
	/// </summary>
	public class ProgramSettings
	{
		// The file name used to save the program settings.
		private readonly string fileName;

		// An Xml document used to store and save program settings.
		private readonly XmlDocument document;

		public ProgramSettings(string fileName)
		{
			//
			// TODO: Add constructor logic here
			//

			// Assign the file name.
			this.fileName = fileName;

			// If the file already exists, load it. Otherwise, create a new document.
			this.document = new XmlDocument();
			try
			{
				document.Load(this.fileName);
			}
			catch (Exception)
			{
				// Create a new XML document and set the root node.
				document = new XmlDocument();
				document.AppendChild(document.CreateElement("ProgramSettings"));
			}
		}

		//
		// Saves the settings XML file.
		//
		public void Save()
		{
			this.document.Save(this.fileName);
		}

		//
		// Reads a value from the settings file.
		//
		public string GetValue(string section, string name)
		{
			try
			{
				return this.document.DocumentElement.SelectSingleNode(section + "/" + name).InnerText;
			}
			catch (Exception)
			{
				return "";
			}
		}

		//
		// Writes a value to the settings file.
		//
		public void SetValue(string section, string name, string value)
		{
			// If the section does not exist, create it.
			XmlNode sectionNode = this.document.DocumentElement.SelectSingleNode(section);
			if (sectionNode == null)
				sectionNode = this.document.DocumentElement.AppendChild(this.document.CreateElement(section));
			// If the node does not exist, create it.
			XmlNode node = sectionNode.SelectSingleNode(name);
			if (node == null)
				node = sectionNode.AppendChild(this.document.CreateElement(name));

			// Set the value.
			node.InnerText = value;
		}
	}
}
