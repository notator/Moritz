using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;

namespace Moritz.Spec
{
    public class Block : IVoiceDefContainer
    {
        /// <summary>
        /// A Block contains a list of voiceDefs, that can be of both kinds: Trks and InputVoiceDefs. A Seq can only contain Trks.
        /// This constructor converts its argument to a Block so, if the argument needs to be preserved, pass a clone.
        /// <para>The seq's Trks are cast to VoiceDefs, and then padded at the beginning and end with rests
        /// so that they all start at the beginning of the Block and have the same duration.</para>
        /// <para>The Block's AbsMsPosition is set to the seq's AbsMsPosition.</para>
        /// <para>There is at least one MidiChordDef at the start of the Block, and at least one MidiChordDef ends at its end.</para>
        /// <para>If an original seq.trk.UniqueDefs list is empty or contains a single restDef, the corresponding
        /// voiceDef will contain a single rest having the same duration as the other trks.</para>
        /// <para>For further documentation about Block consistency, see its private AssertBlockConsistency() function.
        /// </summary>
        /// <param name="seq">cannot be null, and must have Trks</param>
        /// <param name="barlineMsPositions">can be null or empty</param>
        public Block(Seq seq, List<int> barlineMsPositions)
        {
            Debug.Assert(seq.IsNormalized);

            AbsMsPosition = seq.AbsMsPosition;

            foreach(Trk trk in seq.Trks)
            {
                trk.Container = null;
                _voiceDefs.Add(trk);
            } 

            Blockify();

            foreach(Trk trk in seq.Trks)
            {
                trk.Container = this;
            }

            if(barlineMsPositions != null)
            {
                BarlineMsPositions = new List<int>(barlineMsPositions);
            }

            AssertBlockConsistency();
        }

        public void AddInputVoice(InputVoiceDef ivd)
        {
            Debug.Assert((ivd.MsPositionReContainer + ivd.MsDuration) <= MsDuration);
            Debug.Assert(ivd.MidiChannel >= 0 && ivd.MidiChannel <= 3);
            #region check for an existing InputVoiceDef having the same MidiChannel
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                InputVoiceDef existingInputVoiceDef = voiceDef as InputVoiceDef;
                if(existingInputVoiceDef != null)
                { 
                    Debug.Assert(existingInputVoiceDef.MidiChannel != ivd.MidiChannel, "Error: An InputVoiceDef with the same MidiChannel already exists.");
                }
            }
            #endregion

            int startPos = ivd.MsPositionReContainer;
            ivd.MsPositionReContainer = 0;
            foreach(IUniqueDef iud in ivd.UniqueDefs)
            {
                iud.MsPositionReFirstUD += startPos;
            }

            _voiceDefs.Add(ivd);

            Blockify();

            AssertBlockConsistency();
        }

        /// <summary>
        /// Pads the Block with rests at the beginning and end of each VoiceDef where necessary.
        /// Agglommerates rests.
        /// </summary>
        private void Blockify()
        {
            int blockMsDuration = MsDuration; // MsDuration is a property that looks at UniqueDefs in this block.

            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef.UniqueDefs.Count > 0)
                {
                    IUniqueDef firstIUD = voiceDef.UniqueDefs[0];
                    int startRestMsDuration = voiceDef.MsPositionReContainer;
                    if(startRestMsDuration > 0)
                    {
                        voiceDef.Insert(0, new RestDef(0, startRestMsDuration));
                        voiceDef.MsPositionReContainer = 0;
                    }

                    int endOfTrkMsPositionReFirstIUD = voiceDef.EndMsPositionReFirstIUD;
                    int endRestMsDuration = blockMsDuration - endOfTrkMsPositionReFirstIUD;
                    if(endRestMsDuration > 0)
                    {
                        voiceDef.UniqueDefs.Add(new RestDef(endOfTrkMsPositionReFirstIUD, endRestMsDuration));
                    }
                    voiceDef.AgglomerateRests();
                }
                else
                {
                    voiceDef.Add(new RestDef(0, blockMsDuration));
                }
            }
        }

        public void Concat(Block block2)
        {
            Debug.Assert(_voiceDefs.Count == block2._voiceDefs.Count);

            List<int> block2BarlineMsPositions = block2.BarlineMsPositions;
            foreach(int msPosition in block2BarlineMsPositions)
            {
                BarlineMsPositions.Add(this.MsDuration + msPosition);
            }

            for(int i = 0; i < _voiceDefs.Count; ++i)
            {
                VoiceDef vd1 = _voiceDefs[i];
                VoiceDef vd2 = block2._voiceDefs[i];

                Trk trk1 = vd1 as Trk; 
                Trk trk2 = vd2 as Trk;

                InputVoiceDef ivd1 = vd1 as InputVoiceDef;
                InputVoiceDef ivd2 = vd2 as InputVoiceDef;

                Debug.Assert((trk1 != null && trk2 != null) || (ivd1 != null && ivd2 != null));

                vd1.Container = null;
                vd2.Container = null;
                vd1.AddRange(vd2);
                vd1.AgglomerateRests();
                vd1.Container = this;
            }

            AssertBlockConsistency();
        }

        /// <summary>
        /// A deep clone of the Block
        /// </summary>
        public Block Clone()
        {
            List<Trk> trks = new List<Trk>();
            List<int> midiChannelPerOutputVoice = new List<int>();
            foreach(Trk trk in Trks)
            {
                Trk trkClone = trk.Clone();
                trks.Add(trkClone);
                midiChannelPerOutputVoice.Add(trk.MidiChannel);
            }
            Seq seq = new Seq(this.AbsMsPosition, trks, midiChannelPerOutputVoice);
            Block clone = new Block(seq, BarlineMsPositions);

            foreach(InputVoiceDef ivd in InputVoiceDefs)
            {
                clone.AddInputVoice(ivd.Clone());
            }

            return clone;
        }

        /// <summary>
        /// A Block must fulfill the following criteria:
        /// The Trks may contain any combination of RestDef, MidiChordDef and ClefChangeDef.
        /// The InputVoiceDefs may contain any combination of RestDef, InputChordDef and ClefChangeDef.
        /// <para>1. The first VoiceDef in a Block must be a Trk.</para>
        /// <para>2. Trks precede InputVoiceDefs (if any) in the _voiceDefs list.</para>
        /// <para>3. All voiceDefs start at MsPositionReContainer=0 and have the same MsDuration.</para>
        /// <para>4. UniqueDef.MsPositionReFirstIUD attributes are all set correctly (starting at 0).</para>
        /// <para>5. A RestDef is never followed by another RestDef (RestDefs have been agglomerated).</para>
        /// <para>6. In Blocks, Trk and InputVoiceDef objects can additionally contain CautionaryChordDefs (See Seq and InputVoiceDef).</para>
        /// <para>7. There may not be more than 4 InputVoiceDefs</para>
        /// <para>8. The final barline may not be beyond the end of the block.</para>
        /// </summary>
        private void AssertBlockConsistency()
        {
            #region 1. The first VoiceDef in a Block must be a Trk.
            Debug.Assert(_voiceDefs[0] is Trk, "The first VoiceDef in a Block must be a Trk.");
            #endregion

            #region 2. Trks precede InputVoiceDefs (if any) in the _voiceDefs list.
            bool inputVoiceDefFound = false;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef is InputVoiceDef)
                {
                    inputVoiceDefFound = true;
                }
                if(voiceDef is Trk)
                {
                    Debug.Assert(inputVoiceDefFound == false, "Trks must precede InputVoiceDefs (if any) in the _voiceDefs list.");
                }
            }
            #endregion

            #region 3. All voiceDefs must begin at MsPositionReContainer=0 and have the same MsDuration
            int blockMsDuration = MsDuration;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef.MsPositionReContainer > 0 || voiceDef.MsDuration != blockMsDuration)
                {
                    Debug.Assert(false, "All voiceDefs in a block must begin at MsPosition=0 and have the same MsDuration.");
                }
            }
            #endregion

            #region 4. UniqueDef.MsPositionReFirstIUD attributes are all set correctly (starting at 0)
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                int msPositionReFirstIUD = 0;
                foreach(IUniqueDef iud in voiceDef.UniqueDefs)
                {
                    Debug.Assert((iud.MsPositionReFirstUD == msPositionReFirstIUD), "Error in uniqueDef.MsPositionReFirstIUD.");
                    msPositionReFirstIUD += iud.MsDuration;
                }
            }
            #endregion

            #region 5. A RestDef is never followed by another RestDef (RestDefs are agglomerated).
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                bool restFound = false;
                foreach(IUniqueDef iud in voiceDef.UniqueDefs)
                {
                    if(iud is RestDef)
                    {
                        if(restFound)
                        {
                            Debug.Assert(false, "Consecutive rests found!");
                        }
                        restFound = true;
                    }
                    else
                    {
                        restFound = false;
                    }
                }
            }
            #endregion

            #region 6. In Blocks, Trk and InputVoiceDef objects can additionally contain CautionaryChordDefs (See Seq and InputVoiceDef).
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                voiceDef.AssertConsistentInBlock();
            }
            #endregion

            #region 7. There may not be more than 4 InputVoiceDefs
            int nInputVoiceDefs = 0;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef is InputVoiceDef)
                {
                    nInputVoiceDefs++;
                }
            }
            Debug.Assert((nInputVoiceDefs <= 4), "There may not be more than 4 InputVoiceDefs.");
            #endregion 7

            #region 8. The final barline may not be beyond the end of the block.
            if(BarlineMsPositions.Count > 0)
            {
                Debug.Assert(MsDuration >= BarlineMsPositions[BarlineMsPositions.Count - 1], "The final barline may not be beyond the end of the block.");
            }
            #endregion 8. The final barline may not be beyond the end of the block.
        }

        /// <summary>
        /// The argument warp is a list of doubles, in ascending order, beginning with 0 and ending with 1.
        /// The doubles represent moments in the original duration that will be separated from each other
        /// by equal durations when the function returns. The MsDuration of the Block is not changed.
        /// </summary>
        public void WarpDurations(List<double> warp)
        {
            AssertBlockConsistency();
            int blockMsDuration = MsDuration;

            Dictionary<int, int> warpedMsPositionsDict = new Dictionary<int, int>();
            warpedMsPositionsDict.Add(MsDuration, MsDuration);

            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                IReadOnlyList<int> originalPositions = voiceDef.UDMsPositionsReFirstUD;
                voiceDef.WarpDurations(warp);
                IReadOnlyList<int> warpedPositions = voiceDef.UDMsPositionsReFirstUD;
                for(int i = 0; i < originalPositions.Count; ++i)
                {
                    if(!warpedMsPositionsDict.ContainsKey(originalPositions[i]))
                    {
                        warpedMsPositionsDict.Add(originalPositions[i], warpedPositions[i]);
                    }
                }
            }

            WarpBarlineMsPositions(warpedMsPositionsDict);

            Debug.Assert(blockMsDuration == MsDuration);
            AssertBlockConsistency();
        }

        private void WarpBarlineMsPositions(Dictionary<int, int> warpedMsPositionsDict)
        {
            for(int i = 0; i < BarlineMsPositions.Count; ++i)
            {
                Debug.Assert(warpedMsPositionsDict.ContainsKey(BarlineMsPositions[i]));
                BarlineMsPositions[i] = warpedMsPositionsDict[BarlineMsPositions[i]];
            }
        }

        /// <summary>
        /// When this function returns, the block has been consumed, and is no longer usable.
        /// </summary>
        public List<List<VoiceDef>> ConvertToBars()
        {
            #region conditions
            Debug.Assert(BarlineMsPositions.Count > 0, "Block must have at least one barline.");
            Debug.Assert(BarlineMsPositions[BarlineMsPositions.Count - 1] == AbsMsPosition + MsDuration, "The final barline must be at end of the block.");
            #endregion conditions

            List<int> barlineMsPositions = new List<int>( BarlineMsPositions );
            int finalBarlineMsPosition = barlineMsPositions[barlineMsPositions.Count - 1];

            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();

            int barlineIndex = 0;
            while(AbsMsPosition < finalBarlineMsPosition)
            {
                int barlineEndMsPosition = barlineMsPositions[barlineIndex++];
                for(int i = 1; i < BarlineMsPositions.Count; ++i)
                {
                    BarlineMsPositions[i] -= BarlineMsPositions[0];
                }
                BarlineMsPositions.RemoveAt(0);
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
            AssertBlockConsistency();

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
                        remainingBarVoice.UniqueDefs.Add(iud);
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
                AssertBlockConsistency();
            }

            return poppedBar;
        }

        private void SetMsPositions()
        {
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                int msPosition = 0;
                foreach(IUniqueDef iud in voiceDef)
                {
                    iud.MsPositionReFirstUD = msPosition;
                    msPosition += iud.MsDuration;
                }
            }
        }

        /// <summary>
        /// The duration between the beginning of the block and the end of the last UniqueDef in the block.
        /// Setting this value stretches or compresses the msDurations of all the voiceDefs and their contained UniqueDefs.
        /// </summary>
        public int MsDuration
        {
            get
            {
                int msDuration = 0;
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    if(voiceDef.UniqueDefs.Count > 0)
                    {
                        IUniqueDef lastIUD = voiceDef.UniqueDefs[voiceDef.UniqueDefs.Count - 1];
                        int endMsPosReBlock = voiceDef.MsPositionReContainer + lastIUD.MsPositionReFirstUD + lastIUD.MsDuration;
                        msDuration = (msDuration < endMsPosReBlock) ? endMsPosReBlock : msDuration;
                    }
                }
                return msDuration;
            }
            set
            {
                Debug.Assert(_voiceDefs.Count > 0);
                AssertBlockConsistency(); // all Trks and InputVoiceDefs have MsPositionReSeq == 0, and are the same length.
                int currentDuration = MsDuration;
                double factor = ((double)value) / currentDuration;
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    voiceDef.MsDuration = (int)Math.Round(voiceDef.MsDuration * factor);
                    voiceDef.MsPositionReContainer = (int)Math.Round(voiceDef.MsPositionReContainer * factor);
                }
                int roundingError = value - MsDuration;
                if(roundingError != 0)
                {
                    foreach(VoiceDef voiceDef in _voiceDefs)
                    {
                        if((voiceDef.EndMsPositionReFirstIUD + roundingError) == value)
                        {
                            voiceDef.EndMsPositionReFirstIUD += roundingError;
                        }
                    }
                }
                Debug.Assert(MsDuration == value);
            }
        }

        private int _absMsPosition;

        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set
            {
                Debug.Assert(value >= 0);
                _absMsPosition = value;
            }
        }

        public IReadOnlyList<Trk> Trks
        {
            get
            {
                List<Trk> trks = new List<Trk>();
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    Trk trk = voiceDef as Trk;
                    if(trk != null)
                    {
                        trks.Add(trk);
                    }
                }
                return trks.AsReadOnly();
            }
        }

        public List<int> BarlineMsPositions = new List<int>();

        public List<InputVoiceDef> InputVoiceDefs
        {
            get
            {
                List<InputVoiceDef> inputVoiceDefs = new List<InputVoiceDef>();
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    InputVoiceDef inputVoiceDef = voiceDef as InputVoiceDef;
                    if(inputVoiceDef != null)
                    {
                        inputVoiceDefs.Add(inputVoiceDef);
                    }
                }
                return inputVoiceDefs;
            }
        }

        private List<VoiceDef> _voiceDefs = new List<VoiceDef>();
    }
}
