using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Globals;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Algorithm for Song 6.
    /// This will develope as composition progresses...
    /// </summary>
    internal class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        /// <summary>
        /// The Song6Algorithm uses neither krystals nor palettes.
        /// </summary>
        public SongSixAlgorithm(List<Krystal> krystals, List<PaletteDef> paletteDefs)
            : base(krystals, paletteDefs)
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
            int clytemnestrasChannelIndex = 0;

            // The blockMsDurations at positions 1,3,5,7,9,11 will probably be changed by birds and/or winds.
            // They have been set here for tesing purposes during the composition of Clytemnestra.
            // Clytemnestra sets the durations of blocks 2,4,6,8,10
            List<int> blockMsDurations = new List<int>(){8000,0,8000,0,8000,0,8000,0,8000,0,8000};

            // Clytemnestra sets the durations of blocks 2,4,6,8,10
            Clytemnestra clytemnestra = new Clytemnestra(clytemnestrasChannelIndex, blockMsDurations);

            // compose other momentDefs here (using blockMsDurations)
            // Birds birds = new Birds(_paletteDefs, out blockDurations);
            // Winds winds = new Winds(_paletteDefs, out blockdurations);

            List<int> blockMsPositions = GetBlockPositions(blockMsDurations); // for convenience...

            Voice clytemnestrasVoice = GetClytemnestrasVoice(clytemnestra.MomentDefsListPerVerse, blockMsPositions, blockMsDurations);
            // List<Voice> birdsVoices = GetBirdsVoices(birds.MomentDefListPerVoicePerBlock, blockMsPositions, blockMsDurations);
            // List<Voice> windsVoices = GetWindsVoices(winds.MomentDefListPerVoicePerBlock, blockMsPositions, blockMsDurations);

            List<Voice> wholePiece = new List<Voice>();
            wholePiece.Add(clytemnestrasVoice);
            // add birdsVoices
            // add windsVoices 

            List<int> barlineMsPositions = GetBarlineMsPositions(blockMsDurations, clytemnestra.BarlineMsPositionsPerBlock
                //, birds.BarlineMsPositionsPerBlock,
                //winds.BarlineMsPositionsPerBlock,
                );

            // barlineMsPositions does not contain msPos=0 or the position of the final barline
            // It does however contain the positions of the other barlines that begin blocks.

            bars = GetBars(wholePiece, barlineMsPositions);

            //Debug.Assert(bars.Count == NumberOfBars());

            return bars;
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
        /// returns Clytamnestra's voice for the whole piece including rests (but no bars)
        /// </summary>
        private Voice GetClytemnestrasVoice(List<List<MomentDef>> momentDefsListPerVerse, List<int> blockMsPositions, List<int> blockMsDurations)
        {
            Debug.Assert(momentDefsListPerVerse.Count == 5);
            Debug.Assert(blockMsPositions.Count == 11);
            Debug.Assert(blockMsDurations.Count == 11);

            Voice voice = new Voice(null, 0); // midiChannel 0

            int blockIndex = 0;
            LocalizedMidiDurationDef rmdd = new LocalizedMidiDurationDef(blockMsDurations[blockIndex]);
            Debug.Assert(rmdd.MsDuration > 0);

            rmdd.MsPosition = blockMsPositions[blockIndex];
            voice.LocalizedMidiDurationDefs.Add(rmdd);

            for(int verseIndex = 0; verseIndex < 5; ++verseIndex)
            {
                blockIndex++;

                List<MomentDef> momentDefs = momentDefsListPerVerse[verseIndex];

                for(int momentDefIndex = 0; momentDefIndex < momentDefs.Count; ++momentDefIndex)
                {
                    MomentDef momentDef = momentDefs[momentDefIndex];
                    momentDef.MsPosition += blockMsPositions[blockIndex];

                    int restWidth = momentDef.MsWidth - momentDef.MaximumMsDuration;
                    rmdd = null;
                    if(restWidth > 0)
                    {
                        momentDef.MsWidth -= restWidth;
                        rmdd = new LocalizedMidiDurationDef(restWidth);
                        Debug.Assert(rmdd.MsDuration > 0);

                        rmdd.MsPosition = momentDef.MsPosition + momentDef.MsWidth;
                    }

                    MidiChordDef mcd = momentDef.MidiChordDefs[0];
                    LocalizedMidiDurationDef lmdd = new LocalizedMidiDurationDef(mcd, momentDef.MsPosition, momentDef.MsWidth);
                    Debug.Assert(lmdd.MsDuration > 0);
                    
                    voice.LocalizedMidiDurationDefs.Add(lmdd);

                    if(rmdd != null)
                    {
                        voice.LocalizedMidiDurationDefs.Add(rmdd);
                    }
                }

                blockIndex++;
                rmdd = new LocalizedMidiDurationDef(blockMsDurations[blockIndex]);
                Debug.Assert(rmdd.MsDuration > 0);

                rmdd.MsPosition = blockMsPositions[blockIndex];
                voice.LocalizedMidiDurationDefs.Add(rmdd);
            }
            return voice;
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
