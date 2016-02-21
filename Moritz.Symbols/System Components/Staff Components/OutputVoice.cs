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
        public OutputVoice(OutputStaff outputStaff, int midiChannel, byte? masterVolume)
            : base(outputStaff)
        {
            MidiChannel = midiChannel;
            _masterVolume = masterVolume;
        }

		public override void WriteSVG(SvgWriter w, bool staffIsVisible, int systemNumber, int staffNumber, int voiceNumber)
        {
			w.SvgStartGroup("outputVoice", "sys" + systemNumber.ToString() + "staff" + staffNumber.ToString() + "voice" + voiceNumber.ToString());

            if(MasterVolume != null) // is non-null only in the first system
            {
                w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());
                w.WriteAttributeString("score", "masterVolume", null, MasterVolume.ToString());
            }

            base.WriteSVG(w, staffIsVisible);
            w.SvgEndGroup(); // outputVoice
        }

        /// <summary>
        /// The Assistant Composer writes a masterVolume attribute for every OutputVoice in
        /// the first system in the score. No other OutputVoice.masterVolumes are written.
        /// </summary>
        public byte? MasterVolume { get { return _masterVolume; } }
        private byte? _masterVolume = null;
    }
}
