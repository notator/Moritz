﻿using Moritz.Globals;
using Moritz.Xml;

using System.Diagnostics;

namespace Moritz.Spec
{
    public class InputNoteDef
    {
        public InputNoteDef(byte notatedMidiPitch, NoteOn noteOn, NoteOff noteOff, TrkOptions trkOptions)
        {
            Debug.Assert(notatedMidiPitch >= 0 && notatedMidiPitch <= 127);
            // If trkOptions is null, the higher level trkOptions are used.

            NotatedMidiPitch = notatedMidiPitch;
            NoteOn = noteOn;
            NoteOff = noteOff;
            TrkOptions = trkOptions;
        }

        /// <summary>
        /// This constructs an InputNoteDef that, when the noteOff arrives,
        /// turns off all the trks that were turned on by the noteOn.
        /// </summary>
        public InputNoteDef(byte notatedMidiPitch, NoteOn noteOn, TrkOptions trkOptions)
            : this(notatedMidiPitch, noteOn, null, trkOptions)
        {
            //public NoteOff(NoteOn noteOn, Seq seq, TrkOptions trkOptions)
            NoteOff = new NoteOff(noteOn, null, null);
        }

        internal void WriteSVG(SvgWriter w)
        {
            w.WriteStartElement("inputNote");
            w.WriteAttributeString("notatedKey", _notatedMidiPitch.ToString());

            if(TrkOptions != null)
            {
                TrkOptions.WriteSVG(w, false);
            }

            if(NoteOn != null)
            {
                NoteOn.WriteSVG(w);
            }

            if(NoteOff != null)
            {
                NoteOff.WriteSVG(w);
            }

            w.WriteEndElement(); // score:inputNote N.B. This element can be empty!
        }

        public byte NotatedMidiPitch { get { return _notatedMidiPitch; } set { _notatedMidiPitch = M.MidiValue(value); } }
        private byte _notatedMidiPitch;

        public NoteOn NoteOn = null;
        public NoteOff NoteOff = null;

        public TrkOptions TrkOptions = null;
    }
}
