using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantComposer
{
    internal class PaletteDemoAlgorithm : MidiCompositionAlgorithm
    {
        /// <summary>
        /// This file contains a composition algorithm which creates a score having a single Voice, whose
        /// MidiDurationDefs list is set to each of the krystal's values in turn.
        /// This score is never saved, so scoresRootFolder is not used. 
        /// </summary>
        public PaletteDemoAlgorithm(List<Palette> palettes)
            : base(null, palettes)
        {
        }

        /// <summary>
        /// The values are then checked for consistency in the base constructor.
        /// </summary>
        public override List<byte> MidiChannels()
        {
            return new List<byte>() { 0 };
        }

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
        public override List<List<Voice>> DoAlgorithm()
        {
            Voice voice = new OutputVoice(null, 0);
            int msPosition = 0;
            for(int i = 0; i < _palettes[0].Count; ++i)
            {
                IUniqueDef iumdd = _palettes[0].UniqueDurationDef(i);
                iumdd.MsPosition = msPosition;
                msPosition += iumdd.MsDuration;
                voice.UniqueDefs.Add(iumdd);
            }

            List<List<Voice>> voicesPerSystem = new List<List<Voice>>();
            List<Voice> systemVoices = new List<Voice>();
            systemVoices.Add(voice);
            voicesPerSystem.Add(systemVoices);

            Debug.Assert(voicesPerSystem.Count == NumberOfBars());

            return voicesPerSystem;
        }

        /// <summary>
        /// The number of bars produced by DoAlgorithm().
        /// </summary>
        /// <returns></returns>
        public override int NumberOfBars()
        {
            return 1;
        }
    }
}
