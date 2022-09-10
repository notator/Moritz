using Moritz.Globals;

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Moritz.Spec
{
    /// <summary>
    /// A simple class that carries the contoller states and a list of noteOffs from one moment to the next.
    /// This class's WriteSVG(w) function is called by both BasicMidiChordDef and MidiRestDef.
    /// There is one CarryMsgs object per channel.
    /// </summary>
    public class CarryMsgs
    {
        public CarryMsgs()
        {
        }

        /// <summary>
        /// Writes NoteOff messages carried from a previous moment
        /// </summary>
        /// <param name="w"></param>
        internal void WriteSVG(XmlWriter w)
        {
            if(_msgs.Count > 0)
            {
                w.WriteStartElement("noteOffs");
                foreach(MidiMsg msg in _msgs)
                {
                    Debug.Assert(IsNoteOffMsg(msg));
                    msg.WriteSVG(w);
                }
                w.WriteEndElement(); // end of noteOffs
            }
        }

        #region List functions
        /// <summary>
        /// Checks that all messages are noteOffs and have the same channel.
        /// </summary>
        public void Add(MidiMsg msg)
        {
            CheckMsg(msg);
            _msgs.Add(msg);
        }

        /// <summary>
        /// Checks that all messages are noteOffs and have the same channel.
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


        /// <summary>
        /// Checks that the new msg is a noteOff and has the same channel as existing msgs.
        /// </summary>
        private void CheckMsg(MidiMsg msg)
        {
            Debug.Assert(IsNoteOffMsg(msg));

            if(_msgs.Count > 0 && (_msgs[0].Channel != msg.Channel))
            {
                Debug.Assert(false);
            }
        }

        private bool IsNoteOffMsg(MidiMsg msg)
        {
            int statusHighNibbble = msg.Status & 0xF0;
            if(statusHighNibbble == M.CMD_NOTE_OFF_0x80 || (statusHighNibbble == M.CMD_NOTE_ON_0x90 && msg.Data2 == 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public IReadOnlyList<MidiMsg> Msgs { get { return _msgs; } }
        private List<MidiMsg> _msgs = new List<MidiMsg>();

        /// <summary>
        /// These two are true for the first moment in the score, otherwise false.
        /// </summary>
        internal bool IsStartOfEnvs = true;
        internal bool IsStartOfSwitches = true;

        // If an algorithm does not set the bank and patch for each channel at
        // the start of the score, they are both set to 0.
        internal byte BankState = 255;
        internal byte PatchState = 255;
        // If an algorithm does not set the states of the following controllers
        // at the start of the score, they are explicitly set to the followings values:
        // (These are the states that should be set by AllControllersOff)
        //     ModWheelState = 0;
        //     ExpressionState = 127;
        //     PanState = 64;
        //     PitchWheelState = 64;
        //     PitchWheelDeviationState = 2;
        internal byte ModWheelState = 255;
        internal byte ExpressionState = 255;
        internal byte PanState = 255;
        internal byte PitchWheelState = 255;
        internal byte PitchWheelDeviationState = 255;
    }
}