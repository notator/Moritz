using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class Bar : ITrksContainer
    {
        protected Bar() { }

		public Bar(Seq argSeq, IReadOnlyList<InputVoiceDef> argInputVoiceDefs, int startMsPosReSeq, int endMsPosReSeq)
		{
			List<VoiceDef> argVoiceDefs = new List<VoiceDef>(argSeq.Trks);
			if(argInputVoiceDefs != null)
			{
				argVoiceDefs.AddRange(argInputVoiceDefs);
			}

			VoiceDef voiceDef = null;
			foreach(VoiceDef argVoiceDef in argVoiceDefs)
			{
				if(argVoiceDef is Trk argTrk)
				{
					voiceDef = new Trk(argVoiceDef.MidiChannel);
					this._voiceDefs.Add(voiceDef);
				}
				else
				{
					InputVoiceDef argInputVoiceDef = argVoiceDef as InputVoiceDef;
					voiceDef = new InputVoiceDef(argVoiceDef.MidiChannel, 0, new List<IUniqueDef>());
				}
				voiceDef.SetToRange(argVoiceDef, startMsPosReSeq, endMsPosReSeq);
				this._voiceDefs.Add(voiceDef);
			}

			FinalizeBar(argSeq, argInputVoiceDefs);
		}

		/// <summary>
		/// A Bar contains a list of voiceDefs, that can be of both kinds: Trks and InputVoiceDefs. A Seq can only contain Trks.
		/// It does not contain barlines. They are implicit, at the beginning and end of the Bar.
		/// This constructor uses its arguments' voiceDefs directly in the Bar so, if the arguments needs to be used again, pass a clone.
		/// <para>The seq's Trks and the inputVoiceDefs (if any) are cast to VoiceDefs, and then padded at the beginning and end with rests
		/// so that they all start at the beginning of the Bar and have the same duration.</para>
		/// <para>The Bar's AbsMsPosition is set to the seq's AbsMsPosition.</para>
		/// <para>There must be at least one MidiChordDef at the start of the Bar (possibly following a ClefDef), and
		/// at least one MidiChordDef ends at its end.</para>
		/// <para>If an original voiceDef is empty or contains a single restDef, it will be altered to contain a single rest having the
		/// same duration as the other voiceDefs.</para>
		/// <para>For further documentation about Bar consistency, see its private AssertBlockConsistency() function.
		/// </summary>
		/// <param name="seq">Cannot be null, and must have Trks</param>
		/// <param name="inputVoiceDefs">This list can be null or empty</param>
		public Bar(Seq seq, IReadOnlyList<InputVoiceDef> inputVoiceDefs = null)
        {
            FinalizeBar(seq, inputVoiceDefs);
        }

        // Also called by other, specialised Bar constructors
        protected void FinalizeBar(Seq seq, IReadOnlyList<InputVoiceDef> inputVoiceDefs = null)
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

            MakeBarConsistent();

            AssertNonEmptyBarConsistency();
        }

        /// <summary>
        /// A deep clone of the Bar
        /// </summary>
        public Bar Clone()
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
            Bar clone = new Bar(clonedSeq, clonedInputVoiceDefs);

            return clone;
        }

		/// Uses the argument barline msPositions as the EndBarlines of the returned bars (which don't contain barlines). 
		/// An exception is thrown if:
		///    1) the first argument value is less than or equal to 0.
		///    2) the argument contains duplicate msPositions.
		///    3) the argument is not in ascending order.
		///    4) a VoiceDef.MsPositionReContainer is not 0.
		///    5) an msPosition is not the endMsPosition of any IUniqueDef in the bar.
		public List<Bar> GetBars(List<int> barlineMsPositionsReThisBar, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
		{
			CheckBarlineMsPositions(barlineMsPositionsReThisBar);

			List<Bar> bars = new List<Bar>();
			int startMsPos = 0;
			foreach(int endMsPos in barlineMsPositionsReThisBar)
			{
				List<Trk> newSeqTrks = new List<Trk>();
				foreach(Trk trk in Trks)
				{
					Trk newSeqTrk = new Trk(trk.MidiChannel);
					newSeqTrk.SetToRange(trk, startMsPos, endMsPos);
					newSeqTrks.Add(newSeqTrk);
				}
				Seq newSeq = new Seq(startMsPos, newSeqTrks, midiChannelIndexPerOutputVoice);
				Bar bar = new Bar(newSeq, InputVoiceDefs, startMsPos, endMsPos);
				bars.Add(bar);
				startMsPos = endMsPos;
			}

			return bars;
		}

		/// <summary>
		/// An exception is thrown if:
		///    1) the first argument value is less than or equal to 0.
		///    2) the argument contains duplicate msPositions.
		///    3) the argument is not in ascending order.
		///    4) a VoiceDef.MsPositionReContainer is not 0.
		///    5) an msPosition is not the endMsPosition of any IUniqueDef in the bar.
		/// </summary>
		private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReThisBar)
		{
			Debug.Assert(barlineMsPositionsReThisBar[0] > 0, "The first msPosition must be greater than 0.");

			int msDuration = this.MsDuration;
			for(int i = 0; i < barlineMsPositionsReThisBar.Count; ++i)
			{
				int msPosition = barlineMsPositionsReThisBar[i];
				Debug.Assert(msPosition <= this.MsDuration);
				for(int j = i + 1; j < barlineMsPositionsReThisBar.Count; ++j)
				{
					Debug.Assert(msPosition != barlineMsPositionsReThisBar[j], "Error: Duplicate barline msPositions.");
				}
			}

			int currentMsPos = -1;
			foreach(int msPosition in barlineMsPositionsReThisBar)
			{
				Debug.Assert(msPosition > currentMsPos, "Value out of order.");
				currentMsPos = msPosition;
				bool found = false;
				foreach(VoiceDef voiceDef in VoiceDefs)
				{
					Debug.Assert(voiceDef.MsPositionReContainer == 0);
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
					{
						if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
						{
							found = true;
							break;
						}
					}
					if(found)
					{ 
						break;
					}
				}
				Debug.Assert(found == false, "Error: barline must be at the endMsPosition of at least one IUniqueDef.");
			}
		}

		public void AddInputVoice(InputVoiceDef ivd)
        {
            Debug.Assert((ivd.MsPositionReContainer + ivd.MsDuration) <= MsDuration);
            Debug.Assert(ivd.MidiChannel >= 0 && ivd.MidiChannel <= 3);
            #region check for an existing InputVoiceDef having the same MidiChannel
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef is InputVoiceDef existingInputVoiceDef)
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

            MakeBarConsistent();

            AssertNonEmptyBarConsistency();
        }

        public void Concat(Bar bar2)
        {
            Debug.Assert(_voiceDefs.Count == bar2._voiceDefs.Count);

            for(int i = 0; i < _voiceDefs.Count; ++i)
            {
                VoiceDef vd1 = _voiceDefs[i];
                VoiceDef vd2 = bar2._voiceDefs[i];

                Trk trk1 = vd1 as Trk;
                Trk trk2 = vd2 as Trk;

                InputVoiceDef ivd1 = vd1 as InputVoiceDef;
                InputVoiceDef ivd2 = vd2 as InputVoiceDef;

                Debug.Assert((trk1 != null && trk2 != null) || (ivd1 != null && ivd2 != null));

                vd1.Container = null;
                vd2.Container = null;
                vd1.AddRange(vd2);
                vd1.RemoveDuplicateClefDefs();
                vd1.AgglomerateRests();
                vd1.Container = this;
            }

            AssertNonEmptyBarConsistency();
        }

        /// <summary>
        /// Pads the Bar with rests at the beginning and end of each VoiceDef where necessary.
        /// Agglommerates rests.
        /// </summary>
        protected void MakeBarConsistent()
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
        /// A non-empty Bar must fulfill the following criteria:
        /// The Trks may contain any combination of MidiRestDef, MidiChordDef, CautionaryChordDef and ClefDef.
        /// The InputVoiceDefs may contain any combination of InputRestDef, InputChordDef, CautionaryChordDef and ClefDef.
        /// <para>1. The first VoiceDef in a Bar must be a Trk.</para>
        /// <para>2. All Trks must precede InputVoiceDefs (if any) in the _voiceDefs list.</para>
        /// <para>3. All voiceDefs start at MsPositionReContainer=0 and have the same MsDuration.</para>
        /// <para>4. UniqueDef.MsPositionReFirstIUD attributes are all set correctly (starting at 0).</para>
        /// <para>5. A RestDef is never followed by another RestDef (RestDefs have been agglomerated).</para>
        /// <para>6. In Blocks, Trk and InputVoiceDef objects can additionally contain CautionaryChordDefs and ClefDefs (See Seq and InputVoiceDef).</para>
        /// <para>7. There may not be more than 4 InputVoiceDefs</para>
        /// <para>9. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef, or with a CautionaryChordDef.</para>
        /// </summary> 
        protected void AssertNonEmptyBarConsistency()
        {
            #region 1. The first VoiceDef in a Bar must be a Trk.
            Debug.Assert(_voiceDefs[0] is Trk, "The first VoiceDef in a Bar must be a Trk.");
			#endregion

			#region 2. All Trks precede the InputVoiceDefs (if any) in the _voiceDefs list.
			int nTrks = Trks.Count;
			for(int i = nTrks; i < _voiceDefs.Count; i++)
			{
				Debug.Assert(_voiceDefs[i] is InputVoiceDef, "All Trks must precede InputVoiceDefs (if any) in the _voiceDefs list."); 
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

            #region 6. In Blocks, Trk and InputVoiceDef objects can additionally contain CautionaryChordDefs and ClefDefs (See Seq and InputVoiceDef).
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

            #region 8. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef, or with a CautionaryChordDef.
            bool hasCorrectBeginning = false;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef is Trk)
                {
                    if((voiceDef.UniqueDefs.Count > 0 && ((voiceDef.UniqueDefs[0] is MidiChordDef) || (voiceDef.UniqueDefs[0] is CautionaryChordDef)))
                    || (voiceDef.UniqueDefs.Count > 1 && voiceDef.UniqueDefs[0] is ClefDef && voiceDef.UniqueDefs[1] is MidiChordDef))
                    {
                        hasCorrectBeginning = true;
                        break;
                    }
                }
            }
            Debug.Assert(hasCorrectBeginning, "At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef");

            #endregion
        }

        #region envelopes
        /// <summary>
        /// This function does not change the MsDuration of the Bar.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(Envelope envelope, double distortion)
        {
            AssertNonEmptyBarConsistency();
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

            AssertNonEmptyBarConsistency();
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
                if(voiceDef is Trk trk)
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
                AssertNonEmptyBarConsistency(); // all Trks and InputVoiceDefs have MsPositionReSeq == 0, and are the same length.
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

        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set
            {
                Debug.Assert(value >= 0);
                _absMsPosition = value;
            }
        }
		private int _absMsPosition = 0;

		public IReadOnlyList<Trk> Trks
        {
            get
            {
                List<Trk> trks = new List<Trk>();
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    if(voiceDef is Trk trk)
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
                    if(voiceDef is InputVoiceDef inputVoiceDef)
                    {
                        inputVoiceDefs.Add(inputVoiceDef);
                    }
                }
                return inputVoiceDefs;
            }
        }

		public IReadOnlyList<VoiceDef> VoiceDefs { get => _voiceDefs.AsReadOnly(); }
        private List<VoiceDef> _voiceDefs = new List<VoiceDef>();
    }
}
