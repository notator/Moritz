
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
	/// <summary>
	/// The base class for Trk and InputVoiceDef.
	/// <para>VoiceDef classes are IEnumerable (foreach loops can be used).</para>
	/// <para>They are also indexable (IUniqueDef iu = this[index])</para>
	/// </summary>
	public abstract class VoiceDef : IEnumerable
    {
        #region constructors

        protected VoiceDef(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
        {
            Debug.Assert(midiChannel >= 0 && msPositionReContainer >= 0 && iuds != null);

            this.MidiChannel = midiChannel;
            this._msPositionReContainer = msPositionReContainer;
            this._uniqueDefs = iuds;

            SetMsPositionsReFirstUD();

            AssertConsistency();
        }
		#endregion constructors

		internal void SetToRange(VoiceDef argVoiceDef, int startMsPosReSeq, int endMsPosReSeq)
		{
			_uniqueDefs.Clear();
			foreach(IUniqueDef iud in argVoiceDef)
			{
				int msPos = iud.MsPositionReFirstUD;
				if(msPos < startMsPosReSeq)
				{
					continue;
				}
				if(msPos >= endMsPosReSeq)
				{
					break;
				}
				_uniqueDefs.Add(iud);
			}
			//SetMsPositionsReFirstUD();
		}

		#region public indexer & enumerator
		/// <summary>
		/// Indexer. Allows individual UniqueDefs to be accessed using array notation on the VoiceDef.
		/// Automatically resets the MsPositions of all UniqueDefs in the list.
		/// e.g. iumdd = voiceDef[3].
		/// </summary>
		public IUniqueDef this[int i]
        {
            get
            {
                if(i < 0 || i >= _uniqueDefs.Count)
                {
					Debug.Assert(false, "Index out of range");
                }
                return _uniqueDefs[i];
            }
            set
            {
                if(i < 0 || i >= _uniqueDefs.Count)
                {
					Debug.Assert(false, "Index out of range");
				}
                
                Debug.Assert(!((this is Trk && value is InputChordDef) || (this is InputVoiceDef && value is MidiChordDef)));

                _uniqueDefs[i] = value;
                SetMsPositionsReFirstUD();
                AssertConsistency();
            }
        }

        #region Enumerators
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(_uniqueDefs);
        }
        // private enumerator class
        // see http://support.microsoft.com/kb/322022/en-us
        private class MyEnumerator : IEnumerator
        {
            public List<IUniqueDef> _uniqueDefs;
            int position = -1;
            //constructor
            public MyEnumerator(List<IUniqueDef> uniqueDefs)
            {
                _uniqueDefs = uniqueDefs;
            }
            private IEnumerator GetEnumerator()
            {
                return (IEnumerator)this;
            }
            //IEnumerator
            public bool MoveNext()
            {
                position++;
                return (position < _uniqueDefs.Count);
            }
            //IEnumerator
            public void Reset()
            { position = -1; }
            //IEnumerator
            public object Current
            {
                get
                {
                    try
                    {
                        return _uniqueDefs[position];
                    }
                    catch(IndexOutOfRangeException)
                    {
                        Debug.Assert(false);
                        return null;
                    }
                }
            }
        }  //end nested class
        #endregion
        #endregion public indexer & enumerator

        #region consistency
        protected bool CheckIndices(int beginIndex, int endIndex)
        {
			int udCount = _uniqueDefs.Count;
			if(udCount == 0)
			{
				return false;
			}
			else
			{
				Debug.Assert(beginIndex >= 0 && beginIndex < _uniqueDefs.Count, "Error: endIndex out of range.");
				Debug.Assert(endIndex > 0 && endIndex <= _uniqueDefs.Count, "Error: endIndex out of range.");
				Debug.Assert(beginIndex < endIndex, "Error: endIndex must be greater than beginIndex");
				return true;
			}
        }

		public virtual void AssertConsistency()
		{
			Debug.Assert(MsPositionReContainer >= 0);
			Debug.Assert(UniqueDefs != null);
			bool restFound = false;
			foreach(IUniqueDef iud in UniqueDefs)
			{
				if(iud is RestDef)
				{
					Debug.Assert(restFound == false, "Consecutive rests found!");
					restFound = true;
				}
				else
				{
					restFound = false;
				}
			}
			if(UniqueDefs.Count > 0)
			{
				int msPos = 0;
				foreach(IUniqueDef iud in UniqueDefs)
				{
					Debug.Assert(iud.MsPositionReFirstUD == msPos, "Error in iUniqueDef.MsPositionReFirstUD");
					msPos += iud.MsDuration;
				}
			}
		}
        #endregion consistency

        #region  miscellaneous
        /// <summary>
        /// Sets the MsPosition attribute of each IUniqueDef in the _uniqueDefs list.
        /// Uses all the MsDuration attributes, and _msPosition as origin.
        /// This function must be called at the end of any function that changes the _uniqueDefs list.
        /// </summary>
        public void SetMsPositionsReFirstUD()
        {
            if(_uniqueDefs.Count > 0)
            {
                int currentPositionReFirstIUD = 0;
                foreach(IUniqueDef umcd in _uniqueDefs)
                {
                    umcd.MsPositionReFirstUD = currentPositionReFirstIUD;
                    currentPositionReFirstIUD += umcd.MsDuration;
                }
            }
        }
        /// <summary>
        /// Rests dont have lyrics, so their index in the VoiceDef can't be shown as a lyric.
        /// Overridden by Clytemnestra, where the index is inserted before her lyrics.
        /// </summary>
        /// <param name="voiceDef"></param>
        public virtual void SetLyricsToIndex()
        {
            for(int index = 0; index < _uniqueDefs.Count; ++index)
            {
                if(_uniqueDefs[index] is IUniqueSplittableChordDef lmcd)
                {
                    lmcd.Lyric = index.ToString();
                }
            }
        }
        /// <summary>
        /// Inserts a ClefDef at the given index (which must be greater than 0).
        /// <para>If a ClefDef is defined directly before a rest, the resulting SmallClef will be placed before the
        /// following Chord or the bar's end barline.
        /// </para>
        /// <para>If the index is equal to or greater than the number of objects in the voiceDef, the ClefDef will be
        /// placed before the final barline.
        /// </para>
        /// <para>
        /// When changing clefs more than once in the same VoiceDef, it is easier to get the indices right if
        /// they are added backwards.
        /// </para>
        /// </summary>
        /// <param name="index">Must be greater than 0</param>
        /// <param name="clefType">One of the following strings: "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"</param>
        public void InsertClefDef(int index, string clefType)
        {
            #region check args
            Debug.Assert(index > 0, "Cannot insert a clef before the first chord or rest in the bar!");

            if(String.Equals(clefType, "t") == false
            && String.Equals(clefType, "t1") == false
            && String.Equals(clefType, "t2") == false
            && String.Equals(clefType, "t3") == false
            && String.Equals(clefType, "b") == false
            && String.Equals(clefType, "b1") == false
            && String.Equals(clefType, "b2") == false
            && String.Equals(clefType, "b3") == false)
            {
                Debug.Assert(false, "Unknown clef type.");
            }
            #endregion

            ClefDef clefDef = new ClefDef(clefType, 0);
            _Insert(index, clefDef);
        }
        #endregion miscellaneous

        #region attribute changers (Transpose etc.)

        /// <summary>
        /// An object is a NonMidiOrInputChordDef if it is not a MidiChordDef and it is not an InputChordDef.
        /// For example: a CautionaryChordDef, a RestDef or ClefDef.
        /// </summary>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        protected int GetNumberOfNonMidiOrInputChordDefs(int beginIndex, int endIndex)
        {
            int nNonMidiChordDefs = 0;
            for(int i = beginIndex; i < endIndex; ++i)
            {
                if(!(_uniqueDefs[i] is MidiChordDef) && !(_uniqueDefs[i] is InputChordDef))
                    nNonMidiChordDefs++;
            }
            return nNonMidiChordDefs;
        }

		#region Envelopes
		/// <summary>
		/// See Envelope.TimeWarp() for a description of the arguments.
		/// </summary>
		/// <param name="envelope"></param>
		/// <param name="distortion"></param>
		public void TimeWarp(Envelope envelope, double distortion)
		{
			#region requirements
			Debug.Assert(distortion >= 1);
			Debug.Assert(_uniqueDefs.Count > 0);
			#endregion

			int originalMsDuration = MsDuration;

			#region 1. create a List of ints containing the msPositions of the DurationDefs plus the end msPosition of the final DurationDef.
			List<DurationDef> durationDefs = new List<DurationDef>();
			List<int> originalPositions = new List<int>();
			int msPos = 0;
			foreach(IUniqueDef iud in UniqueDefs)
			{
				if(iud is DurationDef dd)
				{
					durationDefs.Add(dd);
					originalPositions.Add(msPos);
					msPos += dd.MsDuration;
				}
			}
			originalPositions.Add(msPos); // end position of duration to warp.
			#endregion
			List<int> newPositions = envelope.TimeWarp(originalPositions, distortion);

			for(int i = 0; i < durationDefs.Count; ++i)
			{
				DurationDef dd = durationDefs[i];
				dd.MsDuration = newPositions[i + 1] - newPositions[i];
			}

			SetMsPositionsReFirstUD();

			AssertConsistency();
		}
		#endregion Envelopes

		#region Transpose
		/// <summary>
		/// Transpose all the IUniqueChordDefs from beginIndex to (excluding) endIndex
		/// up by the number of semitones given in the interval argument.
		/// IUniqueChordDefs are MidiChordDef, InputChordDef and CautionaryChordDef.
		/// Negative interval values transpose down.
		/// It is not an error if Midi pitch values would exceed the range 0..127.
		/// In this case, they are silently coerced to 0 or 127 respectively.
		/// </summary>
		/// <param name="interval"></param>
		public void Transpose(int beginIndex, int endIndex, int interval)
        {
			if(CheckIndices(beginIndex, endIndex))
			{
				for(int i = beginIndex; i < endIndex; ++i)
				{
					if(_uniqueDefs[i] is IUniqueChordDef iucd)
					{
						iucd.Transpose(interval);
					}
				}
			}
        }
        /// <summary>
        /// Transpose the whole VoiceDef up by the number of semitones given in the argument.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            Transpose(0, _uniqueDefs.Count, interval);
        }
        /// <summary>
        /// Transposes all the MidiHeadSymbols in this VoiceDef by the number of semitones in the argument
        /// without changing the sound. Negative arguments transpose downwards.
        /// If the resulting midiHeadSymbol would be less than 0 or greater than 127,
        /// it is silently coerced to 0 or 127 respectively.
        /// </summary>
        /// <param name="p"></param>
        public void TransposeNotation(int semitonesToTranspose)
        {
            foreach(IUniqueDef iud in _uniqueDefs)
            {
                if(iud is IUniqueChordDef iucd)
                {
                    List<byte> midiPitches = iucd.NotatedMidiPitches;
                    for(int i = 0; i < midiPitches.Count; ++i)
                    {
                        midiPitches[i] = M.MidiValue(midiPitches[i] + semitonesToTranspose);
                    }
                }
            }
        }
        /// <summary>
        /// Each IUniqueChordDef is transposed by the interval current at its position.
        /// The argument contains a dictionary[msPosition, transposition].
        /// </summary>
        /// <param name="msPosTranspositionDict"></param>
        public void TransposeToDict(Dictionary<int, int> msPosTranspositionDict)
        {
            List<int> dictPositions = new List<int>(msPosTranspositionDict.Keys);

            int currentMsPosReFirstIUD = dictPositions[0];
            int j = 0;
            for(int i = 1; i < msPosTranspositionDict.Count; ++i)
            {
                int transposition = msPosTranspositionDict[currentMsPosReFirstIUD];
                int nextMsPosReFirstIUD = dictPositions[i];
                while(j < _uniqueDefs.Count && _uniqueDefs[j].MsPositionReFirstUD < nextMsPosReFirstIUD)
                {
                    if(_uniqueDefs[j].MsPositionReFirstUD >= currentMsPosReFirstIUD)
                    {
                        if(_uniqueDefs[j] is IUniqueChordDef iucd)
                        {
                            iucd.Transpose(transposition);
                        }
                    }
                    ++j;
                }
                currentMsPosReFirstIUD = nextMsPosReFirstIUD;
            }
        }

        /// <summary>
        /// Transposes the UniqueDefs from the beginIndex upto (excluding) endIndex
        /// by an equally increasing amount, so that the final MidiChordDef or InputChordDef is transposed by glissInterval.
        /// glissInterval can be negative.
        /// </summary>
        public void StepwiseGliss(int beginIndex, int endIndex, int glissInterval)
        {
			if(CheckIndices(beginIndex, endIndex))
			{ 
				int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

				int nSteps = (endIndex - 1 - beginIndex - nNonMidiChordDefs);
				if(nSteps > 0)
				{
					double interval = ((double)glissInterval) / nSteps;
					double step = interval;
					for(int i = beginIndex; i < endIndex; ++i)
					{
						MidiChordDef mcd = _uniqueDefs[i] as MidiChordDef;
						InputChordDef icd = _uniqueDefs[i] as InputChordDef;
						IUniqueChordDef iucd = (mcd == null) ? (IUniqueChordDef)icd : (IUniqueChordDef)mcd;
						if(iucd != null)
						{
							iucd.Transpose((int)Math.Round(interval));
							interval += step;
						}
					}
				}
			}
        }
        #endregion Transpose

        #endregion  attribute changers (Transpose etc.)

        #region Count changers
        #region list functions
        public abstract void Add(IUniqueDef iUniqueDef);
        protected void _Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(Container is Bar), "Cannot Add IUniqueDefs inside a Bar.");

            if(_uniqueDefs.Count > 0)
			{
				IUniqueDef lastIud = _uniqueDefs[_uniqueDefs.Count - 1];
				iUniqueDef.MsPositionReFirstUD = lastIud.MsPositionReFirstUD + lastIud.MsDuration;
			}
			else
			{
				iUniqueDef.MsPositionReFirstUD = 0;
			}
            _uniqueDefs.Add(iUniqueDef);

            AssertConsistency();
        }
        public abstract void AddRange(VoiceDef voiceDef);

		/// <summary>
		/// This function automatically agglommerates rests.
		/// </summary>
        protected void _AddRange(VoiceDef voiceDef)
        {
            Debug.Assert(!(Container is Bar), "Cannot AddRange of VoiceDefs inside a Bar.");

            _uniqueDefs.AddRange(voiceDef.UniqueDefs);

			AgglomerateRests();

            SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        public abstract void Insert(int index, IUniqueDef iUniqueDef);
        protected void _Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(Container is Bar && iUniqueDef.MsDuration > 0), "Cannot Insert IUniqueDefs that have msDuration inside a Bar.");

            _uniqueDefs.Insert(index, iUniqueDef);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }
        protected void _InsertRange(int index, VoiceDef voiceDef)
        {
            Debug.Assert(!(Container is Bar), "Cannot Insert range of IUniqueDefs inside a Bar.");

            _uniqueDefs.InsertRange(index, voiceDef.UniqueDefs);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        /// <summary>
        /// removes the iUniqueDef from the list, and then resets the positions of all the iUniqueDefs in the list.
        /// </summary>
        public void Remove(IUniqueDef iUniqueDef)
        {
            Debug.Assert(_uniqueDefs.Count > 0);
            Debug.Assert(_uniqueDefs.Contains(iUniqueDef));
            _uniqueDefs.Remove(iUniqueDef);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then resets the positions of all the lmdds in the list.
        /// </summary>
        public void RemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < _uniqueDefs.Count);
            _uniqueDefs.RemoveAt(index);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }
        /// <summary>
        /// Removes count iUniqueDefs from the list, startíng with the iUniqueDef at index.
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            Debug.Assert(index >= 0 && count >= 0 && ((index + count) <= _uniqueDefs.Count));
            _uniqueDefs.RemoveRange(index, count);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        /// <summary>
        /// Remove the IUniqueDefs which start between startMsPosReFirstIUD and (not including) endMsPosReFirstIUD 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void RemoveBetweenMsPositions(int startMsPosReFirstIUD, int endMsPosReFirstIUD)
        {
            IUniqueDef iumdd = _uniqueDefs.Find(f => (f.MsPositionReFirstUD >= startMsPosReFirstIUD) && (f.MsPositionReFirstUD < endMsPosReFirstIUD));
            while(iumdd != null)
            {
                _uniqueDefs.Remove(iumdd);
                iumdd = _uniqueDefs.Find(f => (f.MsPositionReFirstUD >= startMsPosReFirstIUD) && (f.MsPositionReFirstUD < endMsPosReFirstIUD));
            }
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }
        /// <summary>
        /// Removes the iUniqueMidiDurationDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        protected void _Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(!(Container is Bar), "Cannot Replace IUniqueDefs inside a Bar.");

            Debug.Assert(index >= 0 && index < _uniqueDefs.Count);
            _uniqueDefs.RemoveAt(index);
            _uniqueDefs.Insert(index, replacementIUnique);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }
        /// <summary>
        /// Replace all the IUniqueDefs from startMsPosition to (not including) endMsPosition by a single rest.
        /// </summary>
        public void Erase(int startMsPosition, int endMsPosition)
        {
            int beginIndex = FindIndexAtMsPositionReFirstIUD(startMsPosition);
            int endIndex = FindIndexAtMsPositionReFirstIUD(endMsPosition);

            for(int i = beginIndex; i < endIndex; ++i)
            {
                MidiChordDef mcd = this[i] as MidiChordDef;
                InputChordDef icd = this[i] as InputChordDef;
                IUniqueDef iud = (mcd == null) ? (IUniqueDef)icd : (IUniqueDef)mcd;
                if(iud != null)
                {
                    RestDef restDef;
                    if(this is InputVoiceDef)
                    {
                        restDef = new InputRestDef(iud.MsPositionReFirstUD, iud.MsDuration);
                    }
                    else
                    {
                        restDef = new MidiRestDef(iud.MsPositionReFirstUD, iud.MsDuration);
                    }
                    RemoveAt(i);
                    Insert(i, restDef);
                }
            }

            AgglomerateRests();

            AssertConsistency();
        }
        /// <summary>
        /// Extracts nUniqueDefs from the uniqueDefs, and then inserts them again at the toIndex.
        /// </summary>
        public void Translate(int fromIndex, int nUniqueDefs, int toIndex)
        {
            Debug.Assert((fromIndex + nUniqueDefs) <= _uniqueDefs.Count);
            Debug.Assert(toIndex <= (_uniqueDefs.Count - nUniqueDefs));
            List<IUniqueDef> extractedLmdds = _uniqueDefs.GetRange(fromIndex, nUniqueDefs);
            _uniqueDefs.RemoveRange(fromIndex, nUniqueDefs);
            _uniqueDefs.InsertRange(toIndex, extractedLmdds);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }
        /// <summary>
        /// Returns the index of the IUniqueDef which starts at or is otherwise current at msPosition.
        /// If msPosition is the EndMsPosition, the index of the final IUniqueDef + 1 (=Count) is returned.
        /// If the VoiceDef does not span msPosition, -1 (=error) is returned.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int FindIndexAtMsPositionReFirstIUD(int msPositionReFirstIUD)
        {
            int returnedIndex = -1;
            if(msPositionReFirstIUD == EndMsPositionReFirstIUD)
            {
                returnedIndex = this.Count;
            }
            else if(msPositionReFirstIUD >= _uniqueDefs[0].MsPositionReFirstUD && msPositionReFirstIUD < EndMsPositionReFirstIUD)
            {
                returnedIndex = _uniqueDefs.FindIndex(u => ((u.MsPositionReFirstUD <= msPositionReFirstIUD) && ((u.MsPositionReFirstUD + u.MsDuration) > msPositionReFirstIUD)));
            }
            Debug.Assert(returnedIndex != -1);
            return returnedIndex;
        }
        #endregion list functions

        #region VoiceDef duration changers

        /// <summary>
        /// Removes all the rests in this VoiceDef
        /// </summary>
        public void RemoveRests()
        {
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count, 0);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest from beginIndex to endIndex (exclusive) by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<DurationDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest in the UniqueDefs list by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<DurationDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest from beginIndex to endIndex (exclusive) by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest in the UniqueDefs list by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }

        /// <summary>
        /// Multiplies the MsDuration of each T from beginIndex to endIndex (exclusive) by factor.
        /// If a MsDuration becomes less than minThreshold, the T (chord or rest) is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        protected void AdjustMsDurations<T>(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            Debug.Assert(!(Container is Bar), "Cannot AdjustChordMsDurations inside a Bar.");

			if(CheckIndices(beginIndex, endIndex))
			{
				Debug.Assert(factor >= 0);
				for(int i = beginIndex; i < endIndex; ++i)
				{
					IUniqueDef iumdd = _uniqueDefs[i];
					if(iumdd is T)
					{
						iumdd.MsDuration = (int)((double)iumdd.MsDuration * factor);
					}
				}

				for(int i = _uniqueDefs.Count - 1; i >= 0; --i)
				{
					IUniqueDef iumdd = _uniqueDefs[i];
					if(iumdd.MsDuration < minThreshold)
					{
						_uniqueDefs.RemoveAt(i);
					}
				}

				SetMsPositionsReFirstUD();

				AssertConsistency();
			}
        }

        /// <summary>
        /// An object is a NonDurationDef if it is not a DurationDef.
        /// For example: a cautionaryChordDef or a clefDef.
        /// </summary>
        private int GetNumberOfNonDurationDefs(int beginIndex, int endIndex)
        {
            int nNonDurationDefs = 0;
            for(int i = beginIndex; i < endIndex; ++i)
            {
                if(! (_uniqueDefs[i] is DurationDef))
                    nNonDurationDefs++;
            }
            return nNonDurationDefs;
        }

        /// <summary>
        /// Creates an exponential accelerando or decelerando from beginIndex to (not including) endIndex.
        /// This function changes the msDuration in the given index range.
        /// endIndex can be equal to this.Count.
        /// </summary>
        public void CreateAccel(int beginIndex, int endIndex, double startEndRatio)
        {
            Debug.Assert(((beginIndex + 1) < endIndex) && (startEndRatio >= 0) && (endIndex <= Count));

            int nNonDurationDefs = GetNumberOfNonDurationDefs(beginIndex, endIndex);

            double basicIncrement = (startEndRatio - 1) / (endIndex - beginIndex - nNonDurationDefs);
            double factor = 1.0;

            for(int i = beginIndex; i < endIndex; ++i)
            {
                _uniqueDefs[i].AdjustMsDuration(factor);
                factor += basicIncrement;
            }

            SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        #endregion VoiceDef duration changers

        internal void RemoveDuplicateClefDefs()
        {
            if(_uniqueDefs.Count > 1)
            {
                for(int i = _uniqueDefs.Count - 1; i > 0; --i)
                {
                    IUniqueDef iud1 = _uniqueDefs[i];
                    if(iud1 is ClefDef)
                    {
                        for(int j = i - 1; j >= 0; --j)
                        {
                            IUniqueDef iud2 = _uniqueDefs[j];
                            if(iud2 is ClefDef)
                            {
                                if(string.Compare(((ClefDef)iud1).ClefType, ((ClefDef)iud2).ClefType) == 0) 
                                {
                                    _uniqueDefs.RemoveAt(i);
                                }
                                break;
                            }
                        }
                    }
                }
                AssertConsistency();
            }
        }

        /// <summary>
        /// Combines all consecutive rests.
        /// </summary>
        public void AgglomerateRests()
        {
            if(_uniqueDefs.Count > 1)
            {
                for(int i = _uniqueDefs.Count - 1; i > 0; --i)
                {
                    IUniqueDef lmdd2 = _uniqueDefs[i];
                    IUniqueDef lmdd1 = _uniqueDefs[i - 1];
                    if(lmdd2 is RestDef && lmdd1 is RestDef)
                    {
                        lmdd1.MsDuration += lmdd2.MsDuration;
                        _uniqueDefs.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the rest or chord at index, and extends the previous rest or chord
        /// by the removed duration, so that other msPositions don't change.
        /// </summary>
        /// <param name="p"></param>
        protected void AgglomerateRestOrChordAt(int index)
        {
            Debug.Assert(index > 0 && index < Count);
            _uniqueDefs[index - 1].MsDuration += _uniqueDefs[index].MsDuration;
            _uniqueDefs.RemoveAt(index);

            AssertConsistency();
        }

        #endregion Count changers

        #region Properties

        private int _midiChannel = int.MaxValue; // the MidiChannel will only be valid if set to a value in range [0..15]
        public int MidiChannel
        {
            get
            {
                return _midiChannel;
            }
            set
            {
                Debug.Assert(value >= 0 && value <= 15);
                _midiChannel = value;
            }
        }

        public ITrksContainer Container = null;

        public int Count { get { return _uniqueDefs.Count; } }

        private int _msPositionReContainer = 0;
        /// <summary>
        /// The msPosition of the first note or rest in the UniqueDefs list re the start of the containing Seq or Bar.
        /// The msPositions of the IUniqueDefs in the Trk are re the first IUniqueDef in the list, so the first IUniqueDef.MsPositionReFirstUID is always 0;
        /// </summary>
        public virtual int MsPositionReContainer
        {
            get
            {
                return _msPositionReContainer;
            }
            set
            {
				Debug.Assert(value >= 0);
                _msPositionReContainer = value;
            }
        }

        /// <summary>
        /// Setting this property stretches or compresses all the durations in the UniqueDefs list to fit the given total duration.
        /// This does not change the VoiceDef's MsPosition, but does affect its EndMsPosition.
        /// See also EndMsPosition.set.
        /// </summary>
        public int MsDuration
        {
            get
            {
                int total = 0;
                foreach(IUniqueDef iud in _uniqueDefs)
                {
					if(iud is IUniqueSplittableChordDef iuscd && iuscd.MsDurationToNextBarline != null)
					{
						total += (int)iuscd.MsDurationToNextBarline;
					}
					else
					{
						total += iud.MsDuration;
					}
                    
                }
                return total;
            }
            set
            {
                Debug.Assert(value > 0);

                int msDuration = value;

                List<int> relativeDurations = new List<int>();
                foreach(IUniqueDef iumdd in _uniqueDefs)
                {
                    if(iumdd.MsDuration > 0)
                        relativeDurations.Add(iumdd.MsDuration);
                }

                List<int> newDurations = M.IntDivisionSizes(msDuration, relativeDurations);

                Debug.Assert(newDurations.Count == relativeDurations.Count);
                int i = 0;
                int newTotal = 0;
                foreach(IUniqueDef iumdd in _uniqueDefs)
                {
                    if(iumdd.MsDuration > 0)
                    {
                        iumdd.MsDuration = newDurations[i];
                        newTotal += iumdd.MsDuration;
                        ++i;
                    }
                }

                Debug.Assert(msDuration == newTotal);

                SetMsPositionsReFirstUD();

                AssertConsistency();
            }
        }

        /// <summary>
        /// The position of the end of the last UniqueDef in the list re the first IUniqueDef in the list, or 0 if the list is empty.
        /// Setting this value can only be done if the UniqueDefs list is not empty, and the value
        /// is greater than the position of the final UniqueDef in the list. It then changes
        /// the msDuration of the final IUniqueDef.
        /// See also MsDuration.set.
        /// </summary>
        public int EndMsPositionReFirstIUD
        {
            get
            {
                int endMsPosReFirstUID = 0;
                if(_uniqueDefs.Count > 0)
                {
                    IUniqueDef lastIUD = _uniqueDefs[_uniqueDefs.Count - 1];
                    endMsPosReFirstUID += (lastIUD.MsPositionReFirstUD + lastIUD.MsDuration);
                }
                return endMsPosReFirstUID;
            }
            set
            {
                Debug.Assert(_uniqueDefs.Count > 0);
                Debug.Assert(value > EndMsPositionReFirstIUD);

                IUniqueDef lastLmdd = _uniqueDefs[_uniqueDefs.Count - 1];
                lastLmdd.MsDuration = value - EndMsPositionReFirstIUD;

                AssertConsistency();
            }
        }

        public List<IUniqueDef> UniqueDefs { get { return _uniqueDefs; } }
        protected List<IUniqueDef> _uniqueDefs = new List<IUniqueDef>();
 
        #endregion Properties
    }
}
