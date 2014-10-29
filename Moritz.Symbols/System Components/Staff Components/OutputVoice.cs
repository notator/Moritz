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

        public OutputVoice(OutputStaff outputStaff, string midiChannel, string voiceID, string masterVolume)
            : base(outputStaff)
        {
            Debug.Assert(midiChannel != null);
            _midiChannel = byte.Parse(midiChannel);

            if(voiceID != null)
            {
                _voiceID = byte.Parse(voiceID);
            }
            if(masterVolume != null)
            {
                _masterVolume = byte.Parse(masterVolume);
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("outputVoice", null);

            w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());

            if(VoiceID != null) // VoiceID is null if there are no InputVoices in the score.
            {
                w.WriteAttributeString("score", "voiceID", null, VoiceID.ToString());
            }

            if(MasterVolume != null)
            {
                w.WriteAttributeString("score", "masterVolume", null, MasterVolume.ToString());
            }

            base.WriteSVG(w);

            w.SvgEndGroup(); // outputVoice
        }

        public byte MidiChannel { get { return _midiChannel; } } 
        private byte _midiChannel;

        public byte? VoiceID { get { return _voiceID; } }
        private byte? _voiceID = null;

        /// <summary>
        /// The composition algorithm sets the MasterVolume to a value != null for every OutputVoice in
        /// the first system in the score. All other OutputVoice.MasterVolumes are null.
        /// This value is written into the SVG-MIDI file as an attribute of each OutputVoice in the first
        /// system, but not to OutputVoices in later systems.
        /// It should be used by the Assistant Performer when initializing the channels.
        /// </summary>
        public byte? MasterVolume { get { return _masterVolume; } }
        private byte? _masterVolume = null;
    }
}
