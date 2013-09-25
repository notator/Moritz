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
            return new List<byte>() { 0, 1, 2 };
        }

        /// <summary>
        /// The number of bars produced by DoAlgorithm().
        /// </summary>
        /// <returns></returns>
        public override int NumberOfBars()
        {
            return 94;
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
            #region Composed values (constants)
            // the number of values in the first five krystal blocks (the krystal actually has six blocks)
            int nBaseWindChords = 82;

            // Krystals included in the Song Six.mkss file can be examined by hovering over the _krystals variable.
            // Currently:
            //  _krystals[0] is xk3(7.7.1)-3: The first 6 blocks (=21 strands =82 values) are the palette values defining bass wind
            //  _krystals[1] is xk2(7.7.1)-2: The density input for _krystals[0]
            //  _krystals[2] is lk1(7)-1    : The density input for _krystals[1]
            #endregion

            // The blockMsDurations at positions 1,3,5,7,9,11 will be set by the Winds (possibly taking account of the birds).
            // Clytemnestra sets the durations of blocks 2,4,6,8,10 (see below).
            List<int> blockMsDurations = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            Clytemnestra clytemnestra = new Clytemnestra(blockMsDurations);
            // Clytemnestra has now set the durations of blocks 2,4,6,8,10

            Winds winds = new Winds(_krystals, _paletteDefs, nBaseWindChords);

            SetBlockMsDurations(blockMsDurations, winds);

            clytemnestra.MidiDefSequence = clytemnestra.GetMidiDefSequence(blockMsDurations);

            List<int> barlineMsPositions = clytemnestra.GetBarlineMsPositions(blockMsDurations);
            barlineMsPositions = winds.AddInterludeBarlinePositions(barlineMsPositions);

            // LocalMidiDurationDefs at barlineMsPositions cannot be shifted sideways.
            // barlineMsPositions is an argument to a new function, to be added to MidiDefSequence:
            //    AdjustDefMsPosition(barlineMsPositions, anchor1index, defIndex, newDefMsPos, anchor2index)
            //
            //winds.AdjustMsPositions(barlineMsPositions);

            //Birds birds = new Birds(clytemnestra, winds, _krystals, _paletteDefs, blockMsDurations);

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
            MidiDefSequence bassWind = winds.MidiDefSequences[0];
            List<int> baseWindChordIndexPerClytBlock = new List<int>();

            baseWindChordIndexPerClytBlock.Add(winds.BaseWindKrystalStrandIndices[4]);  // strand 5
            baseWindChordIndexPerClytBlock.Add(winds.BaseWindKrystalStrandIndices[7]);  // strand 8 (verse 1 has 3 strands
            baseWindChordIndexPerClytBlock.Add(winds.BaseWindKrystalStrandIndices[10]); // strand 11 (verse 2 has 3 strands)
            baseWindChordIndexPerClytBlock.Add(winds.BaseWindKrystalStrandIndices[13]); // strand 14 (verse 3 has 3 strands)
            baseWindChordIndexPerClytBlock.Add(winds.BaseWindKrystalStrandIndices[18]); // strand 19 (verse 4 has 5 strands, verse 5 has 3 strands)

            foreach(int chordIndex in baseWindChordIndexPerClytBlock)
            {
                msPosPerClytBlock.Add(bassWind[chordIndex].MsPosition);
            }

            blockMsDurations[0] = msPosPerClytBlock[0];
            blockMsDurations[2] = msPosPerClytBlock[1] - msPosPerClytBlock[0] - blockMsDurations[1];
            blockMsDurations[4] = msPosPerClytBlock[2] - msPosPerClytBlock[1] - blockMsDurations[3];
            blockMsDurations[6] = msPosPerClytBlock[3] - msPosPerClytBlock[2] - blockMsDurations[5];
            blockMsDurations[8] = msPosPerClytBlock[4] - msPosPerClytBlock[3] - blockMsDurations[7];
            blockMsDurations[10] = bassWind.EndMsPosition - msPosPerClytBlock[4] - blockMsDurations[9];

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
            clytemnestrasVoice.LocalizedMidiDurationDefs = clytemnestra.MidiDefSequence.LocalizedMidiDurationDefs;
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
            // barlineMsPositions does not contain msPos=0 or the position of the final barline
            // It does however contain all the other barline positions.
            List<List<Voice>> bars = new List<List<Voice>>();
            bars = GetBarsFromBarlineMsPositions(system, barlineMsPositions);
            Console.WriteLine("bars.Count = " + bars.Count.ToString());
            Debug.Assert(bars.Count == NumberOfBars());
            return bars;
        }

        /// <summary>
        /// Splits the voices (currently in a single bar) into bars
        /// barlineMsPositions contains neither msPosition 0, nor the position of the final barline.
        /// </summary>
        private List<List<Voice>> GetBarsFromBarlineMsPositions(List<Voice> voices, List<int> barLineMsPositions)
        {
            List<List<Voice>> bars = new List<List<Voice>>();
            List<List<Voice>> twoBars = null;

            for(int i = barLineMsPositions.Count - 1; i >= 0; --i)
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
