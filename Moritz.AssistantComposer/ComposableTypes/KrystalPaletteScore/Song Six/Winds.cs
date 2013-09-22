using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Score;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    class Winds
    {
        // The durations of the Winds are adjusted to the blockMsDurations
        public Winds(List<Krystal> krystals, List<PaletteDef> paletteDefs, int topWindChannelIndex, List<int> blockMsDurations)
        {
            Debug.Assert(blockMsDurations.Count == 11);

            int nWindsVoices = 2; // including the baseVoice

            MidiDefSequence baseMidiDefSequence = GetBaseMidiDefSequence(krystals[0], paletteDefs[0], blockMsDurations);
            Voice baseVoice = new Voice(null, (byte)(topWindChannelIndex + nWindsVoices - 1));
            baseVoice.LocalizedMidiDurationDefs = baseMidiDefSequence.LocalizedMidiDurationDefs;
            _voices.Insert(0, baseVoice);

            // etc. -- create and insert other wind voices, so that wind channels are ordered top down in order of channel.
            MidiDefSequence tenorMidiDefSequence = GetTenorMidiDefSequence(baseMidiDefSequence);
            Voice tenorVoice = new Voice(null, (byte)(topWindChannelIndex + nWindsVoices - 2));
            tenorVoice.LocalizedMidiDurationDefs = tenorMidiDefSequence.LocalizedMidiDurationDefs;
            _voices.Insert(0, tenorVoice);
        }

        private MidiDefSequence GetBaseMidiDefSequence(Krystal krystal, PaletteDef paletteDef, List<int> blockMsDurations)
        {
            List<List<int>> kValues = krystal.GetValues((uint)1);
            List<int> sequence = kValues[0];

            MidiDefSequence baseMidiDefSequence = new MidiDefSequence(paletteDef, sequence);

            baseMidiDefSequence.Transpose(-4);

            int finalTotalDuration = 0;
            for(int i = 0; i < 11; ++i)
            {
                finalTotalDuration += blockMsDurations[i];
            }

            // The durations of the contained LocalizedMidiDurationDefs are adjusted to the blockMsDurations
            baseMidiDefSequence.MsDuration = finalTotalDuration;

            return baseMidiDefSequence;
        }

        private MidiDefSequence GetTenorMidiDefSequence(MidiDefSequence baseMidiDefSequence)
        {
            MidiDefSequence tenorMidiDefSequence = baseMidiDefSequence.Clone();

            tenorMidiDefSequence.LocalizedMidiDurationDefs.Reverse();
            tenorMidiDefSequence.MsPosition = 0;
            tenorMidiDefSequence.Transpose(24);

            return tenorMidiDefSequence;
        }

        // each voice has the duration of the whole piece
        public List<Voice> Voices { get { return _voices; } } // voices are ordered top to bottom
        private List<Voice> _voices = new List<Voice>(); 
    }
}
