using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	/// <summary>
	/// A ModeGrpTrk is a Trk with a Mode.
	/// The Mode may not be null, and can be shared with other ModeGrpTrks.
	/// In other words, ModeGrpTrks do not own their Modes. 
	/// Beaming: BeamContinues is set to false on the final MidiChordDef.
	/// (Beams break automatically, even if MidiChordDef.BeamContinues is true,
	/// if the following symbol is a rest or has no beam.) 
	/// The following conditions are checked whenever the IUniqueDefs list is changed
	/// (If a check fails, an exception is thrown.):
	/// 1. Mode may not be null.
	/// 2. RootOctave must be greater than or equal to 0.
	/// 3. The first iUniqueDef must be a MidiChordDef.
	/// 4. The ModeGrpTrk can only contain MidiChordDef and MidiRestDef objects.
	/// 5. The MidiChordDefs can only contain pitches that are in the Mode.
	/// 6. The ModeGrpTrk may not contain consecutive MidiRestDefs
	/// 7. All MidiChordDef.BeamContinues properties are true, except the last, which is false.
	/// </summary>
	public class ModeGrpTrk : Trk
    {
		#region constructors
		/// <summary>
		/// ModeGrpTrk objects own unique IUniqueDefs, but can share Modes. The Mode may not be null.
		/// </summary>
		/// <param name="midiChannel"></param>
		/// <param name="msPositionReContainer"></param>
		/// <param name="iudList"></param>
		/// <param name="mode">can not be null</param>
		/// <param name="rootOctave">must be greater than or equal to 0</param>
		public ModeGrpTrk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iudList, Mode mode, int rootOctave)
            : base(midiChannel, msPositionReContainer, iudList)
        {
			_mode = mode; // _mode is checked by AssertModeGrpTrkConsistency() below.
			RootOctave = rootOctave; // RootOctave is checked by AssertModeGrpTrkConsistency() below.

			var velocityPerAbsolutePitch = mode.GetVelocityPerAbsolutePitch();

			base.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

			if(iudList.Count > 0)
			{
				SetBeamEnd();
			}

			AssertModeGrpTrkConsistency();
		}

		/// <summary>
		/// The IUniqueDefs are cloned, the other attributes (including the Mode) are not.
		/// </summary>
		public new ModeGrpTrk Clone
		{
			get
			{
				List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
				ModeGrpTrk ModeGrpTrk = new ModeGrpTrk(this.MidiChannel, this.MsPositionReContainer, clonedIUDs, Mode, RootOctave)
				{
					Container = this.Container
				};

				return ModeGrpTrk;
			}
		}
		#endregion constructors

		/// <summary>
		/// The following conditions are checked (If a check fails, an exception is thrown.):
		/// 1. Mode may not be null.
		/// 2. RootOctave must be greater than or equal to 0.
		/// 3. The first iUniqueDef must be a MidiChordDef.
		/// 4. The ModeGrpTrk can only contain MidiChordDef and MidiRestDef objects.
		/// 5. The MidiChordDefs can only contain pitches that are in the Mode.
		/// 6. The ModeGrpTrk may not contain consecutive MidiRestDefs
		/// 7. All MidiChordDef.BeamContinues properties are true, except the last, which is false.
		/// </summary>
		private void AssertModeGrpTrkConsistency()
		{
			base.AssertConsistency(); // also checks consecutive rests.

			Debug.Assert(Mode != null, "Mode must be set.");
			Debug.Assert(RootOctave >= 0, "Root Octave must be >= 0");
			if(_uniqueDefs.Count > 0)
			{
				Debug.Assert(_uniqueDefs[0] is MidiChordDef, "The first IUniqueDef must be a MidiChordDef.");
			}

			foreach(IUniqueDef iud in _uniqueDefs)
			{
				if(!(iud is MidiChordDef || iud is MidiRestDef))
				{
					Debug.Assert(false, "Illegal type.");
				}

				if(iud is MidiChordDef mcd)
				{
					Debug.Assert(_mode.ContainsAllPitches(mcd), "Illegal pitches.");
				}
			}

			MidiChordDef lastMidiChordDef = LastMidiChordDef;
			foreach(IUniqueDef iud in _uniqueDefs)
			{
				if(iud is MidiChordDef mcd)
				{
					if(mcd != lastMidiChordDef)
					{
						Debug.Assert(mcd.BeamContinues == true, "BeamContinues must only be false on the final MidiChordDef.");
					}
					else
					{
						Debug.Assert(mcd.BeamContinues == false, "BeamContinues must be false on the final MidiChordDef.");
					}
				}
			}
		}

		#region Overridden functions
		#region UniqueDefs list component changers
		/// <summary>
		/// Appends a new MidiChordDef or MidiRestDef to the end of the list.
		/// IUniqueDefs in ModeGrpTrks cannot be ClefDefs or CautionaryChordDefs.
		/// Automatically sets the iUniqueDef's msPosition.
		/// </summary>
		public override void Add(IUniqueDef iUniqueDef)
        {            
            base.Add(iUniqueDef);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}

        /// <summary>
        /// Adds the argument's UniqueDefs to the end of this Trk.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public override void AddRange(VoiceDef voiceDef)
        {
            base.AddRange(voiceDef);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}

		/// <summary>
		/// Sets the current IUniqueDefs list to the argument, then sets the MidiChordDef.BeamContinues properties
		/// and checks that the ModeGrpTrk is consistent. (The argument may not contain consecutive rests, etc.)
		/// </summary>
		internal void SetIUniqueDefs(List<IUniqueDef> iUniqueDefs)
		{
			_uniqueDefs = iUniqueDefs;
			SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}

		/// <summary>
		/// Inserts the iUniqueDef in the list at the given index, and then
		/// resets the positions of all the uniqueDefs in the list.
		/// </summary>
		public override void Insert(int index, IUniqueDef iUniqueDef)
        {
			base.Insert(index, iUniqueDef);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        /// <summary>
        /// Inserts the trk's UniqueDefs in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void InsertRange(int index, Trk trk)
        {
            base.InsertRange(index, trk);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public override void Replace(int index, IUniqueDef replacementIUnique)
        {
            base.Replace(index, replacementIUnique);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        /// <summary> 
        /// This function attempts to add all the non-MidiRestDef UniqueDefs in trk2 to the calling Trk
        /// at the positions given by their MsPositionReFirstIUD added to trk2.MsPositionReContainer,
        /// whereby trk2.MsPositionReContainer is used with respect to the calling Trk's container.
        /// Before doing the superimposition, the calling Trk is given leading and trailing RestDefs
        /// so that trk2's uniqueDefs can be added at their original positions without any problem.
        /// These leading and trailing RestDefs are however removed before the function returns.
        /// The superimposed uniqueDefs will be placed at their original positions if they fit inside
        /// a MidiRestDef in the original Trk. A Debug.Assert() fails if this is not the case.
        /// To insert single uniqueDefs between existing uniqueDefs, use the function
        ///     Insert(index, iudToInsert).
        /// trk2's UniqueDefs are not cloned.
        /// </summary>
        /// <returns>this</returns>
        public override Trk Superimpose(Trk trk2)
        {
            Trk trk = base.Superimpose(trk2);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
			return trk;
        }

        #endregion UniqueDefs list component changers
        #region UniqueDefs list order changers
        public override void Permute(int axisNumber, int contourNumber)
        {
            base.Permute(axisNumber, contourNumber);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        /// <summary>
        /// Re-orders up to 7 partitions in this Trk's UniqueDefs list. The content of each partition is not changed. The Trk's AxisIndex property is set.
        /// <para>1. Creates partitions (lists of UniqueDefs) using the partitionSizes in the third argument.</para>  
        /// <para>2. Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array) using the axisNumber and contourNumber arguments.</para>
        /// <para>3. Resets the UniqueDefs list to the concatenation of the re-ordered partitions.</para>
        /// </summary>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        /// <param name="partitionSizes">The number of UniqueDefs in each partition to be re-ordered.
        /// <para>This partitionSizes list must contain 1..7 partition sizes. The sizes must all be greater than 0. The sum of all the sizes must be equal
        /// to UniqueDefs.Count.</para>
        /// <para>An Exception is thrown if any of these conditions is not met.</para>
        /// <para>If the partitions list contains only one value, this function returns silently without doing anything.</para></param>
        public override void PermutePartitions(int axisNumber, int contourNumber, List<int> partitionSizes)
        {
            base.PermutePartitions(axisNumber, contourNumber, partitionSizes);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        public override void SortRootNotatedPitchAscending()
        {
            SortByRootNotatedPitch(true);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        public override void SortRootNotatedPitchDescending()
        {
            SortByRootNotatedPitch(false);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        public override void SortVelocityIncreasing()
        {
            SortByVelocity(true);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        public override void SortVelocityDecreasing()
        {
            SortByVelocity(false);
            SetBeamEnd();
			AssertModeGrpTrkConsistency();
		}
        #endregion UniqueDefs list order changers
        #endregion Overridden functions

        /// <summary>
        /// Shears the group vertically, using TransposeStepsInMode(steps).
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// The number of steps to transpose intermediate chords is calculated as a linear sequence from startSteps to endSteps.
        /// If there is only one chord in the ModeGrpTrk, it is transposed by startSteps.
        /// </summary>
        /// <param name="startSteps">The number of steps in the mode to transpose the first chord</param>
        /// <param name="endSteps">The number of steps in the mode to transpose the last chord</param>
        internal virtual void Shear(int startSteps, int endSteps)
        {
            Debug.Assert(Mode != null);

            List<int> stepsList = new List<int>();

            if(Count == 1)
            {
                stepsList.Add(startSteps);
            }
            else
            {
                double incr = ((double)(endSteps - startSteps)) / (Count - 1);
                double dSteps = startSteps;
                for(int i = 0; i < _uniqueDefs.Count; ++i)
                {
                    stepsList.Add((int)Math.Round(dSteps));
                    dSteps += incr;
                }
            }

            Shear(stepsList);
			AssertModeGrpTrkConsistency();
		}

        /// <summary>
        /// Shears the group vertically, using TransposeStepsInMode(steps).
        /// The number of ints in the argument must equal the number of UniqueDefs in the ModeGrpTrk.
        /// Each MidiChordDef in the UniqueDefs is transposed by the corresponding number of steps.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// </summary>
        /// <param name="stepsList">Each int is the number of steps to transpose the corresponding MidiChordDef.</param>
        internal virtual void Shear(List<int> stepsList)
        {
            Debug.Assert(Mode != null);
            Debug.Assert(stepsList != null && stepsList.Count == this.Count);

            if(Count == 1 && this[0] is MidiChordDef)
            {
                TransposeStepsInMode(stepsList[0]);
            }
            else
            {
                for(int i = 0; i < _uniqueDefs.Count; ++i)
                {
                    if(_uniqueDefs[i] is MidiChordDef mcd)
                    {
                        mcd.TransposeStepsInMode(_mode, stepsList[i]);
                    }
                }
            }
        }

        /// <summary>
        /// All the pitches in all the MidiChordDefs must be contained in this ModeGrpTrk's Mode.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// </summary>
        /// <param name="stepsToTranspose"></param>
        public virtual void TransposeStepsInMode(int stepsToTranspose)
        {
            Debug.Assert(_mode != null);
            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.TransposeStepsInMode(_mode, stepsToTranspose);
            }
			AssertModeGrpTrkConsistency();
		}

        /// <summary>
        /// The rootPitch and all the pitches in all the MidiChordDefs must be contained in the Trk's mode.
        /// Calculates the number of steps to transpose within the Trk's Mode, and then calls TransposeStepsInMode.
        /// The rootPitch will be the lowest pitch in any MidiChordDef.BasicMidiChordDefs[0] in the Trk.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// </summary>
        /// <param name="rootPitch"></param>
        public void TransposeToRootInMode(int rootPitch)
        {
            Debug.Assert(_mode != null && _mode.Gamut.Contains(rootPitch));

            int currentLowestPitch = int.MaxValue;

            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                currentLowestPitch = (mcd.BasicMidiChordDefs[0].Pitches[0] < currentLowestPitch) ? mcd.BasicMidiChordDefs[0].Pitches[0] : currentLowestPitch;
            }

            int stepsToTranspose = _mode.IndexOf(rootPitch) - _mode.IndexOf(currentLowestPitch);

            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.TransposeStepsInMode(_mode, stepsToTranspose);
            }
			AssertModeGrpTrkConsistency();
		}

        #region Mode
        /// <summary>
        /// When the Mode is set after the original ModeGrpTrk has been constructed, pitches are mapped to pitches in the same
        /// octave in the new Mode. The velocities will be those of the original pitches.
        /// Note that there may be less pitches per chord after setting the Mode in this way, since duplicate pitches
        /// will have been removed.
        /// </summary>
        public Mode Mode
        {
            get
            {
                Debug.Assert(_mode != null);
                return _mode;
            }
            set
            {
                Debug.Assert(value != null);
                Mode oldMode = _mode;
                _mode = value;
                for(int i = 0; i < this.UniqueDefs.Count; ++i)
                {
                    if(UniqueDefs[i] is MidiChordDef mcd)
                    {
                        List<BasicMidiChordDef> bmcds = mcd.BasicMidiChordDefs;
                        for(int j = 0; j < bmcds.Count; ++j)
                        {
                            BasicMidiChordDef bmcd = bmcds[j];
                            List<byte> oldPitches = bmcd.Pitches;
                            List<byte> oldVelocities = bmcd.Velocities;
                            List<byte> newPitches = new List<byte>();
                            List<byte> newVelocities = new List<byte>();
                            for(int k = 0; k < oldPitches.Count; ++k)
                            {
                                int pitchIndexInOldMode = oldMode.IndexOf(oldPitches[k]);
                                int pitchIndexInNewMode = GetPitchIndexInNewMode(oldMode, _mode, pitchIndexInOldMode);
                                byte newPitch = (byte)_mode.Gamut[pitchIndexInNewMode];
                                if(newPitches.Count == 0 || newPitch != newPitches[newPitches.Count - 1])
                                {
                                    byte oldVelocity = oldVelocities[k];
                                    newPitches.Add(newPitch);
                                    newVelocities.Add(oldVelocity);
                                }
                            }
                            bmcd.Pitches.Clear();
                            bmcd.Pitches.AddRange(newPitches);
                            bmcd.Velocities.Clear();
                            bmcd.Velocities.AddRange(newVelocities);
                        }
                        mcd.NotatedMidiPitches.Clear();
                        mcd.NotatedMidiPitches.AddRange(mcd.BasicMidiChordDefs[0].Pitches);
                        mcd.NotatedMidiVelocities = new List<byte>(mcd.BasicMidiChordDefs[0].Velocities);
                    }
                }
				AssertModeGrpTrkConsistency();
			}
        }

		public MidiChordDef LastMidiChordDef
		{
			get
			{
				MidiChordDef lastMidiChordDef= null;
				for(int i = _uniqueDefs.Count - 1; i >= 0; i--)
				{
					if( _uniqueDefs[i] is MidiChordDef mcd)
					{
						lastMidiChordDef = mcd;
						break;
					} 
				}
				return lastMidiChordDef;
			}
		}

		public MidiChordDef FirstMidiChordDef
		{ 
			get
			{
				MidiChordDef firstMidiChordDef = _uniqueDefs[0] as MidiChordDef;
				Debug.Assert(firstMidiChordDef != null);
				return firstMidiChordDef;

			}
		}

		protected Mode _mode;
		public readonly int RootOctave;

        /// <summary>
        /// Returns the index of a pitch in the same octave as the pitch at pitchIndexInOldMode.
        /// </summary>
        /// <param name="oldMode">may not be null</param>
        /// <param name="newMode">may not be null</param>
        /// <param name="pitchIndexInOldMode"></param>
        /// <returns></returns>
        private int GetPitchIndexInNewMode(Mode oldMode, Mode newMode, int pitchIndexInOldMode)
        {
            Debug.Assert(oldMode != null && newMode != null);

            int oldNPitchesPerOctave = oldMode.NPitchesPerOctave;
            int octave = pitchIndexInOldMode / oldNPitchesPerOctave;
            int oldPitchIndexInOctave = pitchIndexInOldMode - (octave * oldNPitchesPerOctave);
            int newNPitchesPerOctave = newMode.NPitchesPerOctave;

            // find the minimum angular distance between the oldPitchIndexInOctave and any newPitchIndexInOctave
            double oldRadians = oldPitchIndexInOctave * ((2 * Math.PI) / oldNPitchesPerOctave);
            int newPitchIndexInOctave = 0;
            double currentMinDelta = Double.MaxValue;
            for(int i = 0; i < newNPitchesPerOctave; ++i)
            {
                double newRadians = i * ((2 * Math.PI) / newNPitchesPerOctave);
                double delta1Abs = (newRadians > oldRadians) ? newRadians - oldRadians : oldRadians - newRadians;
                double oldRadiansPlus2pi = oldRadians + (2 * Math.PI);
                double delta2Abs = (newRadians > oldRadiansPlus2pi) ? newRadians - oldRadiansPlus2pi : oldRadiansPlus2pi - newRadians;
                double minDelta = (delta1Abs < delta2Abs) ? delta1Abs : delta2Abs;
                if(minDelta < currentMinDelta)
                {
                    currentMinDelta = minDelta;
                    newPitchIndexInOctave = i;
                }
            }

            int newPitchIndex = newPitchIndexInOctave + (octave * newNPitchesPerOctave);
			int newModeGamutCount = newMode.Gamut.Count;

			newPitchIndex = (newPitchIndex < newModeGamutCount) ? newPitchIndex : newModeGamutCount - 1;

            return newPitchIndex;
        }
        #endregion Mode

        /// <summary>
        /// Sets BeamContinues to true on all MidiChordDefs, except the last, which is set to true.
        /// </summary>
        private void SetBeamEnd()
        {
			MidiChordDef finalMCD = null;
			foreach(IUniqueDef iud in _uniqueDefs)
			{
				if(iud is MidiChordDef mcd)
				{
					mcd.BeamContinues = true;
					finalMCD = mcd;
				}
            }
			if(finalMCD != null)
			{
				finalMCD.BeamContinues = false;
			}
        }

        public override string ToString()
        {
            return ($"ModeGrpTrk: MsDuration={MsDuration} MsPositionReContainer={MsPositionReContainer} Count={Count}");
        }
    }
}
