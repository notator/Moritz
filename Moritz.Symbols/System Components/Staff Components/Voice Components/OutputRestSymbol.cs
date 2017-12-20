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
            if(iumdd is MidiRestDef mrd)
            {
                _midiRestDef = mrd;
            }

			// This needs testing!!
			if(iumdd is CautionaryChordDef ccd)
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
                Debug.Assert(_msDuration > 0);
				if(staffIsVisible)
				{
                    w.SvgStartGroup(Metrics.CSSObjectClass.ToString()); // "rest"
                    w.WriteAttributeString("score", "alignment", null, ((Metrics.Left + Metrics.Right) / 2).ToString(M.En_USNumberFormat));
				}
                else
                {
                    w.SvgStartGroup("rest"); // Metrics is null
                }

                _midiRestDef.WriteSVG(w, channel, carryMsgs);

                if(this.Metrics != null && staffIsVisible)
                {
                    ((RestMetrics)this.Metrics).WriteSVG(w);
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
