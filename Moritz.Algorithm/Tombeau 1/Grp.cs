using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Grp : Trk
    {
        #region constructors
        public Grp(Gamut gamut, int octave, int nPitchesPerChord, int msDurationPerChord, int nChords, double velocityFactor)
            : base(0, 0, new List<IUniqueDef>(), gamut)
        {
            for(int i = 0; i < nChords; ++i)
            {
                int rootNotatedPitch = gamut.AbsolutePitchHierarchy[i] + (12 * octave);
                MidiChordDef mcd = new MidiChordDef(msDurationPerChord, gamut, rootNotatedPitch, nPitchesPerChord, null);
                mcd.AdjustVelocities(velocityFactor);
                _uniqueDefs.Add(mcd);
            }

            SetBeamEnd();
        }

        /// <summary>
        /// Used by Clone.
        /// </summary>
        public Grp(int midiChannel, int msPositionReContainer, List<IUniqueDef> clonedIUDs, Gamut clonedGamut)
            : base(midiChannel, msPositionReContainer, clonedIUDs, clonedGamut)
        {
            SetBeamEnd();
        }
        #endregion constructors

        /// <summary>
        /// A deep clone of the Grp.
        /// </summary>
        public new Grp Clone()
        {
            Trk clonedTrk = base.Clone();
            Grp grp = new Grp(this.MidiChannel, this.MsPositionReContainer, clonedTrk.UniqueDefs, clonedTrk.Gamut);
            grp.Container = this.Container;

            return grp;
        }

        #region Overridden functions
        #region UniqueDefs list component changers
        /// <summary>
        /// Appends the new MidiChordDef, RestDef, CautionaryChordDef or ClefChangeDef to the end of the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// Used by Block.PopBar(...), so accepts a CautionaryChordDef argument.
        /// </summary>
        public override void Add(IUniqueDef iUniqueDef)
        {
            base.Add(iUniqueDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Adds the argument's UniqueDefs to the end of this Trk.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public override void AddRange(VoiceDef voiceDef)
        {
            base.AddRange(voiceDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            base.Insert(index, iUniqueDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Inserts the trk's UniqueDefs in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void InsertRange(int index, Trk trk)
        {
            base.InsertRange(index, trk);
            SetBeamEnd();
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public override void Replace(int index, IUniqueDef replacementIUnique)
        {
            base.Replace(index, replacementIUnique);
            SetBeamEnd();
        }
        /// <summary> 
        /// This function attempts to add all the non-RestDef UniqueDefs in trk2 to the calling Trk
        /// at the positions given by their MsPositionReFirstIUD added to trk2.MsPositionReContainer,
        /// whereby trk2.MsPositionReContainer is used with respect to the calling Trk's container.
        /// Before doing the superimposition, the calling Trk is given leading and trailing RestDefs
        /// so that trk2's uniqueDefs can be added at their original positions without any problem.
        /// These leading and trailing RestDefs are however removed before the function returns.
        /// The superimposed uniqueDefs will be placed at their original positions if they fit inside
        /// a RestDef in the original Trk. A Debug.Assert() fails if this is not the case.
        /// To insert single uniqueDefs between existing uniqueDefs, use the function
        ///     Insert(index, iudToInsert).
        /// trk2's UniqueDefs are not cloned.
        /// </summary>
        /// <returns>this</returns>
        public override Trk Superimpose(Trk trk2)
        {
            Trk trk = base.Superimpose(trk2);
            SetBeamEnd();
            return trk;
        }
        #endregion UniqueDefs list component changers
        #region UniqueDefs list order changers
        public override void Permute(int axisNumber, int contourNumber)
        {
            base.Permute(axisNumber, contourNumber);
            SetBeamEnd();
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
        }
        public override void SortRootNotatedPitchAscending()
        {
            SortByRootNotatedPitch(true);
            SetBeamEnd();
        }
        public override void SortRootNotatedPitchDescending()
        {
            SortByRootNotatedPitch(false);
            SetBeamEnd();
        }
        public override void SortVelocityIncreasing()
        {
            SortByVelocity(true);
            SetBeamEnd();
        }
        public override void SortVelocityDecreasing()
        {
            SortByVelocity(false);
            SetBeamEnd();
        }
        #endregion UniqueDefs list order changers
        #region Velocity changers 
        /// <summary>
        /// The arguments are passed unchanged to MidiChordDef.SetVelocityPerAbsolutePitch(...) for each MidiChordDef in this Trk.
        /// See the MidiChordDef documentation for details.
        /// If velocityPerAbsolutePitch contains values that are less than minimumVelocity, the velocities are increased proportionally.
        /// If minimumVelocity==0, in both velocityPerAbsolutePitch and here, the notes that would have been given velocity=0 are removed
        /// completely. If this results in a MidiChordDef with no notes, it is replaced here by a restDef of the same MsDuration.
        /// </summary>
        /// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [0..127]) in order of absolute pitch</param>
        /// <param name="minimumVelocity">In range 0..127</param>
        /// <param name="percent">In range 0..100. The proportion of the final velocity value that comes from this function.</param>
        public override void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch, byte minimumVelocity, double percent = 100.0)
        {
            base.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, minimumVelocity, percent);
            SetBeamEnd(); // the final MidiChordDef may have been replaced by a RestDef
        }
        /// <summary>
        /// Sets the velocity of each MidiChordDef in the Trk (anti-)proportionally to its duration.
        /// The (optional) percent argument determines the proportion of the final velocity for which this function is responsible.
        /// The other component of the final velocity value is its existing velocity. If percent is 100.0, the existing velocity
        /// is replaced completely.
        /// N.B 1) Neither velocityForMinMsDuration nor velocityForMaxMsDuration can be zero! -- that would be a NoteOff.
        /// and 2) velocityForMinMsDuration can be less than, equal to, or greater than velocityForMaxMsDuration.
        /// </summary>
        /// <param name="velocityForMinMsDuration">in range 1..127</param>
        /// <param name="velocityForMaxMsDuration">in range 1..127</param>
        public override void SetVelocitiesFromDurations(byte velocityForMinMsDuration, byte velocityForMaxMsDuration, double percent = 100.0)
        {
            base.SetVelocitiesFromDurations(velocityForMinMsDuration, velocityForMaxMsDuration, percent);
            SetBeamEnd();
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs
        /// from beginIndex to (not including) endIndex by the argument factor.
        /// </summary>
        public override void AdjustVelocities(int beginIndex, int endIndex, double factor)
        {
            base.AdjustVelocities(beginIndex, endIndex, factor);
            SetBeamEnd();
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs by the argument factor.
        /// </summary>
        public override void AdjustVelocities(double factor)
        {
            base.AdjustVelocities(factor);
            SetBeamEnd();
        }
        /// Creates a hairpin in the velocities from startMsPosition to endMsPosition (non-inclusive).
        /// This function does NOT change velocities outside the range given in its arguments.
        /// There must be at least two IUniqueDefs in the msPosition range given in the arguments.
        /// The factors by which the velocities are multiplied change arithmetically:
        /// The velocity of the first IUniqueDefs is multiplied by startFactor, and the velocity
        /// of the last MidiChordDef in range by endFactor.
        /// Can be used to create a diminueno or crescendo.
        /// <param name="startMsPosition">MsPositionReFirstIUD</param>
        /// <param name="endMsPosition">MsPositionReFirstIUD</param>
        /// <param name="startFactor"></param>
        /// <param name="endFactor"></param>
        public override void AdjustVelocitiesHairpin(int startMsPosition, int endMsPosition, double startFactor, double endFactor)
        {
            base.AdjustVelocitiesHairpin(startMsPosition, endMsPosition, startFactor, endFactor);
            SetBeamEnd();
        }
        #endregion Velocity changers
        #endregion Overridden functions

        /// <summary>
        /// Sets BeamContinues to false on the final MidiChordDef, and true on all the others.
        /// </summary>
        private void SetBeamEnd()
        {
            bool isFinalChord = true;
            for(int i = _uniqueDefs.Count - 1; i >= 0; --i)
            {
                MidiChordDef mcd = _uniqueDefs[i] as MidiChordDef;
                if(mcd != null)
                {
                    if(isFinalChord)
                    {
                        mcd.BeamContinues = false;
                        isFinalChord = false;
                    }
                    else
                    {
                        mcd.BeamContinues = true;
                    }
                }
            }
        }

        public override string ToString()
        {
            return ($"Grp: MsDuration={MsDuration} MsPositionReContainer={MsPositionReContainer} Count={Count}");
        }


    }
}
