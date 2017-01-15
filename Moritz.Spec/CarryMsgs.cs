using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Moritz.Spec
{
    /// <summary>
    /// A simple class that is the list of noteOffs to be carried from one moment to the next.
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
            if(_msgs.Count > 0)
            {
                w.WriteStartElement("noteOffs");
                foreach(MidiMsg mm in _msgs)
                {
                    Debug.Assert(IsNoteOffMsg(mm));
                    mm.WriteSVG(w);
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
            if(statusHighNibbble == 0x80 || (statusHighNibbble == 0x90 && msg.Data2 == 0))
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
    }
}