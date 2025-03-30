using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
	/// In Seqs, Trks can contain any combination of RestDef and MidiChordDef.
    /// In Blocks, Trks can additionally contain CautionaryChordDefs and ClefDefs.
    /// Each ChannelDef has a list of Trk objects, each of which has the same midi channel.
    /// (A Voice has a single, defined midi channel)
    /// <para>Trks are IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(IUniqueDef iumdd in trk) { ... }</para>
    /// <para>An Enumerator for MidiChordDefs is also defined so that</para>
    /// <para>foreach(MidiChordDef mcd in trkDef.MidiChordDefs) { ... }</para>
    /// <para>can also be used.</para>
    /// <para>This class is also indexable, as in:</para>
    /// <para>IUniqueDef iu = trk[index];</para>
    /// </summary>
    public class Trk : ICloneable
    {
        #region constructors
        public Trk(List<IUniqueDef> iuds)
        {
            _uniqueDefs = iuds;

            AssertConsistency();;
        }

        /// <summary>
        /// A Trk with msPositionReContainer=0 and an empty UniqueDefs list.
        /// </summary>
        public Trk()
        {
            _uniqueDefs = new List<IUniqueDef>();

            AssertConsistency();;
        }

        /// <summary>
        /// Returns a deep clone of this Trk.
        /// </summary>
        public object Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Trk trk = new Trk(clonedIUDs);

            return trk;
        }

        /// <summary>
        /// Also used by Clone() functions in subclasses
        /// </summary>
        protected List<IUniqueDef> GetUniqueDefsClone()
        {
            List<IUniqueDef> clonedIUDs = new List<IUniqueDef>();
            foreach(IUniqueDef iu in _uniqueDefs)
            {
                IUniqueDef clonedIUD = (IUniqueDef)iu.Clone();
                clonedIUDs.Add(clonedIUD);
            }
            return clonedIUDs;
        }


        public Tuple<Trk, Trk> PopTrk(int poppedMsDuration)
        {
            CanContainCautionaryChordDef = true;
            Debug.Assert(CanContainClefDefs == false);

            AssertConsistency();

            Trk poppedTrk = new Trk(new List<IUniqueDef>());
            poppedTrk.CanContainCautionaryChordDef = true;
            Debug.Assert(poppedTrk.CanContainClefDefs == false);

            Trk remainingTrk = new Trk(new List<IUniqueDef>());
            remainingTrk.CanContainCautionaryChordDef = true;
            Debug.Assert(remainingTrk.CanContainClefDefs == false);

            foreach(IUniqueDef iud in UniqueDefs)
            {
                int iudMsDuration = iud.MsDuration;
                int iudStartPos = iud.MsPositionReFirstUD;
                int iudEndPos = iudStartPos + iudMsDuration;

                if(iudStartPos >= poppedMsDuration)
                {
                    // moves the first iud, regardless of type, to the start of remainingTrk
                    // This also moves CautionaryChordDefs (which can only be in this position).
                    remainingTrk.UniqueDefs.Add(iud);
                }
                else if(iudEndPos > poppedMsDuration)
                {
                    int durationBeforeBarline = poppedMsDuration - iudStartPos;
                    int durationAfterBarline = iudEndPos - poppedMsDuration;
                    if(iud is RestDef)
                    {
                        // This is a rest. Split it.
                        RestDef firstRestHalf = new RestDef(iudStartPos, durationBeforeBarline);
                        poppedTrk.UniqueDefs.Add(firstRestHalf);

                        RestDef secondRestHalf = new RestDef(poppedMsDuration, durationAfterBarline);
                        remainingTrk.UniqueDefs.Add(secondRestHalf);
                    }
                    else if(iud is CautionaryChordDef)
                    {
                        Debug.Assert(false, "There shouldnt be any cautionary chords here.");
                        // This error can happen if an attempt is made to set barlines too close together,
                        // i.e. (I think) if an attempt is made to create a bar that contains nothing... 
                    }
                    else if(iud is MidiChordDef)
                    {
                        IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
                        uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
                        poppedTrk.UniqueDefs.Add(uniqueChordDef);

                        Debug.Assert(remainingTrk.UniqueDefs.Count == 0);
                        CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
                        remainingTrk.UniqueDefs.Add(ccd);
                    }
                }
                else
                {
                    Debug.Assert(iudEndPos <= poppedMsDuration && iudStartPos >= 0);
                    poppedTrk.UniqueDefs.Add(iud);
                }
            }

            return new Tuple<Trk, Trk>(poppedTrk, remainingTrk);
        }

        #endregion constructors

        /// <summary>
        /// If Finalized is false then the UniqueDefs can only contain any combination of
        ///     RestDef, MidiChordDef.
        /// If finalized is true then the UniqueDefs can contain any combination of
        ///     RestDef, MidiChordDef, CautionaryChordDef, ClefDef.
        /// Checks the content of the trk, taking the current state of CanContainCautionaryChordDef 
        /// and CanContainClefDefs into account.
        /// The MsPositionReFirstUD of each UniqueDef is always consistent with the uniqueDef.MsDurations.
        /// Consecutive MidiRestDefs are always illegal.
        /// </summary>
        public void AssertConsistency()
        {
            if(UniqueDefs.Count > 0)
            {
                int msPos = 0;
                foreach(IUniqueDef iud in UniqueDefs)
                {
                    Debug.Assert(iud.MsPositionReFirstUD == msPos, "Error in iUniqueDef.MsPositionReFirstUD");
                    msPos += iud.MsDuration;
                }
            }

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

            if(!CanContainCautionaryChordDef && !CanContainClefDefs)
            {
                foreach(IUniqueDef iud in UniqueDefs)
                {
                    Debug.Assert(iud is MidiChordDef || iud is RestDef);
                }
            }
            else if(CanContainCautionaryChordDef && !CanContainClefDefs)
            {
                IUniqueDef iud = UniqueDefs[0];
                Debug.Assert(iud is MidiChordDef || iud is RestDef || iud is CautionaryChordDef);
                for(int i = 1; i < UniqueDefs.Count; i++)
                {
                    iud = UniqueDefs[i];
                    Debug.Assert(iud is MidiChordDef || iud is RestDef);
                }
            }
            else
            {
                // ClefDefs must be added _after_ CautionaryChordDefs
                Debug.Assert(!(!CanContainCautionaryChordDef && CanContainClefDefs));
                foreach(IUniqueDef iud in UniqueDefs)
                {
                    Debug.Assert(iud is MidiChordDef || iud is RestDef || iud is CautionaryChordDef || iud is ClefDef);
                }
            } 
        }

        /// <summary>
        /// This value is used by the AssertConsistency() function.
        /// If CanContainCautionaryChordDef is false, Trks can only contain MidiChordDef and RestDef IUniqueDefs.
        /// If CanContainCautionaryChordDef is true, the first IUniqueDef can also be a CautionaryChordDef.
        /// </summary>
        private bool CanContainCautionaryChordDef = false;
        /// <summary>
        /// This value is used by the AssertConsistency() function.
        /// If CanContainClefDefs is false, Trks cannot contain ClefDefs.
        /// If CanContainClefDefs is true, Trks can contain ClefDefs.
        /// </summary>
        private bool CanContainClefDefs = false;

        public List<IUniqueDef> UniqueDefs { get { return _uniqueDefs; } }
        private List<IUniqueDef> _uniqueDefs = null;

        #region moved here from ChannelDef

        /// <summary>
        /// The patch only needs to be set in the first chord in each Trk,
        /// since it will be set by shunting if the Assistant Performer starts later.
        /// </summary>
        public void SetPatch0InTheFirstChord()
        {
            MidiChordDef firstMidiChordDef = null;
            foreach(IUniqueDef iUniqueDef in UniqueDefs)
            {
                firstMidiChordDef = iUniqueDef as MidiChordDef;
                if(firstMidiChordDef != null)
                {
                    firstMidiChordDef.MidiChordControlDefs.Preset = 0;
                    firstMidiChordDef.MidiChordControlDefs.AllControllersOff = true;
                    break;
                }
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

        public int Count { get { return _uniqueDefs.Count; } }

        #region  miscellaneous
        internal void SetToRange(ChannelDef argVoiceDef, int startMsPosReSeq, int endMsPosReSeq)
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
            SetMsPositionsReFirstUD();
        }
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
        /// <summary>
        /// Rests dont have lyrics, so their index in the ChannelDef can't be shown as a lyric.
        /// Overridden by Clytemnestra, where the index is inserted before her lyrics.
        /// </summary>
        /// <param name="channelDef"></param>
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

        #endregion miscellaneous

        #region attribute changers (Transpose etc.)

        /// <summary>
        /// An object is a NonMidiOrInputChordDef if it is not a MidiChordDef.
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
                if(!(_uniqueDefs[i] is MidiChordDef))
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
        /// Transpose the whole ChannelDef up by the number of semitones given in the argument.
        /// </summary>
        /// <param name="interval"></param>
        public void Transpose(int interval)
        {
            Transpose(0, _uniqueDefs.Count, interval);
        }
        /// <summary>
        /// Transposes all the MidiHeadSymbols in this ChannelDef by the number of semitones in the argument
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
                    List<int> midiPitches = iucd.Pitches;
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
                        IUniqueChordDef iucd = _uniqueDefs[i] as IUniqueChordDef;
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
        /// <summary>
        /// Appends the new MidiChordDef, RestDef, CautionaryChordDef or ClefDef to the end of the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// N.B. Can be used to Add CautionaryChordDef and ClefDef arguments.
        /// </summary>
        public void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(CanContainCautionaryChordDef == true);

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
        protected void _Add(IUniqueDef iUniqueDef)
        {
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

        /// <summary>
        /// This function __moves__ the channelDef's UniqueDefs to the end of this channelDef's UniqueDefs _without_cloning_them_ (c.f. Trk.ConcatCloneAt(...) ).
        /// Rests are automatically agglommerated.
        /// </summary>
        public void AddRange(Trk trk)
        {
            _uniqueDefs.AddRange(trk.UniqueDefs);

            AgglomerateRests();

            SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        public void Insert(int uidIndex, IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(iUniqueDef.MsDuration > 0), "Cannot Insert IUniqueDefs that have msDuration inside a Bar.");

            UniqueDefs.Insert(uidIndex, iUniqueDef);

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

                IUniqueDef iud = this[i] as IUniqueDef;
                if(iud != null)
                {
                    RestDef restDef = new RestDef(iud.MsPositionReFirstUD, iud.MsDuration);
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
        /// If the ChannelDef does not span msPosition, -1 (=error) is returned.
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

        #region ChannelDef duration changers

        /// <summary>
        /// Removes all the rests in this ChannelDef
        /// </summary>
        public void RemoveRests()
        {
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count, 0);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest from beginIndex to endIndex (exclusive) by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this ChannelDef changes accordingly.
        /// </summary>
        public void AdjustMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<DurationDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each chord and rest in the UniqueDefs list by factor.
        /// If a chord or rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this ChannelDef changes accordingly.
        /// </summary>
        public void AdjustMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<DurationDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest from beginIndex to endIndex (exclusive) by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this ChannelDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each rest in the UniqueDefs list by factor.
        /// If a rest's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this ChannelDef changes accordingly.
        /// </summary>
        public void AdjustRestMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<RestDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }

        /// <summary>
        /// Multiplies the MsDuration of each T from beginIndex to endIndex (exclusive) by factor.
        /// If a MsDuration becomes less than minThreshold, the T (chord or rest) is removed.
        /// The total duration of this ChannelDef changes accordingly.
        /// </summary>
        protected void AdjustMsDurations<T>(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
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
                if(!(_uniqueDefs[i] is DurationDef))
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
                _uniqueDefs[i].MsDuration = (int)Math.Round(_uniqueDefs[i].MsDuration * factor);
                factor += basicIncrement;
            }

            SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        #endregion ChannelDef duration changers

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
        #endregion moved here from ChannelDef

        #region Add, Remove, Insert, Replace objects in the Trk

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
        /// Appends **clones** of the trk2.UniqueDefs after the end of this trk's UniqueDefs at msPositionReThisTrk, inserting a rest as necessary.
        /// To simply concatenate trk2 with this trk, pass this trk's MsDuration as the second argument.
        /// An exception will be thrown if msPositionReThisTrk is less than the current duration of this trk.
        /// Any rests that result from this operation are silently agglommerated.
        /// The MsPositionReFirstUD values in the concatenated IUniqueDefs are automatically adjusted for this trk. 
        /// </summary>
        public void ConcatCloneAt(Trk trk2, int msPositionReThisTrk)
        {
            Debug.Assert(MsDuration <= msPositionReThisTrk);
            if(msPositionReThisTrk > MsDuration)
            {
                int msDuration = (msPositionReThisTrk - MsDuration);
                IUniqueDef lastIud = _uniqueDefs[_uniqueDefs.Count - 1];
                if(lastIud is RestDef finalRestDef) // Trks are "OutputVoiceDefs" so cannot contain InputRestDefs.
                {
                    lastIud.MsDuration += msDuration;
                }
                else
                {
                    RestDef midiRestDef = new RestDef(0, msDuration);
                    this.Add(midiRestDef);
                }
            }

            foreach(IUniqueDef iu2 in trk2.UniqueDefs)
            {
                IUniqueDef lastIud = (_uniqueDefs.Count > 0) ? _uniqueDefs[_uniqueDefs.Count - 1] : null;
                IUniqueDef clonedIUD = (IUniqueDef)iu2.Clone();

                if(_uniqueDefs.Count > 0 && (lastIud is RestDef finalRestDef && clonedIUD is RestDef restDef2))
                {
                    finalRestDef.MsDuration += restDef2.MsDuration;
                }
                else
                {
                    this.Add(clonedIUD);
                }
            }
        }


        /// <summary>
        /// Removes the iUniqueMidiDurationDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(replacementIUnique.MsDuration == _uniqueDefs[index].MsDuration);

            Debug.Assert(index >= 0 && index < _uniqueDefs.Count);
            _uniqueDefs.RemoveAt(index);
            _uniqueDefs.Insert(index, replacementIUnique);
            SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        #endregion Add, Remove, Insert, Replace objects in the Trk

        #region Changing the Trk's duration
        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef from beginIndex to endIndex (exclusive) by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this Trk changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(beginIndex, endIndex, factor, minThreshold);
        }

        /// <summary>
        /// Multiplies the MsDuration of each midiChordDef in the UniqueDefs list by factor.
        /// If a midiChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this Trk changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<MidiChordDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        #endregion Changing the Trk's duration

        #region Changing MidiChordDef attributes

        #region Envelopes

        public List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(IUniqueDef iud in UniqueDefs)
            {
                originalMsPositions.Add(iud.MsPositionReFirstUD);
            }
            originalMsPositions.Add(originalMsDuration);

            return originalMsPositions;
        }

        #region SetVelocityPerAbsolutePitch
        /// <summary>
        /// The arguments are passed unchanged to MidiChordDef.SetVelocityPerAbsolutePitch(...) for each MidiChordDef in this Trk.
        /// See the MidiChordDef documentation for details.
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
        public virtual void SetVelocityPerAbsolutePitch(List<int> velocityPerAbsolutePitch)
        {
            #region conditions
            Debug.Assert(velocityPerAbsolutePitch.Count == 12);
            for(int i = 0; i < 12; ++i)
            {
                M.AssertIsVelocityValue(velocityPerAbsolutePitch[i]);
            }
            #endregion conditions
            for(int i = 0; i < UniqueDefs.Count; ++i)
            {
                if(UniqueDefs[i] is MidiChordDef mcd)
                {
                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
                    Debug.Assert(mcd.Pitches.Count > 0);
                }
            }
        }
        #endregion SetVelocityPerAbsolutePitch
        #region SetVelocitiesFromDurations
        /// <summary>
        /// The first two arguments must be in range [1..127].
        /// Sets the velocity of each MidiChordDef in the Trk (anti-)proportionally to its duration.
        /// The (optional) percent argument determines the proportion of the final velocity for which this function is responsible.
        /// The other component of the final velocity value is its existing velocity. If percent is 100.0, the existing velocity
        /// is replaced completely.
        /// N.B 1) Neither velocityForMinMsDuration nor velocityForMaxMsDuration can be zero! -- that would be a NoteOff.
        /// and 2) velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration.
        /// </summary>
        /// <param name="velocityForMinMsDuration">in range 1..127</param>
        /// <param name="velocityForMaxMsDuration">in range 1..127</param>
        public virtual void SetVelocitiesFromDurations(byte velocityForMinMsDuration, byte velocityForMaxMsDuration, double percent = 100.0)
        {
            Debug.Assert(velocityForMinMsDuration >= 1 && velocityForMinMsDuration <= 127);
            Debug.Assert(velocityForMaxMsDuration >= 1 && velocityForMaxMsDuration <= 127);

            int msDurationRangeMin = int.MaxValue;
            int msDurationRangeMax = int.MinValue;

            #region find msDurationRangeMin and msDurationRangeMax 
            foreach(IUniqueDef iud in _uniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    msDurationRangeMin = (msDurationRangeMin < mcd.MsDuration) ? msDurationRangeMin : mcd.MsDuration;
                    msDurationRangeMax = (msDurationRangeMax > mcd.MsDuration) ? msDurationRangeMax : mcd.MsDuration;
                }
            }
            #endregion find msDurationRangeMin and msDurationRangeMax

            foreach(IUniqueDef iud in _uniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    mcd.SetVelocityFromDuration(msDurationRangeMin, msDurationRangeMax, velocityForMinMsDuration, velocityForMaxMsDuration, percent);
                }
            }
        }
        #endregion SetVelocitiesFromDurations
        #region SetVerticalVelocityGradient
        /// <summary>
        /// The arguments must both be in range [1..127].
        /// This function calls MidiChordDef.SetVerticalVelocityGradient(rootVelocity, topVelocity)
        /// on all the MidiChordDefs in the Trk. 
        /// </summary>
        public void SetVerticalVelocityGradient(byte rootVelocity, byte topVelocity)
        {
            #region conditions
            Debug.Assert(rootVelocity > 0 && rootVelocity <= 127);
            Debug.Assert(topVelocity > 0 && topVelocity <= 127);
            #endregion conditions

            foreach(IUniqueDef iud in UniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    mcd.SetVerticalVelocityGradient(rootVelocity, topVelocity);
                }
            }
        }

        #endregion SetVerticalVelocityGradient
        #endregion Envelopes

        /// <summary>
        /// Preserves the MsDuration of the Trk as a whole by resetting it after doing the following:
        /// 1. Creates a sorted list of the unique bottom or top pitches in all the MidiChordDefs in the Trk.
        ///    The use of the bottom or top pitch is controlled by argument 3: useBottomPitch.
        /// 2. Creates a duration per pitch dictionary, whereby durationPerPitch[lowestPitch] is durationForLowestPitch
        ///    and durationPerPitch[lowestPitch] is durationForHighestPitch. The intermediate duration values are
        ///    interpolated logarithmically.
        /// 3. Sets the MsDuration of each MidiChordDef to (percent * the values found in the duration per pitch dictionary) plus
        ///   ((100-percent)percent * the original durations). Rest msDurations are left unchanged at this stage.
        /// 4. Resets the MsDuration of the Trk to its original value.
        /// N.B. a Debug.Assert() fails if an attempt is made to set the msDuration of a BasicMidiChordDef to zero.
        /// </summary>
        /// <param name="durationForLowestPitch"></param>
        /// <param name="durationForHighestPitch"></param>
        public void SetDurationsFromPitches(int durationForLowestPitch, int durationForHighestPitch, bool useBottomPitch, double percent = 100.0)
        {
            Debug.Assert(percent >= 0 && percent <= 100);

            List<int> pitches = new List<int>();
            #region get pitches
            foreach(IUniqueDef iud in _uniqueDefs)
            {
                if(iud is MidiChordDef mcd)
                {
                    int pitch = (useBottomPitch == true) ? mcd.Pitches[0] : mcd.Pitches[mcd.Pitches.Count - 1];
                    if(!pitches.Contains(pitch))
                    {
                        pitches.Add(pitch);
                    }
                }
            }
            #endregion get pitches
            if(pitches.Count == 1)
            {
                int mcdMsDuration = (useBottomPitch == true) ? durationForLowestPitch : durationForHighestPitch;
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    if(iud is MidiChordDef mcd)
                    {
                        mcd.MsDuration = mcdMsDuration;
                    }
                }
            }
            else
            {
                Debug.Assert(pitches.Count > 1);
                pitches.Sort();
                #region get durations
                int nPitches = pitches.Count;
                double factor = Math.Pow((((double)durationForHighestPitch) / durationForLowestPitch), (((double)1) / (nPitches - 1)));
                List<int> msDurations = new List<int>();
                double msDuration = durationForLowestPitch;
                foreach(byte pitch in pitches)
                {
                    msDurations.Add((int)Math.Round(msDuration));
                    msDuration *= factor;
                }
                Debug.Assert(msDurations.Count == nPitches);
                Debug.Assert(msDurations[msDurations.Count - 1] == durationForHighestPitch);
                #endregion get durations
                #region get duration per pitch dictionary
                Dictionary<int, int> msDurPerPitch = new Dictionary<int, int>();   // pitch, msDuration
                for(int i = 0; i < nPitches; ++i)
                {
                    msDurPerPitch.Add(pitches[i], msDurations[i]);
                }
                #endregion get get duration per pitch dictionary
                #region set durations
                double factorForNewValue = percent / 100.0;
                double factorForOldValue = 1 - factorForNewValue;
                int currentMsDuration = this.MsDuration;
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    if(iud is MidiChordDef mcd)
                    {
                        int pitch = (useBottomPitch == true) ? mcd.Pitches[0] : mcd.Pitches[mcd.Pitches.Count - 1];
                        int oldDuration = mcd.MsDuration;
                        int durPerPitch = msDurPerPitch[pitch];
                        mcd.MsDuration = (int)Math.Round((oldDuration * factorForOldValue) + (durPerPitch * factorForNewValue));
                    }
                }
                this.MsDuration = currentMsDuration;
                #endregion set durations
            }
        }

        /// <summary>
        /// Multiplies each expression value in the MidiChordDefs
        /// from beginIndex to (excluding) endIndex by the argument factor.
        /// </summary>
        public void AdjustExpression(int beginIndex, int endIndex, double factor)
        {
            if(CheckIndices(beginIndex, endIndex))
            {
                for(int i = beginIndex; i < endIndex; ++i)
                {
                    if(_uniqueDefs[i] is MidiChordDef iumdd)
                    {
                        iumdd.MidiChordControlDefs.Expression = (int)Math.Round((int)iumdd.MidiChordControlDefs.Expression * factor);
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs from beginIndex to (exclusive) endIndex by
        /// the argument factor (which must be greater than zero).
        /// The resulting velocities are in range 1..127.
        /// If a resulting velocity would have been less than 1, it is silently coerced to 1.
        /// If it would have been greater than 127, it is silently coerced to 127.
        /// </summary>
        public virtual void AdjustVelocities(int beginIndex, int nonInclusiveEndIndex, double factor)
        {
            if(CheckIndices(beginIndex, nonInclusiveEndIndex))
            {
                Debug.Assert(factor > 0.0);
                for(int i = beginIndex; i < nonInclusiveEndIndex; ++i)
                {
                    if(_uniqueDefs[i] is MidiChordDef mcd)
                    {
                        mcd.AdjustVelocities(factor);
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs by the argument factor (must be greater than zero).
        /// N.B MidiChordDefs will be turned into RestDefs if all their notes have zero velocity!
        /// </summary>
        public virtual void AdjustVelocities(double factor)
        {
            Debug.Assert(factor > 0.0);
            for(int i = 0; i < UniqueDefs.Count; ++i)
            {
                if(UniqueDefs[i] is MidiChordDef mcd)
                {
                    mcd.AdjustVelocities(factor);
                    if(mcd.Pitches.Count == 0)
                    {
                        Replace(i, new RestDef(mcd.MsPositionReFirstUD, mcd.MsDuration));
                    }
                }
            }
        }
        /// <summary>
        /// Creates a hairpin in the velocities from the IUniqueDef at beginIndex to the IUniqueDef at endIndex - 1 (inclusive).
        /// This function does NOT change velocities outside the range given in its arguments.
        /// There must be at least two IUniqueDefs in the msPosition range given in the arguments.
        /// The factors by which the velocities are multiplied change arithmetically:
        /// The velocity of the the IUniqueDef at beginIndex is multiplied by startFactor, and
        /// the velocity of the the IUniqueDef at endIndex is multiplied by endFactor.
        /// Can be used to create a diminueno or crescendo, or (using multiple calls) hairpins.
        /// N.B MidiChordDefs will be turned into RestDefs if all their notes have zero velocity!
        /// </summary>
        /// <param name="startIndex">index of start UniqueDef (range 0 to this.Count - 2)</param>
        /// <param name="endIndex">index of end UniqueDef (range startIndex + 1 to this.Count - 1)</param>
        /// <param name="startFactor">greater than or equal to 0</param>
        /// <param name="endFactor">greater than or equal to 0</param>
        public virtual void AdjustVelocitiesHairpin(int beginIndex, int endIndex, double startFactor, double endFactor)
        {
            if(CheckIndices(beginIndex, endIndex))
            {
                Debug.Assert(startFactor >= 0 && endFactor >= 0);
                int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);
                int steps = endIndex - 1 - beginIndex - nNonMidiChordDefs;
                if(steps > 0)
                {
                    double factorIncrement = (endFactor - startFactor) / steps;
                    double factor = startFactor;
                    List<IUniqueDef> lmdds = _uniqueDefs;

                    for(int i = beginIndex; i < endIndex; ++i)
                    {
                        MidiChordDef mcd = _uniqueDefs[i] as MidiChordDef;
                        if(mcd != null)
                        {
                            mcd.AdjustVelocities(factor);
                            factor += factorIncrement;
                        }
                        if(mcd.Pitches.Count == 0)
                        {
                            Replace(i, new RestDef(mcd.MsPositionReFirstUD, mcd.MsDuration));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a moving pan from startPanValue at beginIndex to endPanValue at (beginIndex + count - 1).
        /// (In other words, it sets count MidiChordDefs.)
        /// Implemented using one pan value per MidiChordDef.
        /// This function does NOT change pan values outside the position range given in its arguments.
        /// </summary>
        /// <param name="beginIndex">The index at which to start setting pan values</param>
        /// <param name="count">The number of IUniqueDefs to set (among these, only MidiChordDefs will be set)</param>
        /// <param name="startPanValue">The MSB of the initial pan value (in range 0..127)</param>
        /// <param name="endPanValue">The MSB of the final pan value (in range 0..127)</param>
        public void SetPanGliss(int beginIndex, int count, int startPanValue, int endPanValue)
        {
            int endIndex = beginIndex + count;
            if(CheckIndices(beginIndex, endIndex))
            {
                Debug.Assert(startPanValue >= 0 && startPanValue <= 127 && endPanValue >= 0 && endPanValue <= 127);

                int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);
                int steps = (endIndex - 1 - beginIndex - nNonMidiChordDefs);
                if(steps > 0)
                {
                    double increment = ((double)(endPanValue - startPanValue)) / steps;
                    double panValue = startPanValue;
                    List<IUniqueDef> lmdds = _uniqueDefs;

                    for(int i = beginIndex; i < endIndex; ++i)
                    {
                        if(_uniqueDefs[i] is MidiChordDef iumdd)
                        {
                            int intPan = (int)Math.Round(panValue);
                            iumdd.MidiChordControlDefs.Pan = intPan;
                            panValue += increment;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Sets the pitchwheelDeviation for MidiChordDefs in the defined range.
        /// Rests in the range dont change.
        /// </summary>
        /// <param name="beginIndex">The index at which to start setting pitchWheelDeviations</param>
        /// <param name="count">The number of IUniqueDefs to set (among these, only MidiChordDefs will be set)</param>
        public void SetPitchWheelDeviation(int beginIndex, int count, int deviation)
        {
            int endIndex = beginIndex + count;
            if(CheckIndices(beginIndex, endIndex))
            {
                for(int i = beginIndex; i < endIndex; ++i)
                {
                    if(this[i] is MidiChordDef mcd)
                    {
                        mcd.MidiChordControlDefs.VelocityPitchSensitivity = M.MidiValue(deviation);
                    }
                }
            }
        }
        /// <summary>
        /// Removes the pitchwheel commands (not the pitchwheelDeviations)
        /// from chords in the range beginIndex to endIndex inclusive.
        /// Rests in the range are not changed.
        /// </summary>
        public void RemoveScorePitchWheelCommands(int beginIndex, int endIndex)
        {
            if(CheckIndices(beginIndex, endIndex))
            {
                for(int i = beginIndex; i < endIndex; ++i)
                {
                    if(this[i] is MidiChordDef umcd)
                    {
                        umcd.MidiChordControlDefs.PitchWheel = null;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an exponential change (per index) of pitchwheelDeviation from beginIndex to endIndex,
        /// </summary>
        /// <param name="finale"></param>
        protected void AdjustPitchWheelDeviations(int beginIndex, int endIndex, int pwValueAtBeginIndex, int pwValueAtEndIndex)
        {
            Debug.Assert(beginIndex >= 0 && beginIndex < endIndex && endIndex < Count);
            Debug.Assert(pwValueAtBeginIndex >= 0 && pwValueAtEndIndex >= 0);
            Debug.Assert(pwValueAtBeginIndex <= 127 && pwValueAtEndIndex <= 127);

            int nNonMidiChordDefs = GetNumberOfNonMidiOrInputChordDefs(beginIndex, endIndex);

            double pwdfactor = Math.Pow(pwValueAtEndIndex / pwValueAtBeginIndex, (double)1 / (endIndex - beginIndex - nNonMidiChordDefs)); // f13.Count'th root of furies1EndPwdValue/furies1StartPwdValue -- the last pwd should be furies1EndPwdValue

            for(int i = beginIndex; i < endIndex; ++i)
            {
                if(_uniqueDefs[i] is MidiChordDef umc)
                {
                    umc.MidiChordControlDefs.PitchWheelSensitivity = M.MidiValue(pwValueAtBeginIndex * (Math.Pow(pwdfactor, i)));
                }
            }
        }


        #endregion Changing MidiChordDef attributes

        #region alignment
        /// <summary>
        /// _uniqueDefs[indexToAlign] is moved to toMsPositionReFirstIUD, and the surrounding symbols are spread accordingly
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

            AssertConsistency();;
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
            string msg = "\nError in ChannelDef.cs,\nfunction AlignDefMsPosition()\n\n";
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

        #region Re-ordering the Trk's UniqueDefs

        #region Permute()
        /// <summary>
        /// This function permutes any number of UniqueDefs in the trk's UniqueDefs list according to the contour retrieved (from
        /// the static K.Contour[] array) using the axisNumber and contourNumber arguments. The trk's AxisIndex property is set.
        /// If there are more than 7 UniqueDefs in the list, 7 partitions are automatically created and permuted recursively
        /// using the same contour.
        /// </summary>
        /// <param name="axisNumber">A value in range 1..12 inclusive.</param>
        /// <param name="contourNumber">A value in range 1..12 inclusive.</param>
        public virtual void Permute(int axisNumber, int contourNumber)
        {
            Debug.Assert(!(contourNumber < 1 || contourNumber > 12), "contourNumber out of range 1..12");
            Debug.Assert(!(axisNumber < 1 || axisNumber > 12), "axisNumber out of range 1..12");

            PermuteRecursively(axisNumber, contourNumber, _uniqueDefs);
        }

        /// <summary>
        /// Re-orders the UniqueDefs in (part of) this Trk.
        /// <para>1. Creates a list of not more than 7 partitions that are as equal in length as possible.</para>   
        /// <para>2. Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array) using</para>
        /// <para>-  the axisNumber and contourNumber arguments.</para>
        /// <para>3  Re-orders the UniqueDefs in each partition whose count is greater than 1 by calling itself recursively.</para>
        /// <para>4. Resets the UniqueDefs list to the concatenation of the partitions (that have been re-ordered internally and externally).</para>
        /// </summary>.
        /// <param name="uniqueDefs">Call this function with the trk's UniqueDefs list.</param>
        /// <param name="axisNumber">A value greater than or equal to 1, and less than or equal to 12 An exception is thrown if this is not the case.</param>
        /// <param name="contourNumber">A value greater than or equal to 1, and less than or equal to 12. An exception is thrown if this is not the case.</param>
        private void PermuteRecursively(int axisNumber, int contourNumber, List<IUniqueDef> uniqueDefs)
        {
            List<List<IUniqueDef>> partitions = GetPartitionsOfEqualLength(uniqueDefs);

            Debug.Assert(partitions.Count > 0 && partitions.Count <= 7);

            IUniqueDef axisUniqueDef = uniqueDefs[0];

            if(partitions.Count > 1)
            {
                // re-order the partitions
                partitions = DoContouring(axisNumber, contourNumber, partitions);
                int axisPartitionIndex = GetPartitionIndex(partitions.Count, axisNumber);
                List<IUniqueDef> axisPartition = partitions[axisPartitionIndex];
                axisUniqueDef = axisPartition[0];
                foreach(List<IUniqueDef> partition in partitions)
                {
                    if(partition.Count > 1)
                    {
                        PermuteRecursively(axisNumber, contourNumber, partition); // recursive call
                    }
                }
            }

            List<IUniqueDef> sortedLmdds = ConvertPartitionsToFlatIUDs(partitions);

            for(int i = 0; i < sortedLmdds.Count; ++i)
            {
                uniqueDefs[i] = sortedLmdds[i];
            }

            // only do the following at the top level of the recursion
            if(uniqueDefs == _uniqueDefs)
            {
                AxisIndex = _uniqueDefs.FindIndex(u => (u == axisUniqueDef));
                AssertConsistency();;
            }
        }
        /// <summary>
        /// Returns a list of partitions (each of which is a list of IUniqueDefs)
        /// The returned list:
        ///     * is as long as possible, but contains not more than 7 partitions.
        ///     * contains partitions whose Count is distributed as evenly as possible along the list. 
        /// </summary>
        private List<List<IUniqueDef>> GetPartitionsOfEqualLength(List<IUniqueDef> uniqueDefs)
        {
            Debug.Assert(uniqueDefs.Count > 0);

            List<int> partitionSizes = GetEqualPartitionSizes(uniqueDefs.Count);
            List<List<IUniqueDef>> partitions = new List<List<IUniqueDef>>();
            int lmddIndex = 0;
            foreach(int size in partitionSizes)
            {
                List<IUniqueDef> partition = new List<IUniqueDef>();
                for(int i = 0; i < size; ++i)
                {
                    partition.Add(uniqueDefs[lmddIndex++]);
                }
                partitions.Add(partition);
            }
            return partitions;
        }
        /// <summary>
        /// Returns a list that adds up to count.
        /// The returned list:
        ///     * is as long as possible, but contains not more than 7 ints.
        ///     * contains values that are distributed evenly along the list. 
        /// </summary>
        /// <returns></returns>
        private List<int> GetEqualPartitionSizes(int count)
        {
            List<int> partitionSizes = new List<int>();
            if(count > 7)
            {
                partitionSizes = M.IntDivisionSizes(count, 7);
            }
            else
            {
                for(int i = 0; i < count; ++i)
                {
                    partitionSizes.Add(1);
                }
            }
            return partitionSizes;
        }
        #endregion Permute()

        #region PermutePartitions()
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
        public virtual void PermutePartitions(int axisNumber, int contourNumber, List<int> partitionSizes)
        {
            CheckPermutePartitionsArgs(axisNumber, contourNumber, partitionSizes);

            IUniqueDef axisUniqueDef = UniqueDefs[0];

            List<List<IUniqueDef>> partitions = GetPartitionsFromPartitionSizes(partitionSizes);

            if(partitions.Count > 1)
            {
                // re-order the partitions
                partitions = DoContouring(axisNumber, contourNumber, partitions);
                int axisPartitionIndex = GetPartitionIndex(partitions.Count, axisNumber);
                List<IUniqueDef> axisPartition = partitions[axisPartitionIndex];
                axisUniqueDef = axisPartition[0];
            }

            List<IUniqueDef> sortedLmdds = ConvertPartitionsToFlatIUDs(partitions);

            for(int i = 0; i < sortedLmdds.Count; ++i)
            {
                _uniqueDefs[i] = sortedLmdds[i];
            }

            AxisIndex = _uniqueDefs.FindIndex(u => (u == axisUniqueDef));

            AssertConsistency();;
        }

        /// <summary>
        /// Debug.Assert fails if one of the following conditions is not met.
        /// <para>axisNumber must be in the range 1..12</para>
        /// <para>contourNumber must be in the range 1..12</para>
        /// <para>partitionSizes.Count must be greater than 0, and less than 8.</para>
        /// <para>each partitionSize must be greater then 0.</para>
        /// <para>the sum of all the partition sizes must be equal to UniqueDefs.Count</para>
        /// </summary>
        private void CheckPermutePartitionsArgs(int axisNumber, int contourNumber, List<int> partitionSizes)
        {
            Debug.Assert(!(axisNumber < 1 || axisNumber > 12), "axisNumber out of range 1..12");
            Debug.Assert(!(contourNumber < 1 || contourNumber > 12), "contourNumber out of range 1..12");

            Debug.Assert(!(partitionSizes.Count < 1 || partitionSizes.Count > 7), "partitionSizes.Count must be in range 1..7");

            int totalPartitionSizes = 0;
            foreach(int size in partitionSizes)
            {
                Debug.Assert(size >= 1, "each partition must contain at least one IUniqueDef");
                totalPartitionSizes += size;
            }
            Debug.Assert((totalPartitionSizes == _uniqueDefs.Count), "Sum of partition sizes does not match number of UniqueDefs.");
        }
        private List<List<IUniqueDef>> GetPartitionsFromPartitionSizes(List<int> partitionSizes)
        {
            List<List<IUniqueDef>> partitions = new List<List<IUniqueDef>>();
            int lmddIndex = 0;
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
        #endregion PermutePartitions

        #region common to Permute functions
        /// <summary>
        /// Re-orders the partitions according to the contour retrieved (from the static K.Contour[] array)
        /// <para>using partitions.Count and the contourNumber and axisNumber arguments.</para>
        /// <para>Does not change the inner contents of the partitions themselves.</para>
        /// </summary>
        /// <returns>A re-ordered list of partitions</returns>
        private List<List<IUniqueDef>> DoContouring(int axisNumber, int contourNumber, List<List<IUniqueDef>> partitions)
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
        /// The index of the value before the axis in the standard contour diagrams.
        /// (i.e. the position of the axis in the diagrams, minus 2)
        /// </summary>
        /// <param name="domain">In range [1..7]</param>
        /// <param name="axisNumber">In range [1..12]</param>
        private int GetPartitionIndex(int domain, int axisNumber)
        {
            Debug.Assert(domain > 0 && domain <= 7);
            Debug.Assert(axisNumber > 0 && axisNumber <= 12);

            return axisIndices[domain - 1, axisNumber - 1];
        }
        private static readonly int[,] axisIndices =
        {
            {0,0,0,0,0,0,0,0,0,0,0,0}, // domain 1
            {0,0,0,0,0,0,0,0,0,0,0,0}, // domain 2
            {0,0,0,0,0,0,1,1,1,1,1,1}, // domain 3
            {0,0,0,0,1,1,2,2,2,2,1,1}, // domain 4
            {0,0,0,1,1,2,3,3,3,2,2,1}, // domain 5
            {0,0,0,1,2,3,4,4,4,3,2,1}, // domain 6
            {0,0,1,2,3,4,5,5,4,3,2,1}  // domain 7
        };

        private List<IUniqueDef> ConvertPartitionsToFlatIUDs(List<List<IUniqueDef>> partitions)
        {
            List<IUniqueDef> newIUDs = new List<IUniqueDef>();
            int msPositionReFirstIUD = 0;
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
        #endregion common to Permute functions

        #endregion Re-ordering the Trk's UniqueDefs

        #region Enumerators
        public IEnumerable<MidiChordDef> MidiChordDefs
        {
            get
            {
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    if(iud is MidiChordDef midiChordDef)
                        yield return midiChordDef;
                }
            }
        }
        #endregion

        /// <summary>
        /// Indexer. Allows individual UniqueDefs to be accessed using array notation on the Trk.
        /// The setter automatically resets the MsPositions of all UniqueDefs in the list.
        /// e.g. iumdd = channelDef[3].
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

                _uniqueDefs[i] = value;
                SetMsPositionsReFirstUD();
                AssertConsistency();;
            }
        }

        public override string ToString()
        {
            return ($"Trk: MsDuration={MsDuration}, Count={Count}");
        }

        /// <summary>
        /// This value is used by Seq.AlignTrkAxes(). It is set by the Permute functions, but can also be set manually.
        /// It is the index of the UniqueDef (in the UniqueDefs list) that will be aligned when calling Seq.AlignTrkAxes().
        /// </summary>
        public int AxisIndex
        {
            get
            {
                return _axisIndex;
            }
            set
            {
                Debug.Assert(_axisIndex < UniqueDefs.Count);
                _axisIndex = value;
            }
        }
        private int _axisIndex = 0;

        /// <summary>
        /// The number of MidiChordDefs and RestDefs in this Trk
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

        //public int MidiChannel { get; set; }

        /// <summary>
        /// Setting this property stretches or compresses all the durations in the UniqueDefs list to fit the given total duration.
        /// This does not change the ChannelDef's MsPosition, but does affect its EndMsPosition.
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

                AssertConsistency();;
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
    }
}
