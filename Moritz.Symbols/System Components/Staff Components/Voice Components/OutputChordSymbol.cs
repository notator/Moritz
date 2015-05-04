using System.Text;

using Moritz.Midi;
using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class OutputChordSymbol : ChordSymbol
    {
        public OutputChordSymbol(Voice voice, MidiChordDef umcd, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd.MsDuration, umcd.MsPosition, minimumCrotchetDurationMS, fontSize)
        {
            _midiChordDef = umcd;

            _msDurationToNextBarline = umcd.MsDurationToNextBarline;

            SetNoteheadPitches(umcd.NotatedMidiPitches);

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

        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            if(staffIsVisible && ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

			w.SvgStartGroup("outputChord", "outputChord" + SvgScore.UniqueID_Number);
			if(staffIsVisible)
			{ 
				w.WriteAttributeString("score", "alignmentX", null, ChordMetrics.OriginX.ToString(M.En_USNumberFormat));
			}
            
            _midiChordDef.WriteSvg(w);

            if(staffIsVisible)
            {
				w.SvgStartGroup(null, "graphics" + SvgScore.UniqueID_Number);
                ChordMetrics.WriteSVG(w);
                w.SvgEndGroup();
            }

            w.SvgEndGroup();
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("output chord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public MidiChordDef MidiChordDef { get { return _midiChordDef; } }
        protected MidiChordDef _midiChordDef = null;
    }
}
