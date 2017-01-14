using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
    /// A simple class that is the list of resetting MidiMsgs to be carried from
    /// a BasicMidiChordDef or MidiRestDef to the following BasicMidiChordDef or MidiRestDef.
    /// This class exists so that its WriteSVG(w) function can be called by both
    /// BasicMidiChordDef and MidiRestDef. 
    /// </summary>
    public class CarryMsgs
    {
        public CarryMsgs()
        {
        }

        internal void WriteSVG(XmlWriter w)
        {
            int noteOffsCount = NoteOffsCount;
            if(noteOffsCount > 0)
            {
                w.WriteStartElement("noteOffs");
                foreach(MidiMsg mm in _msgs)
                {
                    if(IsNoteOffMsg(mm))
                    {
                        mm.WriteSVG(w);
                    }
                }
                w.WriteEndElement(); // end of noteOffs
            }
            if(_msgs.Count > noteOffsCount)
            {
                CheckPitchWheelDeviationMsgs();

                w.WriteStartElement("ctlOffs");
                foreach(MidiMsg mm in _msgs)
                {
                    if(IsNoteOffMsg(mm) == false)
                    {
                        mm.WriteSVG(w);
                    }
                }
                w.WriteEndElement(); // end of ctlOffs

            }
        }

        public void Add(MidiMsg msg)
        {
            CheckChannel(msg);
            _msgs.Add(msg);
        }

        public void AddRange(List<MidiMsg> msgs)
        {
            foreach(MidiMsg msg in msgs)
            {
                Add(msg);
            }
        }

        /// <summary>
        /// Checks that all messages have the same channel.
        /// </summary>
        public void Clear()
        {
            _msgs.Clear();
        }

        /// <summary>
        /// Checks that the new msg has the same channel.
        /// </summary>
        private void CheckChannel(MidiMsg msg)
        {
            if(_msgs.Count > 0 && (_msgs[0].Channel != msg.Channel))
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Calls Debug.Assert(false) if the first PitchWheelDeviation Msg
        /// exists and is not followed by three messages that are consistent.
        /// </summary>
        private void CheckPitchWheelDeviationMsgs()
        {
            // A PitchWheelDeviation message group consists of the following four messages:
            // They must all have:
            // a) identical status bytes with high nibble 0xB, low nibble is channel
            // b) data1 values identical to those given here (in the same order),
            // c) the first two data2 bytes must be 0.
            // The CTL_DATA_ENTRY_COARSE data2 value sets the semitones deviation.
            // The CTL_DATA_ENTRY_Fine data2 value sets the cents deviation.
            //< msg s = "0xB0", data1 = "0x65", data2 = "0" /> // CTL_REGISTERED_PARAMETER_COARSE
            //< msg s = "0xB0", data1 = "0x64", data2 = "0" /> // CTL_REGISTERED_PARAMETER_FINE   
            //< msg s = "0xB0", data1 = "0x06", data2 = "2" /> // CTL_DATA_ENTRY_COARSE, 2 semitones
            //< msg s = "0xB0", data1 = "0x26", data2 = "0" /> // CTL_DATA_ENTRY_FINE, 0 cents
            int msg1Index = -1;
            for(int i = 0; i <_msgs.Count; ++i)
            {
                MidiMsg msg = _msgs[i];
                if((msg.Status & 0xF0) == 0xB0 && msg.Data1 == 0x65)
                {
                    msg1Index = i;
                    break;
                }
            }
            if(msg1Index >= 0)
            {
                List<MidiMsg> pwdMsgs = new List<MidiMsg>();
                for(int i = msg1Index; i < _msgs.Count; ++i)
                {
                    pwdMsgs.Add(_msgs[i]);
                }

                if(pwdMsgs.Count < 4)
                {
                    Debug.Assert(false, "Not enough pitchWheel deviation messages");
                }
                else
                {
                    MidiMsg msg1 = pwdMsgs[0];
                    if(msg1.Data2 != 0)
                    {
                        Debug.Assert(false, "Error in first msg.");
                    }
                    MidiMsg msg2 = pwdMsgs[1];
                    if(msg2.Status != msg1.Status || msg2.Data1 != 0x64 || msg2.Data2 != 0)
                    {
                        Debug.Assert(false, "Error in second msg.");
                    }
                    MidiMsg msg3 = pwdMsgs[2];
                    if(msg3.Status != msg1.Status || msg3.Data1 != 0x06)
                    {
                        Debug.Assert(false, "Error in third msg.");
                    }
                    MidiMsg msg4 = pwdMsgs[3];
                    if(msg4.Status != msg1.Status || msg4.Data1 != 0x26)
                    {
                        Debug.Assert(false, "Error in fourth msg.");
                    }
                }
            }
        }

        private bool IsNoteOffMsg(MidiMsg msg)
        {
            int statusHighNibbble = msg.Status & 0xF0;
            if(statusHighNibbble == 0x80 || (statusHighNibbble == 0x90 && msg.Data2 == 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int NoteOffsCount
        {
            get
            {
                int nNoteOffs = 0;
                foreach(MidiMsg msg in _msgs)
                {
                    if(IsNoteOffMsg(msg))
                    {
                        ++nNoteOffs;
                    }
                }
                return nNoteOffs;
            }
        }

        public IReadOnlyList<MidiMsg> Msgs { get { return _msgs; } }
        private List<MidiMsg> _msgs = new List<MidiMsg>();
    }
}