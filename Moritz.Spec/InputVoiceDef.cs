
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
        /// <summary>
        /// An empty InputVoiceDef
        /// </summary>
        /// <param name="msDuration"></param>
        public InputVoiceDef()
            : base()
        {
        }

        /// <summary>
        /// A VoiceDef beginning at MsPosition = 0, and containing a single RestDef having msDuration
        /// </summary>
        /// <param name="msDuration"></param>
        public InputVoiceDef(int msDuration)
            : base(msDuration)
        {
        }

        /// <summary>
        /// <para>If the argument is not empty, the MsPositions and MsDurations in the list are checked for consistency.</para>
        /// <para>The new VoiceDef's UniqueDefs list is simply set to the argument (which is not cloned).</para>
        /// </summary>
        public InputVoiceDef(List<IUniqueDef> iuds) 
            : base(iuds)
        {
        }

        /// <summary>
        /// Returns a deep clone of this InputVoiceDef.
        /// </summary>
        public InputVoiceDef DeepClone()
        {
            List<IUniqueDef> clonedLmdds = new List<IUniqueDef>();
            foreach(IUniqueDef iu in this._uniqueDefs)
            {
                IUniqueDef clone = iu.Clone();
                clonedLmdds.Add(clone);
            }

            // Clefchange symbols must point at the following object in their own VoiceDef
            for(int i = 0; i < clonedLmdds.Count; ++i)
            {
                ClefChangeDef clone = clonedLmdds[i] as ClefChangeDef;
                if(clone != null)
                {
                    Debug.Assert(i < (clonedLmdds.Count - 1));
                    ClefChangeDef replacement = new ClefChangeDef(clone.ClefType, clonedLmdds[i + 1]);
                    clonedLmdds.RemoveAt(i);
                    clonedLmdds.Insert(i, replacement);
                }
            }

            return new InputVoiceDef(clonedLmdds);
        }
        #endregion constructors

        #region Enumerators
        public IEnumerable<InputChordDef> InputChordDefs
        {
            get
            {
                foreach(IUniqueDef iud in _uniqueDefs)
                {
                    InputChordDef inputChordDef = iud as InputChordDef;
                    if(inputChordDef != null)
                        yield return inputChordDef;
                }
            }
        }
        #endregion

        #region Count changers
        /// <summary>
        /// Appends the new iUniqueDef to the end of the list.
        /// </summary>
        /// <param name="iUniqueDef"></param>
        public override void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(iUniqueDef is MidiChordDef));
            _Add(iUniqueDef);
        }
        /// <summary>
        /// Adds the argument to the end of this VoiceDef.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public void AddRange(InputVoiceDef voiceDef)
        {
            _AddRange((VoiceDef)voiceDef);
        }
        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(iUniqueDef is MidiChordDef));
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
            InputVoiceDef iVoiceDef = new InputVoiceDef(iuds);
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
