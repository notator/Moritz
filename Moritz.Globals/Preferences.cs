using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;
using System.Diagnostics;

using Moritz.Globals.IODevices;

namespace Moritz.Globals
{
	public sealed class Preferences : IDisposable
	{
        public Preferences()
        {
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

			string moritzAppDataFolder = appData + @"\Moritz";
			// C:\Users\James\AppData\Roaming\Moritz
			M.CreateDirectoryIfItDoesNotExist(moritzAppDataFolder);

			LocalMoritzPreferencesPath = moritzAppDataFolder + @"\Preferences.mzpf";
            #region read prefs
			if(!File.Exists(LocalMoritzPreferencesPath))
            {
				LocalMoritzFolderLocation = "C://Documents";
                PreferredOutputDevice = "";

                Save();

				string msg = "A preferences file could not be found at\n" +
							"\t" + LocalMoritzPreferencesPath + ".\n\n" +
							"A new one has been created with default values.";
				MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
  
			try
			{
				using(XmlReader r = XmlReader.Create(LocalMoritzPreferencesPath))
				{
					M.ReadToXmlElementTag(r, "moritzPreferences"); // check that this is a moritz preferences file

					M.ReadToXmlElementTag(r, "localMoritzFolderLocation");
					LocalMoritzFolderLocation = r.ReadElementContentAsString();
                    M.ReadToXmlElementTag(r, "preferredOutputDevice");
                    PreferredOutputDevice = r.ReadElementContentAsString();
				}
			}
			catch
			{
                string msg = "Error reading preferences file"; 
				MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion

            if(CaptureOutputDevices())
            {
                if((!String.IsNullOrEmpty(PreferredOutputDevice)) && MultimediaMidiOutputDevices.ContainsKey(PreferredOutputDevice) == false)
                {
                    string message = "Can't find the " + PreferredOutputDevice + ".\n\n" +
                        "Check the preferences.";
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    CurrentOutputDeviceName = PreferredOutputDevice;
                }
            }
        }

        private bool CaptureOutputDevices()
        {
            bool success = true;
            foreach(Moritz.Globals.IODevices.OutputDevice netOutputDevice in DeviceCollections.OutputDevices)
            {
                try
                {
                    Multimedia.Midi.OutputDevice outputDevice = new Multimedia.Midi.OutputDevice(netOutputDevice.ID);
                    MultimediaMidiOutputDevices.Add(netOutputDevice.Name, outputDevice);
                }
                catch
                {
                    MultimediaMidiOutputDevices.Clear();
                    success = false;
                    break;
                }
            }
            return success;
        }

		public void Save()
		{
			XmlWriterSettings settings = new XmlWriterSettings(); // not disposable
			settings.Indent = true;
			settings.IndentChars = ("\t");
			settings.CloseOutput = true;
			using(XmlWriter w = XmlWriter.Create(LocalMoritzPreferencesPath, settings))
			{
				w.WriteStartDocument();
				w.WriteComment("file created: " + M.NowString);

				w.WriteStartElement("moritzPreferences");
				w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
				w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, OnlineXMLSchemasFolder + @"\moritzPreferences.xsd");

				w.WriteStartElement("localMoritzFolderLocation");
				w.WriteString(LocalMoritzFolderLocation);
				w.WriteEndElement();

                w.WriteStartElement("preferredOutputDevice");
                w.WriteString(PreferredOutputDevice);
                w.WriteEndElement();

				w.WriteEndElement(); // closes the moritzPreferences element

				w.Close(); // close unnecessary because of the using statement?
			}
		}

        #region IDisposable pattern
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if(!disposed)
			{
                foreach(string key in MultimediaMidiOutputDevices.Keys)
                {
                    MultimediaMidiOutputDevices[key].Dispose();
                }
                MultimediaMidiOutputDevices.Clear();

				if(disposing)
				{
 				}
			}
			disposed = true;
			//base.Dispose(disposing);
		}

		private bool disposed = false;

		#endregion IDisposable pattern

		public readonly string LocalMoritzPreferencesPath;
		
		public string LocalMoritzFolderLocation = null;
		public string PreferredOutputDevice = null;

		#region folders in the LocalMoritzFolder
		public string LocalMoritzAudioFolder { get { return LocalMoritzFolderLocation + @"\Moritz\audio"; } }
		public string LocalMoritzKrystalsFolder	{ get { return LocalMoritzFolderLocation + @"\Moritz\krystals\krystals"; } }
		public string LocalMoritzExpansionFieldsFolder { get { return LocalMoritzFolderLocation + @"\Moritz\krystals\expansion operators"; } }
		public string LocalMoritzModulationOperatorsFolder { get { return LocalMoritzFolderLocation + @"\Moritz\krystals\modulation operators"; } }
		public string LocalMoritzScoresFolder { get { return LocalMoritzFolderLocation + @"\Visual Studio\Projects\MyWebsite\james-ingram-act-two\open-source\assistantPerformerTestSite\scores"; } }
		#endregion folders in the LocalMoritzFolder
		#region online folders
		public string OnlineXMLSchemasFolder { get { return "http://james-ingram-act-two.de/open-source/XMLSchemas"; } }
		#endregion online folders

		/// <summary>
        /// The following field is not saved in the preferences file
        /// </summary>
        public string CurrentOutputDeviceName = null;

        public List<string> AvailableMultimediaMidiOutputDeviceNames
        {
            get
            {
                List<string> names = new List<string>();
                foreach(string key in MultimediaMidiOutputDevices.Keys)
                    names.Add(key);
                return names;
            }
        }

        public Multimedia.Midi.OutputDevice CurrentMultimediaMidiOutputDevice
        {
            get 
            {
                if(MultimediaMidiOutputDevices.Count == 0 || String.IsNullOrEmpty(CurrentOutputDeviceName))
                {
                    return null;
                }
                else if(MultimediaMidiOutputDevices.ContainsKey(CurrentOutputDeviceName))
                {
                    return MultimediaMidiOutputDevices[CurrentOutputDeviceName];
                }
                else
                {
                    throw new ApplicationException("Unknown Output Device: " + CurrentOutputDeviceName);
                }
            }
        }
        public Multimedia.Midi.OutputDevice GetMidiOutputDevice(string deviceName)
        {
            Debug.Assert(MultimediaMidiOutputDevices.ContainsKey(deviceName));
            return MultimediaMidiOutputDevices[deviceName];
        }

        private Dictionary<string, Multimedia.Midi.OutputDevice> MultimediaMidiOutputDevices = new Dictionary<string, Multimedia.Midi.OutputDevice>();
        private const int _sysExBufferSize = 1024;
    }
}
