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
            return 84;
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
            // The chord number, in the base wind, at which each of Clytemnestra's blocks start.
            List<int> baseWindChordNumberPerClytBlock = new List<int>() { 6, 21, 36, 52, 67 };
            #endregion

            List<List<Voice>> bars = new List<List<Voice>>();
            int nBirdVoices = 0; // birds have not yet been composed. Set this value later to the correct value.
            int clytemnestrasChannelIndex = nBirdVoices;
            int topWindChannelIndex = clytemnestrasChannelIndex + 1;

            // The blockMsDurations at positions 1,3,5,7,9,11 will be set by the Winds (possibly taking account of the birds).
            // Clytemnestra sets the durations of blocks 2,4,6,8,10 (see below).
            List<int> blockMsDurations = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            Clytemnestra clytemnestra = new Clytemnestra(clytemnestrasChannelIndex, blockMsDurations);
            // Clytemnestra has now set the durations of blocks 2,4,6,8,10

            Winds winds = new Winds(_krystals, _paletteDefs, nBaseWindChords);

            SetBlockMsDurations(blockMsDurations, winds.MidiDefSequences[0], baseWindChordNumberPerClytBlock);

            //Birds birds = new Birds(winds, _krystals, _paletteDefs, nBirdVoices, blockMsDurations);

            List<int> blockMsPositions = GetBlockPositions(blockMsDurations); // for convenience...

            Voice clytemnestrasVoice = clytemnestra.GetVoice(clytemnestra.MomentDefsListPerVerse, blockMsPositions, blockMsDurations);

            List<Voice> wholePiece = new List<Voice>(); 
            wholePiece.Add(clytemnestrasVoice);
            List<Voice> windVoices = winds.GetVoices(clytemnestrasChannelIndex + 1);
            foreach(Voice voice in windVoices)
            {
                wholePiece.Add(voice);
            } 

            List<int> barlineMsPositions = GetBarlineMsPositions(blockMsDurations, clytemnestra.BarlineMsPositionsPerBlock
                //, birds.BarlineMsPositionsPerBlock,
                //winds.BarlineMsPositionsPerBlock,
                );

            // barlineMsPositions does not contain msPos=0 or the position of the final barline
            // It does however contain the positions of the other barlines that begin blocks.

            bars = GetBars(wholePiece, barlineMsPositions);
            Console.WriteLine("bars.Count = " + bars.Count.ToString());
            Debug.Assert(bars.Count == NumberOfBars());

            return bars;
        }

        /// <summary>
        /// There are 11 blocks. The durations of blocks 2, 4, 6, 8, 10 have been set by Clytemnestra.
        /// </summary>
        /// <param name="blockMsDurations"></param>
        /// <param name="totalMsDuration"></param>
        private void SetBlockMsDurations(List<int> blockMsDurations, MidiDefSequence baseMidiDefSequence, List<int> baseWindChordNumberPerClytBlock)
        {
            List<int> msPosPerClytBlock = new List<int>();
            foreach(int chordNumber in baseWindChordNumberPerClytBlock)
            {
                msPosPerClytBlock.Add(baseMidiDefSequence[chordNumber - 1].MsPosition);
            }

            blockMsDurations[0] = msPosPerClytBlock[0];
            blockMsDurations[2] = msPosPerClytBlock[1] - msPosPerClytBlock[0] - blockMsDurations[1];
            blockMsDurations[4] = msPosPerClytBlock[2] - msPosPerClytBlock[1] - blockMsDurations[3];
            blockMsDurations[6] = msPosPerClytBlock[3] - msPosPerClytBlock[2] - blockMsDurations[5];
            blockMsDurations[8] = msPosPerClytBlock[4] - msPosPerClytBlock[3] - blockMsDurations[7];
            blockMsDurations[10] = baseMidiDefSequence.EndMsPosition - msPosPerClytBlock[4] - blockMsDurations[9];
        }

        private List<int> GetBlockPositions(List<int> blockMsDurations)
        {
            List<int> poss = new List<int>() { 0 };
            int prevPos = 0;
            for(int i = 0; i < 10; ++i)
            {
                poss.Add(blockMsDurations[i] + prevPos);
                prevPos = poss[i+1];
            }
            return poss;
        }

        /// <summary>
        /// When the birds and winds have been composed, the arguments birdsBarlineMsPositionsPerBlock and
        /// windsBarlineMsPositionsPerBlock should be added to this function (there are comments inside, showing how they will be used).
        /// Both these arguments should (like clytemnestrasBarlineMsPositionsPerBlock) contain 11 lists of ints (1 per block).
        /// The contained msPositionsPerBlock lists should contain the positions of the barlines relative to the start of the block, but
        /// contain neither the first barline in the block (=0) nor the position of the end of the block.
        /// </summary>
        /// <returns>A sorted list of barline positions. The list contains neither the first position (=0) nor the last position</returns>
        private List<int> GetBarlineMsPositions(List<int> blockMsDurations, List<List<int>> clytemnestrasBarlineMsPositionsPerBlock)
        { 
            // The list to be returned. Will contain neither the first (=0) nor the final barline positions.
            List<int> barlineMsPoss = new List<int>();

            // Simply the position of each block. 11 values, 1 per block (starting with 0).
            List<int> blockMsPoss = new List<int>() {0};

            Debug.Assert(blockMsDurations.Count == 11);

            int prevPos = 0;
            for(int i = 0; i < 10; ++i)
            {
                int msPos = blockMsDurations[i] + prevPos;
                barlineMsPoss.Add(msPos);
                blockMsPoss.Add(msPos);
                prevPos = msPos;
            }

            Debug.Assert(barlineMsPoss.Count == 10);
            Debug.Assert(blockMsPoss.Count == 11);
            Debug.Assert(clytemnestrasBarlineMsPositionsPerBlock.Count == 11);
            //Debug.Assert(birdsBarlineMsPositionsPerBlock.Count == 11);
            //Debug.Assert(windsBarlineMsPositionsPerBlock.Count == 11);

            for(int i = 0; i < 11; ++i)
            {
                AddBarlinePositions(barlineMsPoss, blockMsPoss[i], clytemnestrasBarlineMsPositionsPerBlock[i]);
                // AddBarlinePositions(barlineMsPoss, blockMsPoss[i], birdsBarlineMsPositionsPerBlock);
                // AddBarlinePositions(barlineMsPoss, blockMsPoss[i], windssBarlineMsPositionsPerBlock);
            }

            barlineMsPoss.Sort();

            return barlineMsPoss;
        }

        private void AddBarlinePositions(List<int> barlineMsPoss, int blockMsPos, List<int> blockBarlineMsPositions)
        {
            if(blockBarlineMsPositions != null && blockBarlineMsPositions.Count > 0)
            {
                foreach(int msPosReBlock in blockBarlineMsPositions)
                {
                    int msPos = blockMsPos + msPosReBlock;
                    if(!barlineMsPoss.Contains(msPos))
                    {
                        barlineMsPoss.Add(msPos);
                    }
                }
            }
        }

        /// <summary>
        /// Splits the voices (currently in a single bar) into bars
        /// barlineMsPositions contains neither msPosition 0, nor the position of the final barline.
        /// </summary>
        private List<List<Voice>> GetBars(List<Voice> voices, List<int> barLineMsPositions)
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
