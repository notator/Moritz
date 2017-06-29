using System;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;
using Moritz.Spec;
using System.Collections.Generic;

namespace Moritz.Symbols
{
    internal class OutputRestSymbol : RestSymbol
	{
        public OutputRestSymbol(Voice voice, IUniqueDef iumdd, int absMsPosition, int minimumCrotchetDurationMS, float fontHeight)
            : base(voice, iumdd, absMsPosition, minimumCrotchetDurationMS, fontHeight)
        {
            CautionaryChordDef ccd = iumdd as CautionaryChordDef;
            MidiRestDef mrd = iumdd as MidiRestDef;

            if(mrd != null)
            {
                _midiRestDef = mrd;
            }

            // This needs testing!!
            if(ccd != null)
            {
                Console.WriteLine("rest is CautionaryChordDef!");
                LocalCautionaryChordDef = ccd;
            }
        }

        /// <summary>
        /// Dont use this function, use the other WriteSVG().
        /// </summary>
        /// <param name="w"></param>
        /// <param name="staffIsVisible"></param>
        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            throw new NotImplementedException();
        }

        public void WriteSVG(SvgWriter w, bool staffIsVisible, int channel, CarryMsgs carryMsgs)
        {
            if(LocalCautionaryChordDef == null)
            {
				w.SvgStartGroup("outputRest");

                Debug.Assert(_msDuration > 0);
				if(staffIsVisible)
				{
					w.WriteAttributeString("score", "alignment", null, ((Metrics.Left + Metrics.Right) / 2).ToString(M.En_USNumberFormat));
				}

                _midiRestDef.WriteSVG(w, channel, carryMsgs);

                if(this.Metrics != null && staffIsVisible)
                {
                    this.Metrics.WriteSVG(w);
                }

                w.SvgEndGroup();
            }
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("inputRest   ");
			sb.Append(InfoString);
			return sb.ToString();
		}

        MidiRestDef _midiRestDef = null;
    }
}
