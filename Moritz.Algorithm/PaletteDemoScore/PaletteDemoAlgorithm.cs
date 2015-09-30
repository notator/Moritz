using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
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
        public PaletteDemoAlgorithm()
            : base()
        {
        }

        public override List<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0 }; } }
        public override List<int> MasterVolumePerOutputVoice { get { return new List<int>() { 127 }; } }
        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfBars { get { return 1; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            VoiceDef voice = new Trk(0, new List<IUniqueDef>());
            int msPosition = 0;
            for(int i = 0; i < palettes[0].Count; ++i)
            {
                IUniqueDef iumdd = palettes[0].UniqueDurationDef(i);
                iumdd.MsPosition = msPosition;
                msPosition += iumdd.MsDuration;
                voice.UniqueDefs.Add(iumdd);
            }
            List<List<VoiceDef>> voicesPerSystem = new List<List<VoiceDef>>();
            List<VoiceDef> systemVoices = new List<VoiceDef>();
            systemVoices.Add(voice);
            voicesPerSystem.Add(systemVoices);
            Debug.Assert(voicesPerSystem.Count == NumberOfBars);
            return voicesPerSystem;
        }
    }
}
