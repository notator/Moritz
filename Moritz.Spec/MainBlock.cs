using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class MainBlock : Block
    {
        /// <summary>
        /// A Block contains a list of VoiceDefs consisting of a group of Trks followed by InputVoiceDefs.
        /// This constructor creates a MainBlock consisting of the concatenation of the Blocks in the argument blockList.
        /// The initialClefs are set to the clefs in initialClefsPerChannel.
        /// initialClefsPerChannel contains the clefs for the Trks followed by the clefs for the clefs for the InputVoiceDefs.
        /// Note that initialClefsPerChannel contains a clef for each channel, regardless of whether it is going to be printed or not. 
        /// The channels for both Trks and InputVoiceDefs are in ascending order. Their order from top to bottom in the score is determined later.
        /// </summary>
        /// <param name="initialClefPerChannel">The clefs to set at the start of each Trk followed by the clefs for each InputVoiceDef</param>
        /// <param name="blockList">A list of Blocks that will be concatenated to become this MainBlock.</param>
        public MainBlock(List<string> initialClefPerChannel, List<Block> blockList)
            : base()
        {
            Debug.Assert(blockList != null && blockList.Count > 0);

            Block block1 = blockList[0];
            int nTrks = block1.Trks.Count; 
            int nInputVoiceDefs = block1.InputVoiceDefs.Count;

            #region conditions
            Debug.Assert(initialClefPerChannel.Count == (nTrks + nInputVoiceDefs));
            foreach(Block block in blockList)
            {
                Debug.Assert(block.Trks.Count == nTrks);
                Debug.Assert(block.InputVoiceDefs.Count == nInputVoiceDefs);
                for(int trkIndex = 0; trkIndex < nTrks; ++trkIndex)
                {
                    // Achtung: Trk midiChannels are those defined in the algorithm's MidiChannelIndexPerOutputVoice.
                    // The output channels must be in ascending oder, **_but_do_not_need_to_be_contiguous_**.
                    // See comment at CompositionAlgorithm.MidiChannelIndexPerOutputVoice.
                    Debug.Assert(block.Trks[trkIndex].MidiChannel == block1.Trks[trkIndex].MidiChannel);
                }
                for(int ivdIndex = 0; ivdIndex < nInputVoiceDefs; ++ivdIndex)
                {
                    // Input channels are in ascending order, starting at 0, and are contiguous.
                    Debug.Assert(block.Trks[ivdIndex].MidiChannel == ivdIndex);
                }
            }
            #endregion conditions

            for(int i = 0; i < nTrks; ++i)
            {
                int channel = block1.Trks[i].MidiChannel;
                VoiceDef trk = new Trk(channel);
                trk.Add(new ClefChangeDef(initialClefPerChannel[channel], 0));
                _voiceDefs.Add(trk);
            }

            int inputVoiceIndex = Trks.Count;
            for(int i = 0; i < nInputVoiceDefs; ++i)
            {
                VoiceDef inputVoiceDef = new InputVoiceDef(i);
                inputVoiceDef.Add(new ClefChangeDef(initialClefPerChannel[inputVoiceIndex++], 0));
                _voiceDefs.Add(inputVoiceDef);
            }

            foreach(Block block in blockList)
            {
                this.Concat(block);
            }
        }

        /// <summary>
        /// When this function returns, the block has been consumed, and is no longer usable.
        /// </summary>
        public List<List<VoiceDef>> ConvertToBars()
        {
            #region conditions
            Debug.Assert(BarlineMsPositionsReBlock.Count > 0, "Block must have at least one barline.");
            Debug.Assert(BarlineMsPositionsReBlock[BarlineMsPositionsReBlock.Count - 1] == AbsMsPosition + MsDuration, "The final barline must be at end of the block.");
            #endregion conditions

            List<int> barlineMsPositions = new List<int>(BarlineMsPositionsReBlock);
            int finalBarlineMsPosition = barlineMsPositions[barlineMsPositions.Count - 1];

            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();

            int barlineIndex = 0;
            while(AbsMsPosition < finalBarlineMsPosition)
            {
                int barlineEndMsPosition = barlineMsPositions[barlineIndex++];
                for(int i = 1; i < _barlineMsPositionsReBlock.Count; ++i)
                {
                    _barlineMsPositionsReBlock[i] -= _barlineMsPositionsReBlock[0];
                }
                _barlineMsPositionsReBlock.RemoveAt(0);
                List<VoiceDef> bar = PopBar(barlineEndMsPosition);
                bars.Add(bar);
            }

            return bars;
        }

        /// <summary>
        /// Creates a "bar" which is a list of voiceDefs containing IUniqueDefs that begin before barlineEndMsPosition,
        /// and removes these IUniqueDefs from the current block.
        /// </summary>
        /// <param name="endBarlineAbsMsPosition"></param>
        /// <returns>The popped bar</returns>
        private List<VoiceDef> PopBar(int endBarlineAbsMsPosition)
        {
            Debug.Assert(AbsMsPosition < endBarlineAbsMsPosition);
            AssertNonEmptyBlockConsistency();

            List<VoiceDef> poppedBar = new List<VoiceDef>();
            List<VoiceDef> remainingBar = new List<VoiceDef>();
            int currentBlockAbsEndPos = this.AbsMsPosition + this.MsDuration;

            bool isLastBar = (currentBlockAbsEndPos == endBarlineAbsMsPosition);

            VoiceDef poppedBarVoice;
            VoiceDef remainingBarVoice;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                Trk outputVoice = voiceDef as Trk;
                InputVoiceDef inputVoice = voiceDef as InputVoiceDef;
                if(outputVoice != null)
                {
                    poppedBarVoice = new Trk(outputVoice.MidiChannel);
                    poppedBar.Add(poppedBarVoice);
                    remainingBarVoice = new Trk(outputVoice.MidiChannel);
                    remainingBar.Add(remainingBarVoice);
                }
                else
                {
                    poppedBarVoice = new InputVoiceDef(inputVoice.MidiChannel);
                    poppedBar.Add(poppedBarVoice);
                    remainingBarVoice = new InputVoiceDef(inputVoice.MidiChannel);
                    remainingBar.Add(remainingBarVoice);
                }
                foreach(IUniqueDef iud in voiceDef.UniqueDefs)
                {
                    int iudMsDuration = iud.MsDuration;
                    int iudAbsStartPos = this.AbsMsPosition + iud.MsPositionReFirstUD;
                    int iudAbsEndPos = iudAbsStartPos + iudMsDuration;

                    if(iudAbsStartPos >= endBarlineAbsMsPosition)
                    {
                        Debug.Assert(iudAbsEndPos <= currentBlockAbsEndPos);
                        if(iud is ClefChangeDef && iudAbsStartPos == endBarlineAbsMsPosition)
                        {
                            poppedBarVoice.UniqueDefs.Add(iud);
                        }
                        else
                        {
                            remainingBarVoice.UniqueDefs.Add(iud);
                        }
                    }
                    else if(iudAbsEndPos > endBarlineAbsMsPosition)
                    {
                        int durationBeforeBarline = endBarlineAbsMsPosition - iudAbsStartPos;
                        int durationAfterBarline = iudAbsEndPos - endBarlineAbsMsPosition;
                        if(iud is RestDef)
                        {
                            // This is a rest. Split it.
                            RestDef firstRestHalf = new RestDef(iudAbsStartPos, durationBeforeBarline);
                            poppedBarVoice.UniqueDefs.Add(firstRestHalf);

                            RestDef secondRestHalf = new RestDef(endBarlineAbsMsPosition, durationAfterBarline);
                            remainingBarVoice.UniqueDefs.Add(secondRestHalf);
                        }
                        else if(iud is CautionaryChordDef)
                        {
                            // This is a cautionary chord. Set the position of the following barline, and
                            // Add a CautionaryChordDef at the beginning of the following bar.
                            iud.MsDuration = endBarlineAbsMsPosition - iudAbsStartPos;
                            poppedBarVoice.UniqueDefs.Add(iud);

                            Debug.Assert(remainingBarVoice.UniqueDefs.Count == 0);
                            CautionaryChordDef secondLmdd = new CautionaryChordDef((IUniqueChordDef)iud, 0, durationAfterBarline);
                            remainingBarVoice.UniqueDefs.Add(secondLmdd);
                        }
                        else if(iud is MidiChordDef || iud is InputChordDef)
                        {
                            IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
                            uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
                            poppedBarVoice.UniqueDefs.Add(uniqueChordDef);

                            Debug.Assert(remainingBarVoice.UniqueDefs.Count == 0);
                            CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
                            remainingBarVoice.UniqueDefs.Add(ccd);
                        }
                    }
                    else
                    {
                        Debug.Assert(iudAbsEndPos <= endBarlineAbsMsPosition && iudAbsStartPos >= AbsMsPosition);
                        poppedBarVoice.UniqueDefs.Add(iud);
                    }
                }
            }

            this.AbsMsPosition = endBarlineAbsMsPosition;

            this._voiceDefs = remainingBar;
            SetMsPositions();

            if(!isLastBar)
            {
                // _voiceDefs is not empty
                AssertNonEmptyBlockConsistency();
            }

            return poppedBar;
        }

    }
}
