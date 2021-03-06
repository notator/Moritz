using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Midi;
using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public class OutputVoice : Voice
    {
        public OutputVoice(OutputStaff outputStaff, int midiChannel)
            : base(outputStaff)
        {
            MidiChannel = midiChannel;
        }

		public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber, int voiceNumber, List<CarryMsgs> carryMsgsPerChannel)
        {
			w.SvgStartGroup(CSSObjectClass.voice.ToString());

            base.WriteSVG(w, carryMsgsPerChannel);
            w.SvgEndGroup(); // outputVoice
        }
    }
}
