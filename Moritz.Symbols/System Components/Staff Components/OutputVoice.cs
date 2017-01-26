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

		public override void WriteSVG(SvgWriter w, bool staffIsVisible, int systemNumber, int staffNumber, int voiceNumber, List<CarryMsgs> carryMsgsPerChannel)
        {
			w.SvgStartGroup("outputVoice");
            w.WriteAttributeString("score", "hasMidi", null, "true"); // this is a "voice" container

            base.WriteSVG(w, staffIsVisible, carryMsgsPerChannel);
            w.SvgEndGroup(); // outputVoice
        }
    }
}
