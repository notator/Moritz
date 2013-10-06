using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    internal class PaletteDemoAlgorithm : MidiCompositionAlgorithm
    {
        /// <summary>
        /// This file contains a composition algorithm which creates a score having a single Voice, whose
        /// MidiDurationDefs list is set to each of the krystal's values in turn.
        /// This score is never saved, so scoresRootFolder is not used. 
        /// </summary>
        public PaletteDemoAlgorithm(List<PaletteDef> paletteDefs)
            : base(null, paletteDefs)
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
            Voice voice = new Voice(null, 0);
            foreach(MidiDurationDef midiDurationDef in _paletteDefs[0])
            {
                LocalMidiDurationDef lmdd = new LocalMidiDurationDef(midiDurationDef);
                voice.LocalMidiDurationDefs.Add(lmdd);
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
