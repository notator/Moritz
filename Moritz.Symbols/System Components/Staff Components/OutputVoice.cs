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
        public OutputVoice(OutputStaff outputStaff, byte midiChannel, byte? voiceID, byte? masterVolume)
            : base(outputStaff)
        {
            _midiChannel = midiChannel;
            _voiceID = voiceID;
            _masterVolume = masterVolume;
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("outputVoice", null);

            if(MasterVolume != null) // is non-null only in the first system
            {
                if(VoiceID != null) // VoiceID is null if there are no InputVoices in the score.
                {
                    w.WriteAttributeString("score", "voiceID", null, VoiceID.ToString());
                }
                w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());
                w.WriteAttributeString("score", "masterVolume", null, MasterVolume.ToString());
            }

            base.WriteSVG(w);
            w.SvgEndGroup(); // outputVoice
        }

        /// <summary>
        /// If the score contains one or more InputVoices, the Assistant Composer writes a
        /// voiceID attribute for every OutputVoice in the first system in the score.
        /// No other OutputVoice.voiceIDs are written.
        /// </summary>
        public byte? VoiceID { get { return _voiceID; } }
        private byte? _voiceID = null;

        /// <summary>
        /// The Assistant Composer writes a midiChannel attribute for every OutputVoice in
        /// the first system in the score. No other OutputVoice.midiChannels are written.
        /// </summary>
        public byte MidiChannel { get { return _midiChannel; } } 
        private byte _midiChannel;

        /// <summary>
        /// The Assistant Composer writes a masterVolume attribute for every OutputVoice in
        /// the first system in the score. No other OutputVoice.masterVolumes are written.
        /// </summary>
        public byte? MasterVolume { get { return _masterVolume; } }
        private byte? _masterVolume = null;
    }
}
