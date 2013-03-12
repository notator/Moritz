using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using Moritz.Globals.IODevices;

namespace Moritz.Globals
{
	public sealed class Preferences : IDisposable
	{
        public Preferences(string moritzFolder)
        {
            _preferencesPath = moritzFolder + @"\Preferences.mzpf";

            #region read prefs
            if(!File.Exists(_preferencesPath))
            {
                string msg = "A preferences file could not be found at\n" +
                "\t" + _preferencesPath + ".\n\n" +
                "A new one has been created with default values.";

                M.CreateDirectoryIfItDoesNotExist(moritzFolder);

                // default values.
                _localUserFolder = moritzFolder;
                _onlineUserFolder = "";
                PreferredInputDevice = "";
                PreferredOutputDevice = "";

                Save();
            }
  
			try
			{
				using(XmlReader r = XmlReader.Create(_preferencesPath))
				{
					M.ReadToXmlElementTag(r, "moritzPreferences"); // check that this is a moritz preferences file

                    M.ReadToXmlElementTag(r, "localUserFolder");
                    LocalUserFolder = r.ReadElementContentAsString();

                    M.ReadToXmlElementTag(r, "onlineUserFolder");
                    OnlineUserFolder = r.ReadElementContentAsString();

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

            try
            {
                CaptureInputDevices();
                CaptureOutputDevices();
            }
            catch
            {
                string msg = "Cannot capture MIDI devices.\n" +
                             "One or more are probably being used by another program.";
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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

        private void CaptureInputDevices()
        {
            foreach(Moritz.Globals.IODevices.InputDevice netInputDevice in DeviceCollections.InputDevices)
            {
                Multimedia.Midi.InputDevice inputDevice = new Multimedia.Midi.InputDevice(netInputDevice.ID);
                inputDevice.AddSysExBuffer(_sysExBufferSize);
                inputDevice.AddSysExBuffer(_sysExBufferSize);
                MultimediaMidiInputDevices.Add(netInputDevice.Name, inputDevice);
            }
        }
        private void CaptureOutputDevices()
        {
            foreach(Moritz.Globals.IODevices.OutputDevice netOutputDevice in DeviceCollections.OutputDevices)
            {
                Multimedia.Midi.OutputDevice outputDevice = new Multimedia.Midi.OutputDevice(netOutputDevice.ID);
                MultimediaMidiOutputDevices.Add(netOutputDevice.Name, outputDevice);
            }
        }

		public void Save()
		{
			XmlWriterSettings settings = new XmlWriterSettings(); // not disposable
			settings.Indent = true;
			settings.IndentChars = ("\t");
			settings.CloseOutput = true;
			using(XmlWriter w = XmlWriter.Create(_preferencesPath, settings))
			{
				w.WriteStartDocument();
				w.WriteComment("file created: " + M.NowString);

				w.WriteStartElement("moritzPreferences");
				w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
				w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.MoritzXMLSchemasFolder + "/moritzPreferences.xsd");

                w.WriteStartElement("localUserFolder");
                w.WriteString(_localUserFolder);
                w.WriteEndElement();

                w.WriteStartElement("onlineUserFolder");
                w.WriteString(_onlineUserFolder);
                w.WriteEndElement();

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

        #region folders and paths
        public string OnlineUserFolder
        {
            set 
            { 
                _onlineUserFolder = value;
                if(!String.IsNullOrEmpty(_onlineUserFolder) && _onlineUserFolder[_onlineUserFolder.Length - 1] == '/')
                {
                    _onlineUserFolder = _onlineUserFolder.Remove(_onlineUserFolder.Length - 1);
                }
            }
            get 
            {
                if(String.IsNullOrEmpty(_onlineUserFolder))
                    return "";
                else
                    return _onlineUserFolder; 
            }
        }

        public string OnlineUserAudioFolder
        {
            get 
            {
                if(String.IsNullOrEmpty(_onlineUserFolder))
                    return "";
                else
                    return _onlineUserFolder + @"/audio"; 
            }
        }

        public string LocalUserFolder
        {
            set { _localUserFolder = value; }
            get { return _localUserFolder; }
        }
        public string LocalKrystalsFolder
        {
            get { return _localUserFolder + @"\krystals\krystals"; }
        }
        public string LocalExpansionFieldsFolder
        {
            get { return _localUserFolder + @"\krystals\expansion operators"; }
        }
        public string LocalModulationOperatorsFolder
        {
            get { return _localUserFolder + @"\krystals\modulation operators"; }
        }
        public string LocalScoresRootFolder
        {
            get { return _localUserFolder + @"\scores"; }
        }

        public string PreferencesPath
        {
            get { return _preferencesPath; }
        }

        // User files can be put anywhere the user likes (so not readonly).
        private string _onlineUserFolder;
        private string _localUserFolder;
        private readonly string _preferencesPath;
        #endregion folders

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
                if(MultimediaMidiOutputDevices.ContainsKey(CurrentOutputDeviceName))
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
