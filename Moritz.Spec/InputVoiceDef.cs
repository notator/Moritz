
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    /// <summary>
    /// During construction, InputVoiceDefs only contain InputChordDef and InputRestDef objects.
    /// In Blocks, they may also contain CautionaryChordDef objects.
    /// <para></para>
    /// <para>This class is IEnumerable, so that foreach loops can be used.</para>
    /// <para>For example:</para>
    /// <para>foreach(IUniqueDef iumdd in inputVoiceDef) { ... }</para>
    /// <para>An Enumerator for InputChordDefs is also defined so that</para>
    /// <para>foreach(InputChordDef mcd in inputVoiceDef.InputChordDefs) { ... }</para>
    /// <para>can also be used.</para>
    /// <para>This class is also indexable, as in:</para>
    /// <para>IUniqueDef iu = inputVoiceDef[index];</para>
    /// </summary>
    public class InputVoiceDef : VoiceDef
    {
        #region constructors
        public InputVoiceDef(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, iuds)
        {
            foreach(IUniqueDef iud in UniqueDefs)
            {
                // In blocks, inputChordDefs can also contain CautionaryChordDefs
                Debug.Assert(iud is InputChordDef || iud is InputRestDef);
            }
        }

        /// <summary>
        /// Returns a deep clone of this InputVoiceDef.
        /// </summary>
        public InputVoiceDef Clone()
        {
            List<IUniqueDef> clonedLmdds = new List<IUniqueDef>();
            foreach(IUniqueDef iu in this._uniqueDefs)
            {
                IUniqueDef clone = (IUniqueDef) iu.Clone();
                clonedLmdds.Add(clone);
            }
            var ivd = new InputVoiceDef(this.MidiChannel, this.MsPositionReContainer, clonedLmdds);
            ivd.Container = this.Container; 
            return ivd;
        }
		#endregion constructors

		/// <summary>
		/// When this function is called, VoiceDef.AssertConsistency() is called (see documentation there), then
		/// the following are checked:
		/// 1. MidiChannels are in range [0..3]
		/// 2. The Container is either null or a Bar.
		/// 3. If the Container is null, the UniqueDefs can contain any combination of InputChordDef and InputRestDef.
		///    If the Container is a Bar, the UniqueDefs can also contain ClefDef and CautionaryChordDef objects.
		/// </summary>
		public override void AssertConsistency()
        {
			base.AssertConsistency();

			Debug.Assert(MidiChannel >= 0 && MidiChannel <= 3);	 // max 4 input channels

			Debug.Assert(Container == null || Container is Bar);

			if(Container is Bar)
			{
				foreach(IUniqueDef iud in UniqueDefs)
				{
					Debug.Assert(iud is InputChordDef || iud is InputRestDef || iud is CautionaryChordDef || iud is ClefDef);
				}
			}
			else // during construction
			{
				foreach(IUniqueDef iud in UniqueDefs)
				{
					Debug.Assert(iud is InputChordDef || iud is InputRestDef);
				}
			}
		}

        #region Count changers
        /// <summary>
        /// Appends the new iUniqueDef to the end of the list. Sets the MsPosition of the iUniqueDef re the first iUniqueDef in the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// </summary>
        /// <param name="iUniqueDef"></param>
        public override void Add(IUniqueDef iud)
        {
            Debug.Assert(iud is InputChordDef || iud is InputRestDef || iud is CautionaryChordDef || iud is ClefDef);
            _Add(iud);
        }
        /// <summary>
        /// Adds the argument to the end of this VoiceDef.
        /// Sets the MsPositions of the appended UniqueDefs re the first iUniqueDef in the list.
        /// </summary>
        public override void AddRange(VoiceDef inputVoiceDef)
        {
            Debug.Assert(inputVoiceDef is InputVoiceDef);
            _AddRange(inputVoiceDef);
        }

        /// <summary>
        /// Adds the argument to the end of this VoiceDef.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public void Concat(InputVoiceDef inputVoiceDef)
        {
            int thisEndMsPositionReSeq = this.EndMsPositionReFirstIUD + this.MsPositionReContainer;
            if(inputVoiceDef.MsPositionReContainer > thisEndMsPositionReSeq)
            {
                InputRestDef rest = new InputRestDef(this.EndMsPositionReFirstIUD, inputVoiceDef.MsPositionReContainer - thisEndMsPositionReSeq);
                this.Add(rest);
            }
            _AddRange(inputVoiceDef);
        }

        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(iUniqueDef is InputChordDef || iUniqueDef is InputRestDef || iUniqueDef is CautionaryChordDef || iUniqueDef is ClefDef);
            _Insert(index, iUniqueDef);
        }
        /// <summary>
        /// Inserts the voiceDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public void InsertRange(int index, InputVoiceDef voiceDef)
        {
            _InsertRange(index, (VoiceDef) voiceDef);
        }

        /// <summary>
        /// This function has been moved here from the VoiceDef class since it is no longer used by Trks.
        /// It needs re-testing... Should it take account of this.MsPositionReContainer? See Trk.Superimpose(...).
        /// </summary>
        private void _InsertInRest(InputVoiceDef iVoiceDef)
        {
            Debug.Assert(!(Container is Bar), "Cannot Insert a Trk in a InputRestDef inside a Bar.");

            int iLmddsStartMsPosReFirstIUD = iVoiceDef[0].MsPositionReFirstUD;
            int iLmddsEndMsPosReFirstIUD = iVoiceDef[iVoiceDef.Count - 1].MsPositionReFirstUD + iVoiceDef[iVoiceDef.Count - 1].MsDuration;

            int restIndex = FindIndexOfSpanningRest(iLmddsStartMsPosReFirstIUD, iLmddsEndMsPosReFirstIUD);

            // if index >= 0, it is the index of a rest into which the chord will fit.
            if(restIndex >= 0)
            {
                InsertVoiceDefInRest(restIndex, iVoiceDef);
                SetMsPositionsReFirstUD(); // just to be sure!
            }
            else
            {
                Debug.Assert(false, "Can't find a rest spanning the chord!");
            }

            AssertConsistency();
        }
        #region _InsertInRest() implementation
        /// <summary>
        /// Returns the index of a rest spanning startMsPos..endMsPos
        /// If there is no such rest, -1 is returned.
        /// </summary>
        /// <param name="startMsPos"></param>
        /// <param name="endMsPos"></param>
        /// <returns></returns>
        private int FindIndexOfSpanningRest(int startMsPos, int endMsPos)
        {
            List<IUniqueDef> lmdds = _uniqueDefs;
            int index = -1, restStartMsPosReFirstIUD = -1, restEndMsPosReFirstIUD = -1;

            for(int i = 0; i < lmdds.Count; ++i)
            {
                InputRestDef umrd = lmdds[i] as InputRestDef;
                if(umrd != null)
                {
                    restStartMsPosReFirstIUD = lmdds[i].MsPositionReFirstUD;
                    restEndMsPosReFirstIUD = lmdds[i].MsPositionReFirstUD + lmdds[i].MsDuration;

                    if(startMsPos >= restStartMsPosReFirstIUD && endMsPos <= restEndMsPosReFirstIUD)
                    {
                        index = i;
                        break;
                    }
                    if(startMsPos < restStartMsPosReFirstIUD)
                    {
                        break;
                    }
                }
            }

            return index;
        }

        private void InsertVoiceDefInRest(int restIndex, VoiceDef iVoiceDef)
        {
            List<IUniqueDef> lmdds = _uniqueDefs;
            IUniqueDef rest = lmdds[restIndex];
            List<IUniqueDef> replacement = GetReplacementList(rest, iVoiceDef);
            int replacementStartMsPosReFirstIUD = replacement[0].MsPositionReFirstUD;
            int replacementEndMsPosReFirstIUD = replacement[replacement.Count - 1].MsPositionReFirstUD + replacement[replacement.Count - 1].MsDuration;
            int restStartMsPosReFirstIUD = rest.MsPositionReFirstUD;
            int restEndMsPosReFirstIUD = rest.MsPositionReFirstUD + rest.MsDuration;
            Debug.Assert(restStartMsPosReFirstIUD == replacementStartMsPosReFirstIUD && restEndMsPosReFirstIUD == replacementEndMsPosReFirstIUD);
            lmdds.RemoveAt(restIndex);
            lmdds.InsertRange(restIndex, replacement);
        }
        /// <summary>
        /// Returns a list having the position and duration of the originalRest.
        /// The iLmdds have been put in(side) the original rest, either at the beginning, middle, or end. 
        /// </summary>
        private List<IUniqueDef> GetReplacementList(IUniqueDef originalRest, VoiceDef iVoiceDef)
        {
            Debug.Assert(originalRest is InputRestDef || originalRest is MidiRestDef);
            Debug.Assert(iVoiceDef[0] is MidiChordDef || iVoiceDef[0] is InputChordDef);
            Debug.Assert(iVoiceDef[iVoiceDef.Count - 1] is MidiChordDef || iVoiceDef[iVoiceDef.Count - 1] is InputChordDef);

            List<IUniqueDef> rList = new List<IUniqueDef>();
            if(iVoiceDef[0].MsPositionReFirstUD > originalRest.MsPositionReFirstUD)
            {
                RestDef rest1;
                if(originalRest is MidiRestDef)
                {
                    rest1 = new MidiRestDef(originalRest.MsPositionReFirstUD, iVoiceDef[0].MsPositionReFirstUD - originalRest.MsPositionReFirstUD);
                }
                else
                {
                    rest1 = new InputRestDef(originalRest.MsPositionReFirstUD, iVoiceDef[0].MsPositionReFirstUD - originalRest.MsPositionReFirstUD);
                }
                rList.Add(rest1);
            }
            rList.AddRange(iVoiceDef.UniqueDefs);
            int iudEndMsPosReFirstIUD = iVoiceDef[iVoiceDef.Count - 1].MsPositionReFirstUD + iVoiceDef[iVoiceDef.Count - 1].MsDuration;
            int originalRestEndMsPosReFirstIUD = originalRest.MsPositionReFirstUD + originalRest.MsDuration;
            if(originalRestEndMsPosReFirstIUD > iudEndMsPosReFirstIUD)
            {
                RestDef rest2;
                if(originalRest is MidiRestDef)
                {
                    rest2 = new MidiRestDef(iudEndMsPosReFirstIUD, originalRestEndMsPosReFirstIUD - iudEndMsPosReFirstIUD);
                }
                else
                {
                    rest2 = new InputRestDef(iudEndMsPosReFirstIUD, originalRestEndMsPosReFirstIUD - iudEndMsPosReFirstIUD);
                }
                rList.Add(rest2);
            }

            return rList;
        }
        #endregion InsertInRest() implementation

        /// <summary>
        /// Creates a new InputVoiceDef containing just the argument inputChordDef,
        /// then calls the other InsertInRest() function with the voiceDef as argument. 
        /// </summary>
        public void InsertInRest(InputChordDef inputChordDef)
        {
            List<IUniqueDef> iuds = new List<IUniqueDef>() { inputChordDef };
            InputVoiceDef iVoiceDef = new InputVoiceDef(this.MidiChannel, 0, iuds);
            InsertInRest(iVoiceDef);
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
        public void InsertInRest(InputVoiceDef inputVoiceDef)
        {
            Debug.Assert(inputVoiceDef[0] is InputChordDef && inputVoiceDef[inputVoiceDef.Count - 1] is InputChordDef);
            _InsertInRest(inputVoiceDef);
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(!(replacementIUnique is MidiChordDef));
            _Replace(index, replacementIUnique);
        }
        #endregion Count changers

        #region InputVoiceDef duration changers
        /// <summary>
        /// Multiplies the MsDuration of each inputChordDef from beginIndex to endIndex (exclusive) by factor.
        /// If a inputChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this VoiceDef changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(int beginIndex, int endIndex, double factor, int minThreshold = 100)
        {
            AdjustMsDurations<InputChordDef>(beginIndex, endIndex, factor, minThreshold);
        }
        /// <summary>
        /// Multiplies the MsDuration of each inputChordDef in the UniqueDefs list by factor.
        /// If a inputChordDef's MsDuration becomes less than minThreshold, it is removed.
        /// The total duration of this InputVoiceDef changes accordingly.
        /// </summary>
        public void AdjustChordMsDurations(double factor, int minThreshold = 100)
        {
            AdjustMsDurations<InputChordDef>(0, _uniqueDefs.Count, factor, minThreshold);
        }
        #endregion InputVoiceDef duration changers
    }
}
