using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score;
using Moritz.Globals;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Algorithm for Song 6.
    /// This will develope as composition progresses...
    /// </summary>
    internal class Song6SketchAlgorithm : MidiCompositionAlgorithm
    {
        /// <summary>
        /// The Song6Algorithm uses neither krystals nor palettes.
        /// </summary>
        public Song6SketchAlgorithm()
            : base(null, null)
        {
        }

        /// <summary>
        /// The values are then checked for consistency in the base constructor.
        /// </summary>
        public override List<byte> MidiChannels()
        {
            return new List<byte>() { 0 };
        }

        public override void AddLyrics(List<SvgSystem> systems)
        {
            Debug.Assert(_clytemnestra != null);
            _clytemnestra.AddLyrics(systems);
        }


        /// <summary>
        /// Sets the midi content of the score, independent of its notation.
        /// This means adding MidiDurationDefs to each voice's MidiDurationDefs list.
        /// The MidiDurations will later be transcribed into a particular notation by a Notator.
        /// Notations are independent of the midi info.
        /// This DoAlgorithm() function is special to this composition.
        /// </summary>
        /// <returns>
        /// A list of sequential bars. Each bar contains all the voices in the bar, from top to bottom.
        /// </returns>
        public override List<List<Voice>> DoAlgorithm()
        {
            List<List<Voice>> bars = new List<List<Voice>>();

            // The following values are just placeholders.
            // They will be replaced by the actual values, when the Furies' music is complete.
            List<List<int>> interludeBars = InterludeBars;

            string algorithmFolderPath = M.Preferences.LocalScoresRootFolder + "\\Song 6 sketch";
            string midiInputFolder = algorithmFolderPath + "\\midi";
            _clytemnestra = new Clytemnestra(midiInputFolder, interludeBars);

            foreach(Voice clytemnestraBar in _clytemnestra.Bars)
            {
                List<Voice> tuttiBar = new List<Voice>() { clytemnestraBar };
                bars.Add(tuttiBar);
            }

            // bars now contain just the singer's voice.
            // the singer's voice has correct durations (except for the intermediate
            // rests and the last note, which is probably a melisma)

            Debug.Assert(bars.Count == NumberOfBars(algorithmFolderPath));

            return bars;
        }

        private List<List<int>> InterludeBars
        {
            get
            {
                return new List<List<int>>()
                {
                    new List<int>(){2000,2000,2000,2000,2000}, // bars before verse 1
                    new List<int>(){2000,2000,2000,2000,2000}, // bars after verse 1
                    new List<int>(){2000,2000,2000,2000,2000}, // bars after verse 2
                    new List<int>(){2000,2000,2000,2000,2000}, // bars after verse 3
                    new List<int>(){2000,2000,2000,2000,2000}, // bars after verse 4
                    new List<int>(){2000,2000,2000,2000,2000}  // bars after verse 5
                };
            }
        }

        /// <summary>
        /// The number of bars produced by DoAlgorithm().
        /// </summary>
        /// <returns></returns>
        public override int NumberOfBars(string algorithmFolderPath)
        {
            string midiFolder = algorithmFolderPath + "\\midi";
            Clytemnestra clytemnestra = new Clytemnestra(midiFolder, InterludeBars);
            return clytemnestra.Bars.Count;
        }

        private Clytemnestra _clytemnestra = null;
    }
}
