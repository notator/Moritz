using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.PaletteDemo
{
    public class PaletteDemoAlgorithm : CompositionAlgorithm
    {
        /// <summary>
        /// This file contains a composition algorithm which creates a score having a single VoiceDef, whose
        /// MidiDurationDefs list is set to each of the krystal's values in turn.
        /// This score is never saved, so scoresRootFolder is not used. 
        /// </summary>
        public PaletteDemoAlgorithm(List<Palette> palettes)
            : base(null, palettes)
        {
        }

        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfOutputVoices { get { return 1; } }
        public override int NumberOfBars { get { return 1; } }

        /// <summary>
        /// Sets the midi content of the score, independent of its notation.
        /// This means adding MidiDurationDefs to each voice's MidiDurationDefs list.
        /// This particular score has no notation, but the MidiDurations constructed
        /// here are usually transcribed into a particular notation by a Notator.
        /// Notations are independent of the midi info.
        /// This DoAlgorithm() function is special to this composition.
        /// It creates a single bar=system containing one event for each of the events
        /// defined in a single palette.
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm()
        {
            VoiceDef voice = new OutputVoiceDef();
            int msPosition = 0;
            for(int i = 0; i < _palettes[0].Count; ++i)
            {
                IUniqueDef iumdd = _palettes[0].UniqueDurationDef(i);
                iumdd.MsPosition = msPosition;
                msPosition += iumdd.MsDuration;
                voice.UniqueDefs.Add(iumdd);
            }

            List<List<VoiceDef>> voicesPerSystem = new List<List<VoiceDef>>();
            List<VoiceDef> systemVoices = new List<VoiceDef>();
            systemVoices.Add(voice);
            voicesPerSystem.Add(systemVoices);

            List<byte> masterVolumes = new List<byte>() {127};
            SetOutputVoiceMasterVolumes(voicesPerSystem[0], masterVolumes);

            Debug.Assert(voicesPerSystem.Count == NumberOfBars);

            return voicesPerSystem;
        }
    }
}
