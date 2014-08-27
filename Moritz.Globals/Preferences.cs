using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;

using Moritz.Globals.IODevices;

namespace Moritz.Globals
{
	public sealed class Preferences : IDisposable
	{
        public Preferences(StringBuilder localMoritzFolder, StringBuilder localMoritzPreferencesPath, StringBuilder localMoritzKrystalsFolder, 
            StringBuilder localMoritzExpansionFieldsFolder, StringBuilder localMoritzModulationOperatorsFolder, 
            StringBuilder localMoritzScoresFolder, StringBuilder onlineMoritzFolder, StringBuilder onlineMoritzAudioFolder,
            StringBuilder onlineXMLSchemasFolder)
        {
            #region constant files and folders
            // These values are displayed as reminders in the preferences dialog.
            LocalMoritzFolder = localMoritzFolder.ToString();
            LocalMoritzPreferencesPath = localMoritzPreferencesPath.ToString();
            LocalMoritzKrystalsFolder = localMoritzKrystalsFolder.ToString();
            LocalMoritzExpansionFieldsFolder = localMoritzExpansionFieldsFolder.ToString();
            LocalMoritzModulationOperatorsFolder = localMoritzModulationOperatorsFolder.ToString();
            LocalMoritzScoresFolder = localMoritzScoresFolder.ToString();
            OnlineMoritzFolder = onlineMoritzFolder.ToString();
            OnlineMoritzAudioFolder = onlineMoritzAudioFolder.ToString();
            OnlineXMLSchemasFolder = onlineXMLSchemasFolder.ToString();
            #endregion

            #region read prefs
            if(!File.Exists(LocalMoritzPreferencesPath))
            {
                string msg = "A preferences file could not be found at\n" +
                "\t" + LocalMoritzPreferencesPath + ".\n\n" +
                "A new one has been created with default values.";

                M.CreateDirectoryIfItDoesNotExist(LocalMoritzFolder);

                // default values.
                PreferredInputDevice = "";
                PreferredOutputDevice = "";

                Save();
            }
  
			try
			{
				using(XmlReader r = XmlReader.Create(LocalMoritzPreferencesPath))
				{
					M.ReadToXmlElementTag(r, "moritzPreferences"); // check that this is a moritz preferences file

                    M.ReadToXmlElementTag(r, "preferredInputDevice");
                    PreferredInputDevice = r.ReadElementContentAsString();
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

            if(CaptureInputDevices() && CaptureOutputDevices())
            {
                if((!String.IsNullOrEmpty(PreferredInputDevice)) && MultimediaMidiInputDevices.ContainsKey(PreferredInputDevice) == false)
                {
                    string message = "Can't find the " + PreferredInputDevice + ".\n\n" +
                        "To use it, quit, turn it on, and restart.";
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    CurrentInputDeviceName = PreferredInputDevice;
                }

                if((!String.IsNullOrEmpty(PreferredOutputDevice)) && MultimediaMidiOutputDevices.ContainsKey(PreferredOutputDevice) == false)
                {
                    string message = "Can't find the " + PreferredOutputDevice + ".\n\n" +
                        "To use it, quit, turn it on, and restart.";
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    CurrentOutputDeviceName = PreferredOutputDevice;
                }
            }
        }

        private bool CaptureInputDevices()
        {
            bool success = true;
            foreach(Moritz.Globals.IODevices.InputDevice netInputDevice in DeviceCollections.InputDevices)
            {
                try
                {
                    Multimedia.Midi.InputDevice inputDevice = new Multimedia.Midi.InputDevice(netInputDevice.ID);
                    inputDevice.AddSysExBuffer(_sysExBufferSize);
                    inputDevice.AddSysExBuffer(_sysExBufferSize);
                    MultimediaMidiInputDevices.Add(netInputDevice.Name, inputDevice);
                }
                catch
                {
                    MultimediaMidiInputDevices.Clear();
                    success = false;
                    break;
                }
            }
            return success;
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
				w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.OnlineXMLSchemasFolder + "/moritzPreferences.xsd");

                w.WriteStartElement("preferredInputDevice");
                w.WriteString(PreferredInputDevice);
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
                foreach(string key in MultimediaMidiInputDevices.Keys)
                {
                    MultimediaMidiInputDevices[key].Dispose();
                }
                MultimediaMidiInputDevices.Clear();

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

        #region fixed folders and paths

        public readonly string LocalMoritzFolder;
        public readonly string LocalMoritzPreferencesPath;
        public readonly string LocalMoritzKrystalsFolder;
        public readonly string LocalMoritzExpansionFieldsFolder;
        public readonly string LocalMoritzModulationOperatorsFolder;
        public readonly string LocalMoritzScoresFolder;
        public readonly string OnlineMoritzFolder;
        public readonly string OnlineMoritzAudioFolder;
        public readonly string OnlineXMLSchemasFolder;

        #endregion fixed folders and paths

        #region devices
        public string PreferredInputDevice = null;
        public string PreferredOutputDevice = null;

        /// <summary>
        /// The following fields are not saved in the preferences file
        /// </summary>
        public string CurrentInputDeviceName = null;
        public string CurrentOutputDeviceName = null;

        public List<string> AvailableMultimediaMidiInputDeviceNames
        {
            get
            {
                List<string> names = new List<string>();
                foreach(string key in MultimediaMidiInputDevices.Keys)
                    names.Add(key);
                return names;
            }
        }
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
        public Multimedia.Midi.InputDevice CurrentMultimediaMidiInputDevice
        {
            get 
            {
                if(MultimediaMidiInputDevices.ContainsKey(CurrentInputDeviceName))
                {
                    return MultimediaMidiInputDevices[CurrentInputDeviceName];
                }
                else
                {
                    throw new ApplicationException("Unknown Input Device: " + CurrentInputDeviceName);
                }
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

        private Dictionary<string, Multimedia.Midi.InputDevice> MultimediaMidiInputDevices = new Dictionary<string, Multimedia.Midi.InputDevice>();
        private Dictionary<string, Multimedia.Midi.OutputDevice> MultimediaMidiOutputDevices = new Dictionary<string, Multimedia.Midi.OutputDevice>();
        private const int _sysExBufferSize = 1024;
        #endregion devices
    }
}
