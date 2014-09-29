using System.Text;
using System.Diagnostics;


using Moritz.Midi;
using Moritz.Globals;
using Moritz.VoiceDef;
using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class Study2b2ChordSymbol : OutputChordSymbol
    {
        public Study2b2ChordSymbol(Voice voice, MidiChordDef umcd, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, umcd, minimumCrotchetDurationMS, fontSize)
        {
            SetGraphicSymbolID();
        }

        public void SetGraphicSymbolID()
        {
            Staff staff = Voice.Staff;
            int staffNumber = 0;
            for(int i = 0; i < staff.SVGSystem.Staves.Count; i++)
            {
                if(staff == staff.SVGSystem.Staves[i])
                {
                    staffNumber = i + 1;
                    break;
                }
            }
            SetSymbolID(staffNumber);
        }

        private void SetSymbolID(int staffNumber)
        {
            StringBuilder idSB = new StringBuilder("symb");
            Head lowestHead = HeadsTopDown[HeadsTopDown.Count - 1];
            int symbolNumber = 0;

            switch(lowestHead.Pitch[0])
            {
                case 'C':
                    symbolNumber = 1;
                    break;
                case 'D':
                    symbolNumber = 3;
                    break;
                case 'E':
                    symbolNumber = 5;
                    break;
                case 'F':
                    symbolNumber = 6;
                    break;
                case 'G':
                    symbolNumber = 8;
                    break;
                case 'A':
                    symbolNumber = 10;
                    break;
                case 'B':
                    symbolNumber = 12;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            symbolNumber += lowestHead.Alteration;

            staffNumber %= 3;
            staffNumber = (staffNumber == 0) ? 3 : staffNumber;

            idSB.Append(staffNumber);
            idSB.Append("_");
            idSB.Append(symbolNumber);

            _graphicSymbolID = idSB.ToString();
        }

        /// <summary>
        /// Writes out a Study2b2ChordSymbol. Uses the symbols defined in Study2b2.WriteSymbolDefinitions()
        /// The Study2b2ChordMetrics have been set in SvgSystem.Justify()
        /// </summary>
        /// <param name="w">The SvgWriter</param>
        public override void WriteSVG(SvgWriter w)
        {
            string idNumber = SvgScore.UniqueID_Number;
            w.SvgStartGroup("study2b2ChordSymbol", null);
            w.WriteAttributeString("score", "object", null, "outputChord");
            w.WriteAttributeString("score", "alignmentX", null, this.Metrics.OriginX.ToString(M.En_USNumberFormat));

            _midiChordDef.WriteSvg(w);

            if(this.Metrics != null)
                this.Metrics.WriteSVG(w);

            w.SvgEndGroup();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("2b2chord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }
    }
}
