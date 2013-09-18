using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Score;
using Moritz.Score.Midi;

using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Globals;
using Krystals4ObjectLibrary;
using Moritz.AssistantComposer;

namespace Moritz.AssistantComposer
{
    class Winds
    {
        // The durations of the Winds are adjusted to the blockMsDurations
        public Winds(List<Krystal> krystals, List<PaletteDef> paletteDefs, int topWindChannelIndex, List<int> blockMsDurations)
        {
            Debug.Assert(blockMsDurations.Count == 11);

            int nWindsVoices = 1; // including the baseVoice
            Voice baseVoice = GetBaseVoice(krystals[0], paletteDefs[0], topWindChannelIndex + nWindsVoices - 1, blockMsDurations);
            _voices.Insert(0, baseVoice);
            // etc. -- create and insert other wind voices, so that wind channels are ordered top down in order of channel.
        }

        private Voice GetBaseVoice(Krystal krystal, PaletteDef paletteDef, int baseWindChannelIndex, List<int> blockMsDurations)
        {
            Voice baseVoice = new Voice(null, (byte) baseWindChannelIndex);

            List<List<int>> kValues = krystal.GetValues((uint)1);
            List<int> sequence = kValues[0];

            List<LocalizedMidiDurationDef> lmdds = GetDefaultLocalizedMidiDurationDefs(sequence, paletteDef);
            int finalTotalDuration = 0;
            for(int i = 0; i < 11; ++i)
            {
                finalTotalDuration += blockMsDurations[i];
            }

            // The durations of the LocalizedMidiDurationDefs are adjusted to the blockMsDurations
            Voice.SetLMDDListMsDuration(lmdds, finalTotalDuration);

            baseVoice.LocalizedMidiDurationDefs = lmdds;

            return baseVoice;
        }

        private List<LocalizedMidiDurationDef> GetDefaultLocalizedMidiDurationDefs(List<int> sequence, PaletteDef paletteDef)
        {
            int msPosition = 0;
            List<LocalizedMidiDurationDef> lmdds = new List<LocalizedMidiDurationDef>();
            foreach(int value in sequence)
            {
                MidiDurationDef midiDurationDef = paletteDef[value - 1];
                LocalizedMidiDurationDef noteDef = new LocalizedMidiDurationDef(midiDurationDef);
                Debug.Assert(midiDurationDef.MsDuration > 0);
                Debug.Assert(noteDef.MsDuration == midiDurationDef.MsDuration);
                noteDef.MsPosition = msPosition;
                msPosition += noteDef.MsDuration;
                lmdds.Add(noteDef);
                Console.WriteLine("MsPosition=" + noteDef.MsPosition.ToString() + "  MsDuration=" + noteDef.MsDuration.ToString());
            }
            Console.WriteLine("---");
            return lmdds;
        }


        // each voice has the duration of the whole piece
        public List<Voice> Voices { get { return _voices; } } // voices are ordered top to bottom
        private List<Voice> _voices = new List<Voice>(); 
    }
}
