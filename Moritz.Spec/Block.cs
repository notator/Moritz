using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class Block : IVoiceDefContainer
    {
        protected Block() { } // called by MainBlock constructor

        /// <summary>
        /// A Block contains a list of voiceDefs, that can be of both kinds: Trks and InputVoiceDefs. A Seq can only contain Trks.
        /// This constructor uses its arguments' voiceDefs directly in the Block so, if the arguments needs to be used again, pass a clone.
        /// <para>The seq's Trks and the inputVoiceDefs (if any) are cast to VoiceDefs, and then padded at the beginning and end with rests
        /// so that they all start at the beginning of the Block and have the same duration.</para>
        /// <para>The Block's AbsMsPosition is set to the seq's AbsMsPosition.</para>
        /// <para>There must be at least one MidiChordDef at the start of the Block (possibly following a ClefChangeDef), and
        /// at least one MidiChordDef ends at its end.</para>
        /// <para>If an original voiceDef is empty or contains a single restDef, it will be altered to contain a single rest having the
        /// same duration as the other voiceDefs.</para>
        /// <para>For further documentation about Block consistency, see its private AssertBlockConsistency() function.
        /// </summary>
        /// <param name="seq">Cannot be null, and must have Trks</param>
        /// <param name="inputVoiceDefs">This list can be null or empty</param>
        public Block(Seq seq, IReadOnlyList<int> approxBarlineMsPositions, List<InputVoiceDef> inputVoiceDefs = null)
        {
            FinalizeBlock(seq, approxBarlineMsPositions, inputVoiceDefs);
        }

        // Also called by other, specialised Block constructors
        protected void FinalizeBlock(Seq seq, IReadOnlyList<int> approxBarlineMsPositions, List<InputVoiceDef> inputVoiceDefs = null)
        {
            Debug.Assert(seq.IsNormalized);

            AbsMsPosition = seq.AbsMsPosition;

            foreach(Trk trk in seq.Trks)
            {
                trk.Container = null;
                _voiceDefs.Add(trk);
            }

            if(inputVoiceDefs != null)
            {
                foreach(InputVoiceDef ivd in inputVoiceDefs)
                {
                    ivd.Container = null;
                    _voiceDefs.Add(ivd);
                }
            }

            Blockify();

            AddBarlines(approxBarlineMsPositions);

            AssertNonEmptyBlockConsistency();
        }

        /// <summary>
        /// A deep clone of the Block
        /// </summary>
        public Block Clone()
        {
            List<Trk> clonedTrks = new List<Trk>();
            List<int> midiChannelPerOutputVoice = new List<int>();
            foreach(Trk trk in Trks)
            {
                Trk trkClone = trk.Clone();
                clonedTrks.Add(trkClone);
                midiChannelPerOutputVoice.Add(trk.MidiChannel);
            }
            Seq clonedSeq = new Seq(this.AbsMsPosition, clonedTrks, midiChannelPerOutputVoice);
            List<InputVoiceDef> clonedInputVoiceDefs = new List<Spec.InputVoiceDef>();
            foreach(InputVoiceDef ivd in InputVoiceDefs)
            {
                clonedInputVoiceDefs.Add(ivd.Clone());
            }
            Block clone = new Block(clonedSeq, this.BarlineMsPositionsReBlock, clonedInputVoiceDefs);

            return clone;
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

            AssertNonEmptyBlockConsistency();
        }

        /// <summary>
        /// Adds each barline at the nearest end msPosition of any IUniqueDef in the Block.
        /// An exception is thrown either:
        ///    1) if approxBarlinePositionReBlock is greater than the msDuration of the block,
        /// or 2) if an attempt is made to add a barline that already exists.
        /// Barlines can be added in any order.
        /// If an existing barline has no associated IUniqueDef in an inputVoiceDef, it is moved to the nearest one.
        /// This function ends by sorting the block's _barlineMsPositionsReBlock into ascending order.
        /// </summary>
        protected void AddBarlines(IReadOnlyList<int> barlineMsPositionsReBlock)
        {
            #region conditions
            int msDuration = this.MsDuration;
            for(int i = 0; i < barlineMsPositionsReBlock.Count; ++i)
            {
                int msPosition = barlineMsPositionsReBlock[i];
                Debug.Assert(msPosition <= this.MsDuration);
                for(int j = i + 1; j < barlineMsPositionsReBlock.Count; ++j)
                {
                    Debug.Assert(msPosition != barlineMsPositionsReBlock[j], "Error: Duplicate barline msPositions.");
                }
            }
            #endregion conditions

            foreach(int msPosition in barlineMsPositionsReBlock)
            {
                #region conditions
                Debug.Assert(msPosition <= this.MsDuration);
                #endregion conditions

                int barlineMsPos = 0;
                int diff = int.MaxValue;
                foreach(VoiceDef voiceDef in this._voiceDefs)
                {
                    for(int uidIndex = voiceDef.Count - 1; uidIndex >= 0; --uidIndex)
                    {
                        int absPos = voiceDef[uidIndex].MsPositionReFirstUD + voiceDef[uidIndex].MsDuration;
                        int localDiff = Math.Abs(msPosition - absPos);
                        if(localDiff < diff)
                        {
                            diff = localDiff;
                            barlineMsPos = absPos;
                        }
                        if(diff == 0)
                        {
                            break;
                        }
                    }
                    if(diff == 0)
                    {
                        break;
                    }
                }

                Debug.Assert(!_barlineMsPositionsReBlock.Contains(barlineMsPos), "Error: Cannot add barline at duplicate position.");

                _barlineMsPositionsReBlock.Add(barlineMsPos);
            }

            if(InputVoiceDefs != null && InputVoiceDefs.Count > 0)
            {
                AdjustBarlinePositionsForInputVoices();
            }

            _barlineMsPositionsReBlock.Sort();
        }

        /// <summary>
        /// Barlines are moved to the end of the nearest existing IUniqueDef in the inputVoiceDefs.
        /// If the position to which it would be moved is already occupied, it is simply removed.
        /// </summary>
        private void AdjustBarlinePositionsForInputVoices()
        {
            List<int> newMsPositions = new List<int>();

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
                int closestMsPosition = closest.MsPositionReFirstUD + closest.MsDuration;
                if(!newMsPositions.Contains(closestMsPosition))
                {
                    newMsPositions.Add(closestMsPosition);
                }
            }

            _barlineMsPositionsReBlock = newMsPositions;
        }

        public void Concat(Block block2)
        {
            Debug.Assert(_voiceDefs.Count == block2._voiceDefs.Count);

            IReadOnlyList<int> block2BarlineMsPositions = block2.BarlineMsPositionsReBlock;
            foreach(int msPosition in block2BarlineMsPositions)
            {
                _barlineMsPositionsReBlock.Add(this.MsDuration + msPosition);
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
                vd1.RemoveDuplicateClefChanges();
                vd1.AgglomerateRests();
                vd1.Container = this;
            }

            AssertNonEmptyBlockConsistency();
        }

        /// <summary>
        /// Pads the Block with rests at the beginning and end of each VoiceDef where necessary.
        /// Agglommerates rests.
        /// </summary>
        protected void Blockify()
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
                        if(voiceDef is InputVoiceDef)
                        {
                            voiceDef.Insert(0, new InputRestDef(0, startRestMsDuration));
                        }
                        else
                        {
                            voiceDef.Insert(0, new MidiRestDef(0, startRestMsDuration));
                        }
                    }

                    int endOfTrkMsPositionReFirstIUD = voiceDef.EndMsPositionReFirstIUD;
                    int endRestMsDuration = blockMsDuration - endOfTrkMsPositionReFirstIUD;
                    if(endRestMsDuration > 0)
                    {
                        if(voiceDef is InputVoiceDef)
                        {
                            voiceDef.UniqueDefs.Add(new InputRestDef(endOfTrkMsPositionReFirstIUD, endRestMsDuration));
                        }
                        else
                        {
                            voiceDef.UniqueDefs.Add(new MidiRestDef(endOfTrkMsPositionReFirstIUD, endRestMsDuration));
                        }
                    }
                    voiceDef.AgglomerateRests();
                }
                else
                {
                    voiceDef.Add(new MidiRestDef(0, blockMsDuration));
                }

                voiceDef.MsPositionReContainer = 0;
                voiceDef.Container = this;
            }
        }

        /// <summary>
        /// A non-empty Block must fulfill the following criteria:
        /// The Trks may contain any combination of MidiRestDef, MidiChordDef, CautionaryChordDef and ClefChangeDef.
        /// The InputVoiceDefs may contain any combination of InputRestDef, InputChordDef, CautionaryChordDef and ClefChangeDef.
        /// <para>1. The first VoiceDef in a Block must be a Trk.</para>
        /// <para>2. Trks precede InputVoiceDefs (if any) in the _voiceDefs list.</para>
        /// <para>3. All voiceDefs start at MsPositionReContainer=0 and have the same MsDuration.</para>
        /// <para>4. UniqueDef.MsPositionReFirstIUD attributes are all set correctly (starting at 0).</para>
        /// <para>5. A RestDef is never followed by another RestDef (RestDefs have been agglomerated).</para>
        /// <para>6. In Blocks, Trk and InputVoiceDef objects can additionally contain CautionaryChordDefs and ClefChangeDefs (See Seq and InputVoiceDef).</para>
        /// <para>7. There may not be more than 4 InputVoiceDefs</para>
        /// <para>8. If there are any barlines, they are in ascending order with no duplicates. And the final barline may not be beyond the end of the block.</para>
        /// <para>9. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefChangeDef, or with a CautionaryChordDef.</para>
        /// </summary> 
        protected void AssertNonEmptyBlockConsistency()
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

            #region 3. All voiceDefs must begin at MsPositionReContainer=0 and have the same MsDuration. (InputVoiceDefs can have msDuration == 0.)
            int blockMsDuration = MsDuration;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                Debug.Assert(voiceDef.MsPositionReContainer == 0, "All voiceDefs in a block must begin at MsPosition=0");
                if(voiceDef is Trk)
                {
                    Debug.Assert(voiceDef.MsDuration == blockMsDuration, "All Trks in a block must have the same duration.");
                }
                else if(voiceDef.MsDuration > 0)
                {
                    Debug.Assert(voiceDef.MsDuration == blockMsDuration, "All InputVoiceDefs in a block must either have msDuration == 0 or msDuration == blockMsDuration.");
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

            #region 6. In Blocks, Trk and InputVoiceDef objects can additionally contain CautionaryChordDefs and ClefChangeDefs (See Seq and InputVoiceDef).
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

            #region 8. If there are any barlines, they must be in ascending order with no duplicates, and the final barline may not be beyond the end of the block.
            if(_barlineMsPositionsReBlock.Count > 0)
            {
                int prevPos = -1;
                foreach(int pos in _barlineMsPositionsReBlock)
                {
                    Debug.Assert(pos > prevPos, "barlines must be in ascending order with no duplicates.");
                    prevPos = pos;
                }
                Debug.Assert(MsDuration >= _barlineMsPositionsReBlock[_barlineMsPositionsReBlock.Count - 1], "The final barline may not be beyond the end of the block.");
            }
            #endregion 9. If there are any barlines, the final barline may not be beyond the end of the block.

            #region 9. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefChangeDef, or with a CautionaryChordDef.
            bool hasCorrectBeginning = false;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef is Trk)
                {
                    if((voiceDef.UniqueDefs.Count > 0 && ((voiceDef.UniqueDefs[0] is MidiChordDef) || (voiceDef.UniqueDefs[0] is CautionaryChordDef)))
                    || (voiceDef.UniqueDefs.Count > 1 && voiceDef.UniqueDefs[0] is ClefChangeDef && voiceDef.UniqueDefs[1] is MidiChordDef))
                    {
                        hasCorrectBeginning = true;
                        break;
                    }
                }
            }
            Debug.Assert(hasCorrectBeginning, "At least one Trk must start with a MidiChordDef, possibly preceded by a ClefChangeDef");

            #endregion
        }

        #region envelopes
        /// <summary>
        /// This function does not change the MsDuration of the Block.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(Envelope envelope, double distortion)
        {
            AssertNonEmptyBlockConsistency();
            int originalMsDuration = MsDuration;
            List<int> originalMsPositions = GetMsPositions();
            Dictionary<int, int> warpDict = new Dictionary<int, int>();
            #region get warpDict
            List<int> newMsPositions = envelope.TimeWarp(originalMsPositions, distortion);

            for(int i = 0; i < newMsPositions.Count; ++i)
            {
                warpDict.Add(originalMsPositions[i], newMsPositions[i]);
            }
            #endregion get warpDict

            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                List<IUniqueDef> iuds = voiceDef.UniqueDefs;
                IUniqueDef iud = null;
                int msPos = 0;
                for(int i = 1; i < iuds.Count; ++i)
                {
                    iud = iuds[i - 1];
                    msPos = warpDict[iud.MsPositionReFirstUD];
                    iud.MsPositionReFirstUD = msPos;
                    iud.MsDuration = warpDict[iuds[i].MsPositionReFirstUD] - msPos;
                    msPos += iud.MsDuration;
                }
                iud = iuds[iuds.Count - 1];
                iud.MsPositionReFirstUD = msPos;
                iud.MsDuration = originalMsDuration - msPos;
            }

            Debug.Assert(originalMsDuration == MsDuration);

            for(int i = 0; i < _barlineMsPositionsReBlock.Count; ++i)
            {
                Debug.Assert(warpDict.ContainsKey(_barlineMsPositionsReBlock[i]));
                _barlineMsPositionsReBlock[i] = warpDict[_barlineMsPositionsReBlock[i]];
            }

            AssertNonEmptyBlockConsistency();
        }

        /// <summary>
        /// Returns a list containing the msPositions of all IUniqueDefs plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                foreach(IUniqueDef iud in voiceDef)
                {
                    int msPos = iud.MsPositionReFirstUD;
                    if(!originalMsPositions.Contains(msPos))
                    {
                        originalMsPositions.Add(msPos);
                    }
                }
                originalMsPositions.Sort();
            }
            originalMsPositions.Add(originalMsDuration);
            return originalMsPositions;
        }

        public void SetPitchWheelSliders(Envelope envelope)
        {
            #region condition
            if(envelope.Domain != 127)
            {
                throw new ArgumentException($"{nameof(envelope.Domain)} must be 127.");
            }
            #endregion condition

            List<int> msPositions = GetMsPositions();
            Dictionary<int, int> pitchWheelValuesPerMsPosition = envelope.GetValuePerMsPosition(msPositions);

            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                Trk trk = voiceDef as Trk;
                if(trk != null)
                {
                    trk.SetPitchWheelSliders(pitchWheelValuesPerMsPosition);
                }
            }
        }

        #endregion envelopes

        protected void SetMsPositions()
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
                AssertNonEmptyBlockConsistency(); // all Trks and InputVoiceDefs have MsPositionReSeq == 0, and are the same length.
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

        protected int _absMsPosition = 0;

        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set
            {
                Debug.Assert(value >= 0);
                _absMsPosition = value;
            }
        }

        public IReadOnlyList<int> BarlineMsPositionsReBlock
        {
            get { return _barlineMsPositionsReBlock.AsReadOnly(); }
        }

        // If barlines are added to this list using the protected AddBarlines() function,
        // they will be moved to the end of the nearest chord or rest.
        protected List<int> _barlineMsPositionsReBlock = new List<int>();

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

        protected List<VoiceDef> _voiceDefs = new List<VoiceDef>();
    }
}
