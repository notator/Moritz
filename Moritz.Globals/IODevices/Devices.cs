using System;
using System.Security.AccessControl;
using Microsoft.Win32;

namespace Moritz.Globals.IODevices
{
    /// <summary>
    /// A MIDI output device.
    /// </summary>
    public class OutputDevice
    {
        private const string DefaultMidiOutDevice = @"Software\Microsoft\ActiveMovie\devenum\{4EFE2452-168A-11D1-BC76-00C04FB9453B}\Default MidiOut Device";
        private const string MidiOutId = "MidiOutId";

        /// <summary>
        /// Creates a new MIDI output device instance.
        /// </summary>
        /// <param name="id">The device's ID.</param>
        /// <param name="caps">The device's capabilities.</param>
        internal OutputDevice(Int32 id, MIDIOUTCAPS caps)
        {
            ID = id;
            Name = caps.szPname;
        }

        /// <summary>
        /// Gets the ID of this device.
        /// </summary>
        public Int32 ID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Name of this device.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        public static Int32 DefaultDeviceID
        {
            get
            {
                RegistryKey defaultKey = null;
                try
                {
                    defaultKey = Registry.CurrentUser.OpenSubKey(DefaultMidiOutDevice);
                    return (int)defaultKey.GetValue(MidiOutId);
                }
                catch
                {
                    return -2;
                }
                finally
                {
                    if (defaultKey != null)
                    {
                        defaultKey.Close();
                    }
                }
            }
            set
            {
                RegistryKey defaultKey = null;
                try
                {
                    defaultKey = Registry.CurrentUser.OpenSubKey(DefaultMidiOutDevice,
                        RegistryKeyPermissionCheck.ReadWriteSubTree,
                        RegistryRights.SetValue);
                    defaultKey.SetValue(MidiOutId, value);
                }
                finally
                {
                    if (defaultKey != null)
                    {
                        defaultKey.Close();
                    }
                }
            }
        }
    }

    /********************************************************************/
    /*
     * Below here added by j.i. May 2009
     * 
     */

    /// <summary>
    /// A MIDI input device.
    /// </summary>
    public class InputDevice
    {
        private const string DefaultMidiInDevice = @"";
        private const string MidiInId = "MidiInId";

        /// <summary>
        /// Creates a new MIDI input device instance.
        /// </summary>
        /// <param name="id">The device's ID.</param>
        /// <param name="caps">The device's capabilities.</param>
        internal InputDevice(Int32 id, MIDIINCAPS caps)
        {
            ID = id;
            Name = caps.szPname;
        }

        /// <summary>
        /// Gets the ID of this device.
        /// </summary>
        public Int32 ID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Name of this device.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        public static Int32 DefaultDeviceID
        {
            get
            {
                RegistryKey defaultKey = null;
                try
                {
                    defaultKey = Registry.CurrentUser.OpenSubKey(DefaultMidiInDevice);
                    return (int)defaultKey.GetValue(MidiInId);
                }
                catch
                {
                    return -2;
                }
                finally
                {
                    if (defaultKey != null)
                    {
                        defaultKey.Close();
                    }
                }
            }
            set
            {
                RegistryKey defaultKey = null;
                try
                {
                    defaultKey = Registry.CurrentUser.OpenSubKey(DefaultMidiInDevice,
                        RegistryKeyPermissionCheck.ReadWriteSubTree,
                        RegistryRights.SetValue);
                    defaultKey.SetValue(MidiInId, value);
                }
                finally
                {
                    if (defaultKey != null)
                    {
                        defaultKey.Close();
                    }
                }
            }
        }
    }
}