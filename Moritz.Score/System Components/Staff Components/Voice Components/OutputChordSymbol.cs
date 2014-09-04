using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.Score
{
    public class OutputChordSymbol : ChordSymbol
    {
        public OutputChordSymbol(Voice voice, UniqueMidiChordDef umcd, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd.MsDuration, umcd.MsPosition, minimumCrotchetDurationMS, fontSize)
        {
            _uniqueMidiChordDef = umcd;

            SetNoteheadPitches(umcd.MidiPitches);

            if(umcd.OrnamentNumberSymbol != 0)
            {
                AddOrnamentSymbol("~" + umcd.OrnamentNumberSymbol.ToString());
            }

            if(umcd.Lyric != null)
            {
                TextInfo textInfo = new TextInfo(umcd.Lyric, "Arial", (float)(FontHeight / 2F), TextHorizAlign.center);
                Lyric lyric = new Lyric(this, textInfo);
                DrawObjects.Add(lyric);
            }
        }

        /// <summary>
        /// Writes this outputChord's midi info
        /// </summary>
        /// <param name="w"></param>
        protected override void WriteContent(SvgWriter w, string idNumber)
        {
            _uniqueMidiChordDef.WriteSvg(w, idNumber);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("output chord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public UniqueMidiChordDef UniqueMidiChordDef { get { return _uniqueMidiChordDef; } }
        protected UniqueMidiChordDef _uniqueMidiChordDef = null;
    }
}
