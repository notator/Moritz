using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    public class Grp : Trk
    {
        #region constructors
        /// <param name="gamut">can not be null</param>
        /// <param name="rootOctave">must be greater than or equal to 0</param>
        /// <param name="nPitchesPerChord">must be greater than 0</param>
        /// <param name="msDurationPerChord">must be greater than 0</param>
        /// <param name="nChords">must be greater than 0</param>
        /// <param name="velocityFactor">must be greater than 0.0</param>
        public Grp(Gamut gamut, int rootOctave, int nPitchesPerChord, int msDurationPerChord, int nChords, double velocityFactor)
            : base(0, 0, new List<IUniqueDef>(), null)
        {
            Debug.Assert(gamut != null);
            Debug.Assert(rootOctave >= 0);
            Debug.Assert(nPitchesPerChord > 0);
            Debug.Assert(msDurationPerChord > 0);
            Debug.Assert(nChords > 0);
            Debug.Assert(velocityFactor > 0.0);

            Gamut = gamut;

            for(int i = 0; i < nChords; ++i)
            {
                int rootNotatedPitch;
                if(i == 0)
                {
                    rootNotatedPitch = gamut.AbsolutePitchHierarchy[i] + (12 * rootOctave);
                    rootNotatedPitch = (rootNotatedPitch <= gamut.MaxPitch) ? rootNotatedPitch : gamut.MaxPitch;
                }
                else
                {
                    List<byte> previousPitches = ((MidiChordDef)_uniqueDefs[i - 1]).BasicMidiChordDefs[0].Pitches;
                    if(previousPitches.Count > 1)
                    {
                        rootNotatedPitch = previousPitches[1];
                    }
                    else
                    {
                        rootNotatedPitch = gamut.AbsolutePitchHierarchy[i];
                        while(rootNotatedPitch < previousPitches[0])
                        {
                            rootNotatedPitch += 12;
                            if(rootNotatedPitch > gamut.MaxPitch)
                            {
                                rootNotatedPitch = gamut.MaxPitch;
                                break;
                            }
                        }
                    }
                }
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
        #endregion Overridden functions

        /// <summary>
        /// Shears the group vertically, maintaining the gamut and its velocities per absolute pitch.
        /// The number of steps to transpose intermediate chords is calculated from startSteps and endSteps.
        /// If there is only one chord in the Grp, it is transposed by startSteps.
        /// </summary>
        /// <param name="startSteps">The number of steps in the gamut to transpose the first chord</param>
        /// <param name="endSteps">The number of steps in the gamut to transpose the last chord</param>
        internal virtual void Shear(int startSteps, int endSteps)
        {
            if(Count == 1)
            {
                TransposeStepsInGamut(startSteps);
            }
            else
            {
                List<int> stepsList = new List<int>();

                double incr = ((double)(endSteps - startSteps)) / (Count - 1);
                double dSteps = startSteps;
                for(int i = 0; i < _uniqueDefs.Count; ++i)
                {
                    stepsList.Add((int)Math.Round(dSteps));
                    dSteps += incr;
                }

                for(int i = 0; i < _uniqueDefs.Count; ++i)
                {
                    MidiChordDef mcd = _uniqueDefs[i] as MidiChordDef;
                    if(mcd != null)
                    {
                        mcd.TransposeStepsInGamut(Gamut, stepsList[i]);
                    }
                }
            }
        }

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
