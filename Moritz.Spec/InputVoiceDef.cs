
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    /// <summary>
    /// During construction, InputVoiceDefs only contain InputChordDef and RestDef objects.
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
                Debug.Assert(iud is InputChordDef || iud is RestDef);
            }
        }

        /// <summary>
        /// An InputVoiceDef with msPositionReContainer=0 and an empty UniqueDefs list.
        /// This constructor is used by Block.PopBar(...).
        /// </summary>
        public InputVoiceDef(int midiChannel)
            : base(midiChannel, 0, new List<IUniqueDef>())
        {
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

        internal override void AssertConsistentInBlock()
        {
            foreach(IUniqueDef iud in UniqueDefs)
            {
                // In blocks, inputChordDefs can also contain CautionaryChordDefs
                Debug.Assert(iud is InputChordDef || iud is RestDef || iud is CautionaryChordDef || iud is ClefChangeDef);
            }
        }

        #region Count changers
        /// <summary>
        /// Appends the new iUniqueDef to the end of the list. Sets the MsPosition of the iUniqueDef re the first iUniqueDef in the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// Used by Block.PopBar(...), so accepts a CautionaryChordDef argument.
        /// </summary>
        /// <param name="iUniqueDef"></param>
        public override void Add(IUniqueDef iud)
        {
            Debug.Assert(iud is InputChordDef || iud is RestDef || iud is CautionaryChordDef || iud is ClefChangeDef);
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
                RestDef rest = new RestDef(this.EndMsPositionReFirstIUD, inputVoiceDef.MsPositionReContainer - thisEndMsPositionReSeq);
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
            Debug.Assert(iUniqueDef is InputChordDef || iUniqueDef is RestDef || iUniqueDef is CautionaryChordDef);
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
            _InsertInRest((VoiceDef)inputVoiceDef);
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
        /// Multiplies the MsDuration of each inputChordDef from beginIndex to (not including) endIndex by factor.
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
