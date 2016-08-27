using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class MainBlock : Block
    {
        /// <summary>
        /// A Block contains a list of VoiceDefs consisting of a group of Trks followed by InputVoiceDefs. (A Seq can only contain Trks.)
        /// This constructor creates an empty Block (at absBlockMsPosition = 0), having one empty Trk per OutputVoice and
        /// nInputVoiceDef empty InputVoiceDefs. The clefs at the beginnings of the Trks and InputVoiceDefs are set to those
        /// in the initialClefs parameter.
        /// </summary>
        /// <param name="initialClefPerChannel">The clefs to set at the start of the Trks and InputVoiceDefs</param>
        /// <param name="midiChannelIndexPerOutputVoice">The channels to be allocated to Trks.</param>
        /// <param name="nInputVoices">(optional) The number of InputVoiceDefs to create.</param>
        public MainBlock(List<string> initialClefPerChannel, IReadOnlyList<int> midiChannelIndexPerOutputVoice, int nInputVoices = 0)
            : base()
        {
            Debug.Assert(midiChannelIndexPerOutputVoice.Count > 0);
            Debug.Assert(initialClefPerChannel.Count == (midiChannelIndexPerOutputVoice.Count + nInputVoices));

            foreach(int channel in midiChannelIndexPerOutputVoice)
            {
                VoiceDef trk = new Trk(channel);
                trk.Add(new ClefChangeDef(initialClefPerChannel[channel], 0));
                _voiceDefs.Add(trk);
            }

            int inputVoiceIndex = midiChannelIndexPerOutputVoice.Count;
            for(int i = 0; i < nInputVoices; ++i)
            {
                VoiceDef inputVoiceDef = new InputVoiceDef(i);
                inputVoiceDef.Add(new ClefChangeDef(initialClefPerChannel[inputVoiceIndex++], 0));
                _voiceDefs.Add(inputVoiceDef);
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

        /// <summary>
        /// If an existing barline has no associated IUniqueDef in the inputVoiceDefs, it is moved to the nearest one.
        /// </summary>
        public void AdjustBarlinePositionsForInputVoices()
        {
            for(int bpIndex = 0; bpIndex < _barlineMsPositionsReBlock.Count; ++bpIndex)
            {
                int barlineMsPos = _barlineMsPositionsReBlock[bpIndex];
                IUniqueDef closest = null;
                int minDiff = int.MaxValue;
                foreach(InputVoiceDef inputVoiceDef in this.InputVoiceDefs)
                {
                    foreach(IUniqueDef iud in inputVoiceDef.UniqueDefs)
                    {
                        int diff = Math.Abs(barlineMsPos - (iud.MsPositionReFirstUD + iud.MsDuration));
                        if(diff < minDiff)
                        {
                            minDiff = diff;
                            closest = iud;
                        }
                    }
                }
                _barlineMsPositionsReBlock[bpIndex] = closest.MsPositionReFirstUD + closest.MsDuration;
            }
        }
    }
}
