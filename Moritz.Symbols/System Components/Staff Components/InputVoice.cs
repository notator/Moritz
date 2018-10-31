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
        }

        /// <summary>
        /// Writes out the noteObjects, and possibly the performanceOptions for an InputVoice.
        /// </summary>
        public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber, int voiceNumber, List<CarryMsgs> unused)
        {
			w.SvgStartGroup(CSSObjectClass.inputVoice.ToString());

            if(MidiChannel >= 0 && MidiChannel <= 15)
            {
                // This can only happen on the first system in the score. See SetMidiChannel(...) below.
                w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());
            }

            base.WriteSVG(w, null); // input voices dont carry messages

            w.SvgEndGroup(); // inputVoice
        }

        public IEnumerable<InputChordSymbol> InputChordSymbols
        {
            get
            {
                foreach(NoteObject no in this.NoteObjects)
                {
					if(no is InputChordSymbol inputChordSymbol)
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
