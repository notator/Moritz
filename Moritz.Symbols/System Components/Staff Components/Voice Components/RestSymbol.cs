using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

using System;
using System.Diagnostics;

namespace Moritz.Symbols
{
    internal class RestSymbol : DurationSymbol
    {
        public RestSymbol(Voice voice, IUniqueDef iumdd, int absMsPosition, int minimumCrotchetDurationMS, float fontHeight)
            : base(voice, iumdd.MsDuration, absMsPosition, minimumCrotchetDurationMS, fontHeight)
        {
            if(iumdd is CautionaryChordDef)
            {
                Console.WriteLine("rest is CautionaryChordDef!");
            }
            LocalCautionaryChordDef = iumdd as CautionaryChordDef;
        }

        public RestSymbol(Voice voice, IUniqueDef iumdd, int absMsPosition, PageFormat pageFormat)
    : this(voice, iumdd, absMsPosition, pageFormat.MinimumCrotchetDuration, pageFormat.MusicFontHeight)
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
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public void WriteSVG(SvgWriter w, int channel, CarryMsgs carryMsgs)
        {
            if(LocalCautionaryChordDef == null)
            {
                Debug.Assert(_msDuration > 0);

                w.SvgStartGroup(CSSObjectClass.rest.ToString()); // "rest"

                w.WriteAttributeString("score", "alignment", null, ((Metrics.Left + Metrics.Right) / 2).ToString(M.En_USNumberFormat));

                _midiRestDef.WriteSVG(w, channel, carryMsgs);

                if(this.Metrics != null)
                {
                    ((RestMetrics)this.Metrics).WriteSVG(w);
                }

                w.SvgEndGroup(); // "rest"
            }
        }

        public override string ToString() => "outputRest " + InfoString;

        MidiRestDef _midiRestDef = null;

        #region display attributes
        /// <summary>
        /// If LocalizedCautionaryChordDef is set:
        /// a) this rest is used like any other rest when justifying systems, but
        /// b) it is not displayed, and does not affect the temporal positions or durations of any chords. 
        /// </summary>
        public CautionaryChordDef LocalCautionaryChordDef = null;
        #endregion display attributes
        #region verticalPos attributes
        public bool Centered = false; // capella default
        public int Shift_Gap = 0; // capella default
        #endregion verticalPos attributes

        /// <summary>
        /// Returns this.Metrics cast to RestMetrics.
        /// Before accessing this property, this.Metrics must be assigned to an object of type RestMetrics.
        /// </summary>
        internal RestMetrics RestMetrics
        {
            get
            {
                RestMetrics restMetrics = Metrics as RestMetrics;
                Debug.Assert(restMetrics != null);
                return restMetrics;
            }
        }

        // Rest MsDuration should only be set when agglommerating consecutive Rests
        public override int MsDuration
        {
            get { return _msDuration; }
            set { _msDuration = value; }
        }
    }
}
