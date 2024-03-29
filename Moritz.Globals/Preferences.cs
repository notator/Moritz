using Moritz.Globals.IODevices;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

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

            #region read prefs
            if(!File.Exists(M.LocalMoritzPreferencesPath))
            {
                LocalMoritzFolderLocation = "C://Documents";
                PreferredOutputDevice = "";

                Save();

                string msg = "A preferences file could not be found at\n" +
                            "\t" + M.LocalMoritzPreferencesPath + ".\n\n" +
                            "A new one has been created with default values.";
                MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            try
            {
                using(XmlReader r = XmlReader.Create(M.LocalMoritzPreferencesPath))
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
            // DeviceCollections.OutputDevices contains all available output devices except VirtualMIDISynth #1,
            // which is reserved for Chrome. 
            foreach(Moritz.Globals.IODevices.OutputDevice netOutputDevice in DeviceCollections.OutputDevices)
            {
                try
                {
                    Sanford.Multimedia.Midi.OutputDevice outputDevice = new Sanford.Multimedia.Midi.OutputDevice(netOutputDevice.ID);
                    MultimediaMidiOutputDevices.Add(netOutputDevice.Name, outputDevice);
                }
                catch
                {
                    // The following MessageBox proved to be more of a nuisance than a help.
                    // The comment it contains is, however, still correct.

                    //MessageBox.Show(netOutputDevice.Name + " is already in use." +
                    //"\n\nMoritz can still be used, but no output device will be available." +
                    //"\n\nIf Moritz needs an output device, close the program that is using" +
                    //"\n" + netOutputDevice.Name + ", restart Moritz, and then restart the other program." +
                    //"\n\n(Currently, Moritz can use all available output devices except VirtualMIDISynth #1," +
                    //"\nwhich is reserved for the Assistant Performer.)", "Information");

                    MultimediaMidiOutputDevices.Clear();
                    success = false;
                    break;
                }
            }

            return success;
        }

        public void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                CloseOutput = true
            }; // not disposable
            using(XmlWriter w = XmlWriter.Create(M.LocalMoritzPreferencesPath, settings))
            {
                w.WriteStartDocument();
                w.WriteComment("file created: " + M.NowString);

                w.WriteStartElement("moritzPreferences");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.OnlineXMLSchemasFolder + @"\moritzPreferences.xsd");

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



        public string LocalMoritzFolderLocation = null;
        public string PreferredOutputDevice = null;

        #region local folders
        //public string LocalMoritzAudioFolder { get { return @"D:\My Work\Programming\Moritz\Moritz\audio"; } }
        //public string LocalMoritzKrystalsFolder	{ get { return @"D:\My Work\Programming\Moritz\Moritz\krystals\krystals"; } }
        //public string LocalMoritzExpansionFieldsFolder { get { return @"D:\My Work\Programming\Moritz\Moritz\krystals\krystals\expansion operators"; } }
        //public string LocalMoritzModulationOperatorsFolder { get { return @"D:\My Work\Programming\Moritz\Moritz\krystals\krystals\modulation operators"; } }
        //public string LocalMoritzScoresFolder { get { return LocalMoritzFolderLocation + @"\Visual Studio\Projects\MyWebsite\james-ingram-act-two\open-source\assistantPerformerTestSite\scores"; } }
        #endregion local folders
        #region online folders
        //public string OnlineXMLSchemasFolder { get { return "https://james-ingram-act-two.de/open-source/XMLSchemas"; } }
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

        public Sanford.Multimedia.Midi.OutputDevice CurrentMultimediaMidiOutputDevice
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
        public Sanford.Multimedia.Midi.OutputDevice GetMidiOutputDevice(string deviceName)
        {
            Debug.Assert(MultimediaMidiOutputDevices.ContainsKey(deviceName));
            return MultimediaMidiOutputDevices[deviceName];
        }

        private Dictionary<string, Sanford.Multimedia.Midi.OutputDevice> MultimediaMidiOutputDevices = new Dictionary<string, Sanford.Multimedia.Midi.OutputDevice>();
        private const int _sysExBufferSize = 1024;
    }
}
