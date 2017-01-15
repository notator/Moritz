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

        #region List functions
        /// <summary>
        /// Checks that all messages have the same channel.
        /// </summary>
        public void Add(MidiMsg msg)
        {
            CheckChannel(msg);
            _msgs.Add(msg);
        }

        /// <summary>
        /// Checks that all messages have the same channel.
        /// </summary>
        public void AddRange(List<MidiMsg> msgs)
        {
            foreach(MidiMsg msg in msgs)
            {
                Add(msg);
            }
        }

        public void Clear()
        {
            _msgs.Clear();
        }

        public int Count { get { return _msgs.Count; } }
        #endregion

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
        /// Calls Debug.Assert(false) if REGISTERED_PARAMETER_COARSE or REGISTERED_PARAMETER_FINE
        /// are set to change the pitchWheel deviation, but are not followed by a corresponding message
        /// that actually sets the value.
        /// N.B. Moritz currently only sends the COARSE pair of messages, but I've left the check for
        /// the FINE pair here, in case that changes.
        /// </summary>
        private void CheckPitchWheelDeviationMsgs()
        {
            // REGISTERED_PARAMETER_COARSE is set to change the pitchWheel deviation using the following messsage:
            // <msg s = "0xB0", data1 = "0x65", data2 = "0" />
            // REGISTERED_PARAMETER_FINE is set to change the pitchWheel deviation using the following messsage:
            // <msg s = "0xB0", data1 = "0x64", data2 = "0" />
            // Following the above messages, the following message uses DATA_ENTRY_COARSE to set the COARSE (semitones) deviation to 2
            // <msg s = "0xB0", data1 = "0x06", data2 = "2" /> // data2 = semitones deviation
            // and the following message uses CTL_DATA_ENTRY_FINE to set the FINE (cents) deviation to 0
            // <msg s = "0xB0", data1 = "0x26", data2 = "0" /> // data2 = cents deviation  
            int coarseMsg1Index = -1;
            int fineMsg1Index = -1;
            for(int i = 0; i <_msgs.Count; ++i)
            {
                MidiMsg msg = _msgs[i];
                if((msg.Status & 0xF0) == 0xB0)
                {
                    if(msg.Data1 == 0x65)
                    {
                        coarseMsg1Index = i;
                    }
                    else if(msg.Data1 == 0x64)
                    {
                        fineMsg1Index = i;
                    }
                    break;
                }
            }
            if((coarseMsg1Index >= 0 && _msgs[coarseMsg1Index].Data2 != 0)
            || (fineMsg1Index >= 0 && _msgs[fineMsg1Index].Data2 != 0))
            {
                Debug.Assert(false, "Data2 must be zero!");
            }
            if(coarseMsg1Index >= 0)
            {
                if(FindControlMsg(coarseMsg1Index, 0x06) == false)
                {
                    Debug.Assert(false, "Can't find coarseMsg2!");
                }
            }
            if(fineMsg1Index >= 0)
            {
                if(FindControlMsg(fineMsg1Index, 0x26) == false)
                {
                    Debug.Assert(false, "Can't find fineMsg2!");
                }
            }
        }

        private bool FindControlMsg(int msg1Index, int data2)
        {
            bool found = false;
            if(msg1Index == _msgs.Count - 1)
            {
                Debug.Assert(false, "msg1 can't be the lst message in the list!");
            }
            int msg1Status = _msgs[msg1Index].Status;
            for(int i = msg1Index + 1; i < _msgs.Count; ++i)
            {
                MidiMsg msg = _msgs[i];
                if(msg.Status == msg1Status && msg.Data2 == data2)
                {
                    found = true;
                    break;
                }
            }
            return found;
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