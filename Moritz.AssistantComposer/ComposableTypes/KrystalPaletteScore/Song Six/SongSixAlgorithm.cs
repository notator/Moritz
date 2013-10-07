using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Algorithm for Song 6.
    /// This will develope as composition progresses...
    /// </summary>
    internal class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        public SongSixAlgorithm(List<Krystal> krystals, List<PaletteDef> paletteDefs)
            : base(krystals, paletteDefs)
        {
        }

        /// <summary>
        /// The values are then checked for consistency in the base constructor.
        /// </summary>
        public override List<byte> MidiChannels()
        {
            return new List<byte>() { 0, 1, 2, 3, 4, 5 };
        }

        /// <summary>
        /// The number of bars produced by DoAlgorithm().
        /// </summary>
        /// <returns></returns>
        public override int NumberOfBars()
        {
            return 93;
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
            /// _krystals[0] is lk1(6)-1.krys containing the line {1, 2, 3, 4, 5, 6}
            /// _krystals[1] is xk2(7.7.1)-9.krys, expanded using:
            ///         expander: e(7.7.1).kexp
            ///         density and points inputs: lk1(6)-1.krys 
            /// _krystals[2] is xk3(7.7.1)-9.krys, expanded using:
            ///         expander: e(7.7.1).kexp
            ///         density and points inputs: xk2(7.7.1)-9.krys
            ///
            /// The bass wind is constructed from _krystals[2] as a flat sequence.
            /// The tenor wind is the bass wind in reverse.

            // Clytemnestra sets the durations of blocks 2,4,6,8,10 (see below).
            // The blockMsDurations at positions 1,3,5,7,9,11 will be set by the Winds.
            List<int> blockMsDurations = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            Clytemnestra clytemnestra = new Clytemnestra(blockMsDurations);
            // Clytemnestra has now set the durations of blocks 2,4,6,8,10

            Winds winds = new Winds(_krystals, _paletteDefs); // only constructs wind 5 -- see ConstructUpperWinds() below.

            SetBlockMsDurations(blockMsDurations, winds);

            clytemnestra.VoiceDef
                = clytemnestra.GetVoiceDef(blockMsDurations);

            List<int> barlineMsPositions = clytemnestra.GetBarlineMsPositions(blockMsDurations);
            // barlineMsPositions contains both the position of bar 1 (0ms) and the position of the final barline
            barlineMsPositions = winds.AddInterludeBarlinePositions(barlineMsPositions);

            Debug.Assert(barlineMsPositions.Count == NumberOfBars() + 1); // includes bar 1 (mPos=0) and the final barline.
            
            winds.CompleteTheWinds(barlineMsPositions);

            #region test code
            //code for testing VoiceDef.SetContour(...)
            //VoiceDef contouredPhrase = winds.VoiceDefs[0];
            //contouredPhrase.SetContour(11, new List<int>() { 1, 4, 1, 2 }, 1, 1);

            //code for testing Translate
            VoiceDef translated = winds.VoiceDefs[0];
            translated.Translate(15, 4, 16);
            #endregion

            //Birds birds = new Birds(clytemnestra, winds, _krystals, _paletteDefs, blockMsDurations);

            clytemnestra.AddIndexToLyrics();
            foreach(VoiceDef wind in winds.VoiceDefs)
            {
                wind.SetLyricsToIndex();
            }

            // system contains one Voice per channel (not divided into bars)
            List<Voice> system = GetVoices(/*birds,*/ clytemnestra, winds);

            List<List<Voice>> bars = GetBars(system, barlineMsPositions);

            return bars;
        }

        /// <summary>
        /// There are 11 blocks. The durations of blocks 2, 4, 6, 8, 10 have been set by Clytemnestra.
        /// </summary>
        /// <param name="blockMsDurations"></param>
        /// <param name="totalMsDuration"></param>
        private void SetBlockMsDurations(List<int> blockMsDurations, Winds winds)
        {
            List<int> msPosPerClytBlock = new List<int>();
            VoiceDef wind5 = winds.VoiceDefs[0];
            List<int> baseWindChordIndexPerClytBlock = new List<int>();

            baseWindChordIndexPerClytBlock.Add(8);  // strand 5
            baseWindChordIndexPerClytBlock.Add(20); // strand 8 (verse 1 has 3 strands
            baseWindChordIndexPerClytBlock.Add(33); // strand 11 (verse 2 has 3 strands)
            baseWindChordIndexPerClytBlock.Add(49); // strand 14 (verse 3 has 3 strands)
            baseWindChordIndexPerClytBlock.Add(70); // strand 19 (verse 4 has 5 strands, verse 5 has 3 strands)

            foreach(int chordIndex in baseWindChordIndexPerClytBlock)
            {
                msPosPerClytBlock.Add(wind5[chordIndex].MsPosition);
            }

            blockMsDurations[0] = msPosPerClytBlock[0];
            blockMsDurations[2] = msPosPerClytBlock[1] - msPosPerClytBlock[0] - blockMsDurations[1];
            blockMsDurations[4] = msPosPerClytBlock[2] - msPosPerClytBlock[1] - blockMsDurations[3];
            blockMsDurations[6] = msPosPerClytBlock[3] - msPosPerClytBlock[2] - blockMsDurations[5];
            blockMsDurations[8] = msPosPerClytBlock[4] - msPosPerClytBlock[3] - blockMsDurations[7];
            blockMsDurations[10] = wind5.EndMsPosition - msPosPerClytBlock[4] - blockMsDurations[9];

            for(int i = 0; i < blockMsDurations.Count; ++i)
            {
                Debug.Assert(blockMsDurations[i] > 0, "Block " + (i+1).ToString() + " would have negative duration.");
            }
        }

        private List<Voice> GetVoices(/*Birds birds,*/ Clytemnestra clytemnestra, Winds winds)
        {
            byte channelIndex = 0;
            List<Voice> voices = new List<Voice>();

            //List<Voice> birdVoices = birds.GetVoices(channelIndex);
            //foreach(Voice voice in birdVoices)
            //{
            //    voices.Add(voice);
            //    channelIndex++;
            //}

            Voice clytemnestrasVoice = new Voice(null, channelIndex++);
            clytemnestrasVoice.LocalMidiDurationDefs = clytemnestra.VoiceDef.LocalMidiDurationDefs;
            voices.Add(clytemnestrasVoice);

            List<Voice> windVoices = winds.GetVoices(channelIndex);

            foreach(Voice voice in windVoices)
            {
                voices.Add(voice);
            }

            return voices;
        }

        private List<List<Voice>> GetBars(List<Voice> system, List<int> barlineMsPositions)
        {
            // barlineMsPositions contains both msPos=0 and the position of the final barline
            List<List<Voice>> bars = new List<List<Voice>>();
            bars = GetBarsFromBarlineMsPositions(system, barlineMsPositions);
            Debug.Assert(bars.Count == NumberOfBars());
            return bars;
        }

        /// <summary>
        /// Splits the voices (currently in a single bar) into bars
        /// barlineMsPositions contains both msPosition 0, and the position of the final barline.
        /// </summary>
        private List<List<Voice>> GetBarsFromBarlineMsPositions(List<Voice> voices, List<int> barLineMsPositions)
        {
            List<List<Voice>> bars = new List<List<Voice>>();
            List<List<Voice>> twoBars = null;

            for(int i = barLineMsPositions.Count - 2; i >= 1; --i)
            {
                twoBars = SplitBar(voices, barLineMsPositions[i]);
                bars.Insert(0, twoBars[1]);
                voices = twoBars[0];
            }
            bars.Insert(0, twoBars[0]);

            return bars;
        }
    }
}
