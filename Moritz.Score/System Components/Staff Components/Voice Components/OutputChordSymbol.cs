using System.Text;

using Moritz.Score.Midi;

namespace Moritz.Score
{
    public class OutputChordSymbol : ChordSymbol
    {
        public OutputChordSymbol(Voice voice, MidiChordDef umcd, int minimumCrotchetDurationMS, float fontSize)
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

        public MidiChordDef MidiChordDef { get { return _uniqueMidiChordDef; } }
        protected MidiChordDef _uniqueMidiChordDef = null;
    }
}
