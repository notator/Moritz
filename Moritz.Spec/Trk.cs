
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    /// <summary>
    /// A temporal sequence of IUniqueDef objects.
    /// <para>(IUniqueDef is implemented by all Unique...Defs that will eventually be converted to NoteObjects.)</para>
    /// <para></para>
    /// <para>This class is IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(IUniqueDef iumdd in trkDef) { ... }</para>
    /// <para>An Enumerator for MidiChordDefs is also defined so that</para>
    /// <para>foreach(MidiChordDef mcd in trkDef.MidiChordDefs) { ... }</para>
    /// <para>can also be used.</para>
    /// <para>This class is also indexable, as in:</para>
    /// <para>IUniqueDef iu = trk[index];</para>
    /// </summary>
    public class Trk : VoiceDef
    {
        #region constructors
		public Trk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
			: base(midiChannel, msPositionReContainer, iuds)
		{
            foreach(IUniqueDef iud in iuds)
            {
                Debug.Assert(iud is MidiChordDef || iud is RestDef || iud is CautionaryChordDef);
            }
		}

		/// <summary>
		/// Returns a deep clone of this TrkDef.
		/// </summary>
		public Trk Clone()
        {
            List<IUniqueDef> clonedIUDs = new List<IUniqueDef>();
            foreach(IUniqueDef iu in _uniqueDefs)
            {
                IUniqueDef clone = iu.Clone();
                clonedIUDs.Add(clone);
            }

            return new Trk(MidiChannel, MsPositionReContainer, clonedIUDs);
        }
        #endregion constructors

        #region Count changers
        /// <summary>
        /// Appends the new MidiChordDef or RestDef or CautionaryChordDef to the end of the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// </summary>
        public override void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(iUniqueDef is MidiChordDef || iUniqueDef is RestDef || iUniqueDef is CautionaryChordDef);
            _Add(iUniqueDef);
        }
        /// <summary>
        /// Adds the argument's UniqueDefs to the end of this Trk.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public override void AddRange(VoiceDef trk)
        {
            Debug.Assert(trk is Trk);
            _AddRange(trk);
        }
        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(iUniqueDef is MidiChordDef || iUniqueDef is RestDef || iUniqueDef is CautionaryChordDef);
            _Insert(index, iUniqueDef);
        }
        /// <summary>
        /// Inserts the trk's UniqueDefs in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public void InsertRange(int index, Trk trk)
        {
            _InsertRange(index, trk);
        }
        /// <summary>
        /// Creates a new TrkDef containing just the argument midiChordDef,
        /// then calls the other InsertInRest() function with the voiceDef as argument. 
        /// </summary>
        public void InsertInRest(MidiChordDef midiChordDef)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>() { midiChordDef };
            Trk trkDefToInsert = new Trk(this.MidiChannel, 0, iuds);
            InsertInRest(trkDefToInsert);
        }
        /// <summary>
        /// An attempt is made to insert the argument iVoiceDef in a rest in the VoiceDef.
        /// The rest is found using the iVoiceDef's MsPositon and MsDuration.
        /// The first and last objects in the argument must be chords, not rests.
        /// The argument may contain just one chord.
        /// The inserted iVoiceDef may end up at the beginning, middle or end of the spanning rest (which will
        /// be split as necessary).
        /// If no rest is found spanning the iVoiceDef, the attempt fails and an exception is thrown.
        /// This function does not change the msPositions of any other chords or rests in the containing VoiceDef,
        /// It does, of course, change the indices of the inserted lmdds and the later chords and rests.
        /// </summary>
        public void InsertInRest(Trk trk)
        {
            Debug.Assert(trk[0] is MidiChordDef && trk[trk.Count - 1] is MidiChordDef);
            _InsertInRest(trk);
        }
		/// <summary>
		/// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
		/// </summary>
		public void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(replacementIUnique is MidiChordDef || replacementIUnique is RestDef);
            _Replace(index, replacementIUnique);
        }
        #endregion Count changers

        #region Trk duration changers
        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef from beginIndex to (not including) endIndex by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef in the UniqueDefs list by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this TrkDef changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        #endregion TrkDef duration changers

        #region MidiChordDef attribute changers
        /// <summary>
        /// Multiplies each expression value in the MidiChordDefs
        /// from beginIndex to (not including) endIndex by the argument factor.
        /// </summary>
        public void AdjustExpression(int beginIndex, int endIndex, double factor)
        {
            CheckIndices(beginIndex, endIndex);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustExpression(factor);
                }
            }
        }
        /// <summary>
        /// Multiplies each expression value in the UniqueDefs by the argument factor.
        /// </summary>
        public void AdjustExpression(double factor)
        {
            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.AdjustExpression(factor);
            }
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs
        /// from beginIndex to (not including) endIndex by the argument factor.
        /// </summary>
        public void AdjustVelocities(int beginIndex, int endIndex, double factor)
        {
            CheckIndices(beginIndex, endIndex);
            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustVelocities(factor);
                }
            }
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs by the argument factor.
        /// </summary>
        public void AdjustVelocities(double factor)
        {
            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.AdjustVelocities(factor);
            }
        }

		/// Creates a hairpin in the velocities from startMsPosition to endMsPosition (non-inclusive).
		/// This function does NOT change velocities outside the range given in its arguments.
		/// There must be at least two IUniqueDefs in the msPosition range given in the arguments.
		/// The factors by which the velocities are multiplied change arithmetically:
		/// The velocity of the first IUniqueDefs is multiplied by startFactor, and the velocity
		/// of the last MidiChordDef in range by endFactor.
		/// Can be used to create a diminueno or crescendo.
		public void AdjustVelocitiesHairpin(int startMsPosition, int endMsPosition, double startFactor, double endFactor)
        {
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            Debug.Assert(((beginIndex + 1) < endIndex) && (startFactor >= 0) && (endFactor >= 0) && (endIndex <= Count));

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double factorIncrement = (endFactor - startFactor) / (endIndex - beginIndex - nNonMidiChordDefs);
            double factor = startFactor;
            List<IUniqueDef> lmdds = _uniqueDefs;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.AdjustVelocities(factor);
                    factor += factorIncrement;
                }
            }
        }
        /// <summary>
        /// Creates a moving pan from startPanValue at startMsPosition to endPanValue at endMsPosition.
        /// Implemented using one pan value per MidiChordDef.
        /// This function does NOT change pan values outside the position range given in its arguments.
        /// </summary>
        public void SetPanGliss(int startMsPosition, int endMsPosition, int startPanValue, int endPanValue)
        {
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            Debug.Assert(((beginIndex + 1) < endIndex) && (startPanValue >= 0) && (startPanValue <= 127)
                && (endPanValue >= 0) && (endPanValue <=127) && (endIndex <= Count));

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double increment = ((double)(endPanValue - startPanValue)) / (endIndex - beginIndex - nNonMidiChordDefs);
            int panValue = startPanValue;
            List<IUniqueDef> lmdds = _uniqueDefs;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = _uniqueDefs[i] as MidiChordDef;
                if(iumdd != null)
                {
                    iumdd.PanMsbs = new List<byte>() { (byte)panValue };
                    panValue += (int)increment;
                }
            }
        }
        /// <summary>
        /// Sets the pitchwheelDeviation for MidichordDefs in the range beginIndex to (not including) endindex.
        /// Rests in the range dont change.
        /// </summary>
        public void SetPitchWheelDeviation(int beginIndex, int endIndex, int deviation)
        {
            CheckIndices(beginIndex, endIndex);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef mcd = this[i] as MidiChordDef;
                if(mcd != null)
                {
                    mcd.PitchWheelDeviation = M.MidiValue(deviation);
                }
            }
        }
        /// <summary>
        /// Removes the pitchwheel commands (not the pitchwheelDeviations)
        /// from chords in the range beginIndex to (not including) endIndex.
        /// Rests in the range are not changed.
        /// </summary>
        public void RemoveScorePitchWheelCommands(int beginIndex, int endIndex)
        {
            CheckIndices(beginIndex, endIndex);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef iumdd = this[i] as MidiChordDef;
                if(iumdd != null)
                {
                    MidiChordDef umcd = iumdd as MidiChordDef;
                    if(umcd != null)
                    {
                        umcd.MidiChordSliderDefs.PitchWheelMsbs = new List<byte>();
                    }
                }
            }
        }
        /// <summary>
        /// Creates an exponential change (per index) of pitchwheelDeviation from startMsPosition to endMsPosition,
        /// </summary>
        /// <param name="finale"></param>
        protected void AdjustPitchWheelDeviations(int startMsPosition, int endMsPosition, int startPwd, int endPwd)
        {
            double furies1StartPwdValue = startPwd, furies1EndPwdValue = endPwd;
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double pwdfactor = Math.Pow(furies1EndPwdValue / furies1StartPwdValue, (double)1 / (endIndex - beginIndex - nNonMidiChordDefs)); // f13.Count'th root of furies1EndPwdValue/furies1StartPwdValue -- the last pwd should be furies1EndPwdValue

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef umc = _uniqueDefs[i] as MidiChordDef;
                if(umc != null)
                {
                    umc.PitchWheelDeviation = M.MidiValue((int)(furies1StartPwdValue * (Math.Pow(pwdfactor, i))));
                }
            }
        }
		#region alignment
		/// <summary>
		/// _uniqueDefs[indexToAlign] is moved to toMsPosition, and the surrounding symbols are spread accordingly
		/// between those at anchor1Index and anchor2Index. The symbols at anchor1Index and anchor2Index do not move.
		/// Note that indexToAlign cannot be 0, and that anchor2Index CAN be equal to _uniqueDefs.Count (i.e.on the final barline).
		/// This function checks that 
		///     1. anchor1Index is in range 0..(indexToAlign - 1),
		///     2. anchor2Index is in range (indexToAlign + 1).._localizedMidiDurationDefs.Count
		///     3. toPosition is greater than the msPosition at anchor1Index and less than the msPosition at anchor2Index.
		/// and throws an appropriate exception if there is a problem.
		/// </summary>
		public void AlignObjectAtIndex(int anchor1Index, int indexToAlign, int anchor2Index, int toMsPositionReFirstIUD)
		{
			// throws an exception if there's a problem.
			CheckAlignDefArgs(anchor1Index, indexToAlign, anchor2Index, toMsPositionReFirstIUD);

			List<IUniqueDef> lmdds = _uniqueDefs;
			int anchor1MsPositionReFirstIUD = lmdds[anchor1Index].MsPositionReFirstUD;
			int fromMsPositionReFirstIUD = lmdds[indexToAlign].MsPositionReFirstUD;
			int anchor2MsPositionReFirstIUD;
			if(anchor2Index == lmdds.Count) // i.e. anchor2 is on the final barline
			{
				anchor2MsPositionReFirstIUD = lmdds[anchor2Index - 1].MsPositionReFirstUD + lmdds[anchor2Index - 1].MsDuration;
			}
			else
			{
				anchor2MsPositionReFirstIUD = lmdds[anchor2Index].MsPositionReFirstUD;
			}

			float leftFactor = (float)(((float)(toMsPositionReFirstIUD - anchor1MsPositionReFirstIUD)) / ((float)(fromMsPositionReFirstIUD - anchor1MsPositionReFirstIUD)));
			for(int i = anchor1Index + 1; i < indexToAlign; ++i)
			{
				lmdds[i].MsPositionReFirstUD = anchor1MsPositionReFirstIUD + ((int)((lmdds[i].MsPositionReFirstUD - anchor1MsPositionReFirstIUD) * leftFactor));
			}

			lmdds[indexToAlign].MsPositionReFirstUD = toMsPositionReFirstIUD;

			float rightFactor = (float)(((float)(anchor2MsPositionReFirstIUD - toMsPositionReFirstIUD)) / ((float)(anchor2MsPositionReFirstIUD - fromMsPositionReFirstIUD)));
			for(int i = anchor2Index - 1; i > indexToAlign; --i)
			{
				lmdds[i].MsPositionReFirstUD = anchor2MsPositionReFirstIUD - ((int)((anchor2MsPositionReFirstIUD - lmdds[i].MsPositionReFirstUD) * rightFactor));
			}

			#region fix MsDurations
			for(int i = anchor1Index + 1; i <= anchor2Index; ++i)
			{
				if(i == lmdds.Count) // possible, when anchor2Index is the final barline
				{
					lmdds[i - 1].MsDuration = anchor2MsPositionReFirstIUD - lmdds[i - 1].MsPositionReFirstUD;
				}
				else
				{
					lmdds[i - 1].MsDuration = lmdds[i].MsPositionReFirstUD - lmdds[i - 1].MsPositionReFirstUD;
				}
			}
            #endregion

            AssertConsistency();
        }
		/// <summary>
		/// Debug.Assert fails if
		///     1. the index arguments are not in ascending order or if any are equal.
		///     2. any of the index arguments are out of range (anchor2Index CAN be _localizedMidiDurationDefs.Count, i.e. the final barline)
		///     3. toPosition is not greater than the msPosition at anchor1Index and less than the msPosition at anchor2Index.
		/// </summary>
		private void CheckAlignDefArgs(int anchor1Index, int indexToAlign, int anchor2Index, int toMsPositionReFirstUD)
		{
			List<IUniqueDef> lmdds = _uniqueDefs;
			int count = lmdds.Count;
			string msg = "\nError in VoiceDef.cs,\nfunction AlignDefMsPosition()\n\n";
            Debug.Assert((anchor1Index < indexToAlign && anchor2Index > indexToAlign),
                    msg + "Index out of order.\n" +
                    "\nanchor1Index=" + anchor1Index.ToString() +
                    "\nindexToAlign=" + indexToAlign.ToString() +
                    "\nanchor2Index=" + anchor2Index.ToString());

            Debug.Assert(!(anchor1Index > (count - 2) || indexToAlign > (count - 1) || anchor2Index > count)// anchor2Index can be at the final barline (=count)!
				|| (anchor1Index < 0 || indexToAlign < 1 || anchor2Index < 2),
                    msg + "Index out of range.\n" +
					"\ncount=" + count.ToString() +
					"\nanchor1Index=" + anchor1Index.ToString() +
					"\nindexToAlign=" + indexToAlign.ToString() +
					"\nanchor2Index=" + anchor2Index.ToString());

			int a1MsPosReFirstUD = lmdds[anchor1Index].MsPositionReFirstUD;
			int a2MsPosReFirstIUD;
			if(anchor2Index == lmdds.Count)
			{
				a2MsPosReFirstIUD = lmdds[anchor2Index - 1].MsPositionReFirstUD + lmdds[anchor2Index - 1].MsDuration;
			}
			else
			{
				a2MsPosReFirstIUD = lmdds[anchor2Index].MsPositionReFirstUD;
			}
			Debug.Assert((toMsPositionReFirstUD > a1MsPosReFirstUD && toMsPositionReFirstUD < a2MsPosReFirstIUD),
			        msg + "Target (msPos) position out of range.\n" +
					"\nanchor1Index=" + anchor1Index.ToString() +
					"\nindexToAlign=" + indexToAlign.ToString() +
					"\nanchor2Index=" + anchor2Index.ToString() +
					"\ntoMsPosition=" + toMsPositionReFirstUD.ToString());
		}
		#endregion alignment

		#endregion MidiChordDef attribute changers)

		#region Enumerators
		private IEnumerable<MidiChordDef> MidiChordDefs
		{
			get
			{
				foreach(IUniqueDef iud in _uniqueDefs)
				{
					MidiChordDef midiChordDef = iud as MidiChordDef;
					if(midiChordDef != null)
						yield return midiChordDef;
				}
			}
		}
		#endregion

		#region public Permute()
		/// <summary>
		/// Re-orders the UniqueDefs in (part of) this VoiceDef.
		/// <para>1. creates partitions (lists of UniqueDefs) using the startAtIndex and partitionSizes in the first two</para>
		/// <para>-  arguments (see parameter info below).</para>   
		/// <para>2. Sorts the partitions into ascending order of their lowest pitches.
		/// <para>3. Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array) using</para>
		/// <para>-  the contourNumber and axisNumber arguments.</para>
		/// <para>4. Concatenates the re-ordered partitions, re-sets their MsPositions, and replaces the UniqueDefs in</para>
		/// <para>-  the original List with the result.</para>
		/// <para>5. If setLyricsToIndex is true, sets the lyrics to the new indices</para>
		/// <para>SpecialCases:</para>
		/// <para>If a partition contains a single LocalMidiRestDef (the partition is a *rest*, having no 'lowest pitch'), the other</para>
		/// <para>partitions are re-ordered as if the rest-partition was not there, and the rest-partition is subsequently re-inserted</para>
		/// <para>at its original partition position.</para>
		/// <para>If two sub-VoiceDefs have the same initial base pitch, they stay in the same order as they were (not necessarily</para>
		/// <para>together, of course.)</para>
		/// </summary>
		/// <param name="startAtIndex">The index in UniqueDefs at which to start the re-ordering.
		/// </param>
		/// <param name="partitionSizes">The number of UniqueDefs in each sub-voiceDef to be re-ordered.
		/// <para>This partitionSizes list must contain:</para>
		/// <para>    1..7 int sizes.</para>
		/// <para>    sizes which are all greater than 0.</para>
		/// <para>    The sum of all the sizes + startAtIndex must be less than or equal to UniqueDefs.Count.</para>
		/// <para>An Exception is thrown if any of these conditions is not met.</para>
		/// <para>If the partitions list contains only one value, this function returns silently without doing anything.</para>
		/// </param>
		/// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
		/// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
		public void Permute(int startAtIndex, List<int> partitionSizes, int axisNumber, int contourNumber)
		{
			CheckSetContourArgs(startAtIndex, partitionSizes, axisNumber, contourNumber);

			List<List<IUniqueDef>> partitions = GetPartitions(startAtIndex, partitionSizes);

			// Remove any partitions (from the partitions list) that contain only a single LocalMidiRestDef
			// Store them (the rests), with their original partition indices, in the returned list of KeyValuePairs.
			List<KeyValuePair<int, List<IUniqueDef>>> restPartitions = GetRestPartitions(partitions);

			if(partitions.Count > 1)
			{
				// All the partitions now contain at least one LocalMidiChordDef
				// Sort into ascending order (part 2 of the above comment)
				partitions = SortPartitions(partitions);
				// re-order the partitions (part 3 of the above comment)
				partitions = DoContouring(partitions, axisNumber, contourNumber);
			}

			RestoreRestPartitions(partitions, restPartitions);

			List<IUniqueDef> sortedLmdds = ConvertPartitionsToFlatIUDs(startAtIndex, partitions);

			for(int i = 0; i < sortedLmdds.Count; ++i)
			{
				_uniqueDefs[startAtIndex + i] = sortedLmdds[i];
			}

            AssertConsistency();
		}

		private List<IUniqueDef> ConvertPartitionsToFlatIUDs(int startAtIndex, List<List<IUniqueDef>> partitions)
		{
			List<IUniqueDef> newIUDs = new List<IUniqueDef>();
			int msPositionReFirstIUD = _uniqueDefs[startAtIndex].MsPositionReFirstUD;
			foreach(List<IUniqueDef> partition in partitions)
			{
				foreach(IUniqueDef pLmdd in partition)
				{
					pLmdd.MsPositionReFirstUD = msPositionReFirstIUD;
					msPositionReFirstIUD += pLmdd.MsDuration;
					newIUDs.Add(pLmdd);
				}
			}
			return newIUDs;
		}

		/// <summary>
		/// Re-insert the restPartitions at their original positions
		/// </summary>
		private void RestoreRestPartitions(List<List<IUniqueDef>> partitions, List<KeyValuePair<int, List<IUniqueDef>>> restPartitions)
		{
			for(int i = restPartitions.Count - 1; i >= 0; --i)
			{
				KeyValuePair<int, List<IUniqueDef>> kvp = restPartitions[i];
				partitions.Insert(kvp.Key, kvp.Value);
			}

            AssertConsistency();
		}

		private List<List<IUniqueDef>> GetPartitions(int startAtIndex, List<int> partitionSizes)
		{
			List<List<IUniqueDef>> partitions = new List<List<IUniqueDef>>();
			int lmddIndex = startAtIndex;
			foreach(int size in partitionSizes)
			{
				List<IUniqueDef> partition = new List<IUniqueDef>();
				for(int i = 0; i < size; ++i)
				{
					partition.Add(_uniqueDefs[lmddIndex++]);
				}
				partitions.Add(partition);
			}
			return partitions;
		}

		/// <summary>
		/// Remove any partitions (from partitions) that contain only a single RestDef.
		/// Store them, with their original partition indices, in the returned list of KeyValuePairs.
		/// </summary>
		private List<KeyValuePair<int, List<IUniqueDef>>> GetRestPartitions(List<List<IUniqueDef>> partitions)
		{
			List<List<IUniqueDef>> newPartitions = new List<List<IUniqueDef>>();
			List<KeyValuePair<int, List<IUniqueDef>>> restPartitions = new List<KeyValuePair<int, List<IUniqueDef>>>();
			for(int i = 0; i < partitions.Count; ++i)
			{
				List<IUniqueDef> partition = partitions[i];
				if(partition.Count == 1 && partition[0] is RestDef)
				{
					restPartitions.Add(new KeyValuePair<int, List<IUniqueDef>>(i, partition));
				}
				else
				{
					newPartitions.Add(partition);
				}
			}

			partitions = newPartitions;
			return restPartitions;
		}

		/// <summary>
		/// Returns the result of sorting the partitions into ascending order of their lowest pitches.
		/// <para>All the partitions contain at least one MidiChordDef</para>
		/// <para>If two partitions have the same lowest pitch, they stay in the same order as they were</para>
		/// <para>(not necessarily together, of course.)</para>    
		/// </summary>
		private List<List<IUniqueDef>> SortPartitions(List<List<IUniqueDef>> partitions)
		{
			List<byte> lowestPitches = GetLowestPitches(partitions);
			List<byte> sortedLowestPitches = new List<byte>(lowestPitches);
			sortedLowestPitches.Sort();

			List<List<IUniqueDef>> sortedPartitions = new List<List<IUniqueDef>>();
			foreach(byte lowestPitch in sortedLowestPitches)
			{
				int pitchIndex = lowestPitches.FindIndex(item => item == lowestPitch);
				sortedPartitions.Add(partitions[pitchIndex]);
				lowestPitches[pitchIndex] = byte.MaxValue;
			}

			return sortedPartitions;
		}

		/// <summary>
		/// Returns a list containing the lowest pitch in each partition
		/// </summary>
		private List<byte> GetLowestPitches(List<List<IUniqueDef>> partitions)
		{
			List<byte> lowestPitches = new List<byte>();
			foreach(List<IUniqueDef> partition in partitions)
			{
				byte lowestPitch = byte.MaxValue;
				foreach(MidiChordDef iumdd in partition)
				{
					foreach(BasicMidiChordDef bmcd in iumdd.BasicMidiChordDefs)
					{
						foreach(byte note in bmcd.Pitches)
						{
							lowestPitch = (lowestPitch < note) ? lowestPitch : note;
						}
					}
				}
				Debug.Assert(lowestPitch != byte.MaxValue, "There must be at least one MidiChordDef in the partition.");
				lowestPitches.Add(lowestPitch);
			}
			return lowestPitches;
		}

		/// <summary>
		/// Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array)
		/// <para>using partitions.Count and the contourNumber and axisNumber arguments.</para>
		/// <para>Does not change the inner contents of the partitions themselves.</para>
		/// </summary>
		/// <returns>A re-ordered list of partitions</returns>
		private List<List<IUniqueDef>> DoContouring(List<List<IUniqueDef>> partitions, int axisNumber, int contourNumber)
		{
			List<List<IUniqueDef>> contouredPartitions = new List<List<IUniqueDef>>();
			int[] contour = K.Contour(partitions.Count, contourNumber, axisNumber);
			foreach(int number in contour)
			{
				// K.Contour() always returns an array containing 7 values.
				// For densities less than 7, the final values are 0.
				if(number == 0)
					break;
				contouredPartitions.Add(partitions[number - 1]);
			}

			return contouredPartitions;
		}

		/// <summary>
		/// Debug.Assert fails if one of the following conditions is not met.
		/// <para>startAtIndex is a valid index in the UniqueDefs list</para>
		/// <para>partitionSizes.Count is greater than 0, and less than 8.</para>
		/// <para>all sizes in partitionSizes are greater then 0.</para>
		/// <para>the sum of startAtIndex plus all the partition sizes is not greater than UniqueDefs.Count</para>
		/// <para>contourNumber is in the range 1..12</para>
		/// <para>axisNumber is in the range 1..12</para>
		/// </summary>
		private void CheckSetContourArgs(int startAtIndex, List<int> partitionSizes, int axisNumber, int contourNumber)
		{
			List<IUniqueDef> lmdds = _uniqueDefs;
			Debug.Assert(!(startAtIndex < 0 || startAtIndex > lmdds.Count - 1), "startAtIndex is out of range.");
			Debug.Assert(!(partitionSizes.Count < 1 || partitionSizes.Count > 7), "partitionSizes.Count must be in range 1..7");

			int totalNumberOfLmdds = startAtIndex;
			foreach(int size in partitionSizes)
			{
				Debug.Assert(size >= 1, "partitions must contain at least one IUniqueDef");
				totalNumberOfLmdds += size;
				Debug.Assert(!(totalNumberOfLmdds > lmdds.Count), "partitions are too big or start too late");
			}
            Debug.Assert(!(contourNumber < 1 || contourNumber > 12), "contourNumber out of range 1..12");
            Debug.Assert(!(axisNumber < 1 || axisNumber > 12), "axisNumber out of range 1..12");
		}
		#endregion public Permute

		/// <summary>
		/// The number of MidiChordDefs and RestDefs in this TrkDef
		/// </summary>
		public int DurationsCount
		{
			get
			{
				int count = 0;
				foreach(IUniqueDef iud in _uniqueDefs)
				{
					if(iud is MidiChordDef || iud is RestDef)
					{
						count++;
					}
				}
				return count;
			}
		}

        /// <summary>
        /// The composition algorithm must set the MasterVolume (to a value != null)
        /// in every TrkDef in the first bar of the score.
        /// All other TrkDefs retain the default value 0. 
        /// </summary>
        public byte? MasterVolume = null; // default value
	}
}
