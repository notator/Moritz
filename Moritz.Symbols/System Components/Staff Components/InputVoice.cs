using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public class InputVoice : Voice
    {
        public InputVoice(InputStaff inputStaff)
            : base(inputStaff)
        {
            _midiChannel = byte.MaxValue;
        }

        /// <summary>
        /// Writes out the noteObjects, and possibly the performanceOptions for an InputVoice.
        /// </summary>
        public override void WriteSVG(SvgWriter w, bool staffIsVisible, int systemNumber, int staffNumber, int voiceNumber)
        {
			w.SvgStartGroup("inputVoice", "sys" + systemNumber.ToString() + "staff" + staffNumber.ToString() + "voice" + voiceNumber.ToString());

            if(_midiChannel < byte.MaxValue)
            {
                // This can only happen on the first system in the score. See SetMidiChannel(...) below.
                w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());
            }

            base.WriteSVG(w, true); // input voices are always visible

            w.SvgEndGroup(); // inputVoice
        }

        internal void SetMidiChannel(byte midiChannel, int systemIndex)
        {
            Debug.Assert(systemIndex == 0);
            if(midiChannel >= 0 && midiChannel < 16)
            {
                _midiChannel = midiChannel;
            }
        }

        public IEnumerable<InputChordSymbol> InputChordSymbols
        {
            get
            {
                foreach(NoteObject no in this.NoteObjects)
                {
                    InputChordSymbol inputChordSymbol = no as InputChordSymbol;
                    if(inputChordSymbol != null)
                        yield return inputChordSymbol;
                }
            }
        }

        /// <summary>
        /// returns true if this inputVoice contains inputChord, otherwise false.
        /// </summary>
        /// <param name="inputChordDef"></param>
        /// <returns></returns>
        internal bool Contains(InputChordSymbol inputChord)
        {
            bool contains = false;
            foreach(InputChordSymbol localChord in InputChordSymbols)
            {
                if(localChord == inputChord)
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }
    }
}
