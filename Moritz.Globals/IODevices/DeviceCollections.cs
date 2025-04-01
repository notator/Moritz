using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Moritz.Globals.IODevices
{
    public static class DeviceCollections
    {
        private static IList<OutputDevice> outputDevices;

        /// <summary>
        /// Gets a list of all MIDI output devices.
        /// </summary>
        public static IList<OutputDevice> OutputDevices
        {
            get
            {
                if(outputDevices == null || outputDevices.Count == 0)
                {
                    LoadOutputDevices();
                }
                return outputDevices;
            }
        }

        /// <summary>
        /// Loads/reloads all devices except VirtualMIDISynth #1 (which is reserved for the Assistant Performer).
        /// </summary>
        public static void LoadOutputDevices()
        {
            outputDevices = null;
            List<OutputDevice> devices = new List<OutputDevice>();
            UInt32 numberOfDevices = Functions.midiOutGetNumDevs();

            if(numberOfDevices > 0)
            {
                for(Int32 i = 0; i < numberOfDevices; i++)
                {
                    MIDIOUTCAPS caps = new MIDIOUTCAPS();
                    uint returnValue = Functions.midiOutGetDevCaps(i, ref caps, (UInt32)Marshal.SizeOf(caps));
                    M.Assert(returnValue == Constants.MMSYSERR_NOERROR);
                    if(string.Compare(caps.szPname, "VirtualMIDISynth #1") != 0)
                    {
                        devices.Add(new OutputDevice(i, caps));
                    }
                }
            }

            outputDevices = devices.AsReadOnly();
        }

        /********************************************************************/
        /*
         * Below here added by j.i. May 2009 
         */

        private static IList<InputDevice> inputDevices;

        /// <summary>
        /// Gets a list of all MIDI input devices.
        /// </summary>
        public static IList<InputDevice> InputDevices
        {
            get
            {
                if(inputDevices == null || inputDevices.Count == 0)
                {
                    LoadInputDevices();
                }
                return inputDevices;
            }
        }

        /// <summary>
        /// Loads/reloads the MIDI input devices
        /// </summary>
        public static void LoadInputDevices()
        {
            inputDevices = null;
            List<InputDevice> devices = new List<InputDevice>();
            UInt32 numberOfInDevices = Functions.MidiInGetNumDevs();

            if(numberOfInDevices > 0)
            {
                for(Int32 i = 0; i < numberOfInDevices; i++)
                {
                    MIDIINCAPS caps = new MIDIINCAPS();
                    if(Functions.MidiInGetDevCaps(i, ref caps, (UInt32)Marshal.SizeOf(caps)) == Constants.MMSYSERR_NOERROR)
                    {
                        devices.Add(new InputDevice(i, caps));
                    }
                }
            }
            inputDevices = devices.AsReadOnly();
        }


    }
}
