using System.Text;

using Moritz.Score.Midi;
using Moritz.Globals;
using Moritz.Score.Notation;

namespace Moritz.Score
{
    public class OutputChordSymbol : ChordSymbol
    {
        public OutputChordSymbol(Voice voice, MidiChordDef umcd, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd.MsDuration, umcd.MsPosition, minimumCrotchetDurationMS, fontSize)
        {
            _midiChordDef = umcd;

            _msDurationToNextBarline = umcd.MsDurationToNextBarline;

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

        public override void WriteSVG(SvgWriter w)
        {
            if(ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

            w.SvgStartGroup("outputChord", null);
            //w.WriteAttributeString("score", "object", null, "outputChord");
            w.WriteAttributeString("score", "alignmentX", null, this.Metrics.OriginX.ToString(M.En_USNumberFormat));

            if(_midiChordDef.PerformanceControlDef != null)
                _midiChordDef.PerformanceControlDef.WriteSvg(w);  
            
            _midiChordDef.WriteSvg(w);

            w.SvgStartGroup("graphics", null);
            ChordMetrics.WriteSVG(w);
            w.SvgEndGroup();

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
