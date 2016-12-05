using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    /// <summary>
    /// A Grp is a Trk with a Gamut. The Gamut may not be null, and can be shared with other Grps.
    /// In other words, Grps do not own their Gamuts.
    /// </summary>
    public class Grp : Trk
    {
        #region constructors
        /// <summary>
        /// Grp objects own unique IUniqueDefs, but can share Gamuts. The Gamut may not be null.
        /// </summary>
        /// <param name="gamut">can not be null</param>
        /// <param name="rootOctave">must be greater than or equal to 0</param>
        /// <param name="nPitchesPerChord">must be greater than 0</param>
        /// <param name="msDurationPerChord">must be greater than 0</param>
        /// <param name="nChords">must be greater than 0</param>
        /// <param name="velocityFactor">must be greater than 0.0</param>
        public Grp(Gamut gamut, int rootOctave, int nPitchesPerChord, int msDurationPerChord, int nChords, double velocityFactor)
            : base(0, 0, new List<IUniqueDef>())
        {
            Debug.Assert(gamut != null);
            Debug.Assert(rootOctave >= 0);
            Debug.Assert(nPitchesPerChord > 0);
            Debug.Assert(msDurationPerChord > 0);
            Debug.Assert(nChords > 0);
            Debug.Assert(velocityFactor > 0.0);

            _gamut = gamut;

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
        public Grp(Gamut gamut, int midiChannel, int msPositionReContainer, List<IUniqueDef> clonedIUDs)
            : base(midiChannel, msPositionReContainer, clonedIUDs)
        {
            Debug.Assert(gamut != null && gamut.ContainsAllPitches(clonedIUDs));
            _gamut = gamut;
            SetBeamEnd();
        }
        
        /// <summary>
        /// The IUniqueDefs are cloned, the other attributes (including the Gamut) are not.
        /// </summary>
        public new Grp Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Grp grp = new Grp(Gamut, this.MidiChannel, this.MsPositionReContainer, clonedIUDs);
            grp.Container = this.Container;

            return grp;
        }
        #endregion constructors

        #region Overridden functions
        #region UniqueDefs list component changers
        /// <summary>
        /// Appends a new MidiChordDef, RestDef, or ClefChangeDef to the end of the list.
        /// IUniqueDefs in Grps cannot be CautionaryChordDefs.
        /// Automatically sets the iUniqueDef's msPosition.
        /// Used by Block.PopBar(...), so accepts a CautionaryChordDef argument.
        /// CautionaryChordDefs are however not allowed in Grps.
        /// </summary>
        public override void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(_gamut != null);
            AssertPitches(iUniqueDef);
            base.Add(iUniqueDef);
            SetBeamEnd();
        }

        /// <summary>
        /// A Debug.Assert fails if the iUniqueDef is a MidichordDef containing pitches that are not in the gamut
        /// or if the iUniqueDef is a CautionaryChordDef.
        /// </summary>
        /// <param name="iUniqueDef"></param>
        private void AssertPitches(IUniqueDef iUniqueDef)
        {            
            MidiChordDef mcd = iUniqueDef as MidiChordDef;
            if(mcd != null)
            {
                Debug.Assert(_gamut.ContainsAllPitches(mcd));
            }
            if(iUniqueDef is CautionaryChordDef)
            {
                Debug.Assert(false, "Grps cannot contain CautionaryChordDefs");
            }
        }

        /// <summary>
        /// Adds the argument's UniqueDefs to the end of this Trk.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public override void AddRange(VoiceDef voiceDef)
        {
            Debug.Assert(_gamut != null);
            foreach(IUniqueDef iud in voiceDef.UniqueDefs)
            {
                AssertPitches(iud);
            }
            base.AddRange(voiceDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(_gamut != null);
            AssertPitches(iUniqueDef);
            base.Insert(index, iUniqueDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Inserts the trk's UniqueDefs in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void InsertRange(int index, Trk trk)
        {
            Debug.Assert(_gamut != null);
            foreach(IUniqueDef iud in trk.UniqueDefs)
            {
                AssertPitches(iud);
            }
            base.InsertRange(index, trk);
            SetBeamEnd();
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public override void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(_gamut != null);
            AssertPitches(replacementIUnique);
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
            Debug.Assert(_gamut != null);
            foreach(IUniqueDef iud in trk2.UniqueDefs)
            {
                AssertPitches(iud);
            }
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
        /// Shears the group vertically, using TransposeStepsInGamut(steps).
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// The number of steps to transpose intermediate chords is calculated from startSteps and endSteps.
        /// If there is only one chord in the Grp, it is transposed by startSteps.
        /// </summary>
        /// <param name="startSteps">The number of steps in the gamut to transpose the first chord</param>
        /// <param name="endSteps">The number of steps in the gamut to transpose the last chord</param>
        internal virtual void Shear(int startSteps, int endSteps)
        {
            Debug.Assert(Gamut != null);

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
                        mcd.TransposeStepsInGamut(_gamut, stepsList[i]);
                    }
                }
            }
        }

        /// <summary>
        /// All the pitches in all the MidiChordDefs must be contained in this Grp's Gamut.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// </summary>
        /// <param name="stepsToTranspose"></param>
        public virtual void TransposeStepsInGamut(int stepsToTranspose)
        {
            Debug.Assert(_gamut != null);
            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.TransposeStepsInGamut(_gamut, stepsToTranspose);
            }
        }

        /// <summary>
        /// The rootPitch and all the pitches in all the MidiChordDefs must be contained in the Trk's gamut.
        /// Calculates the number of steps to transpose within the Trk's Gamut, and then calls TransposeStepsInGamut.
        /// The rootPitch will be the lowest pitch in any MidiChordDef.BasicMidiChordDefs[0] in the Trk.
        /// The vertical velocity sequence remains unchanged except when notes are removed because they are duplicates.
        /// </summary>
        /// <param name="gamut"></param>
        /// <param name="rootPitch"></param>
        public void TransposeToRootInGamut(int rootPitch)
        {
            Debug.Assert(_gamut != null && _gamut.Contains(rootPitch));

            int currentLowestPitch = int.MaxValue;

            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                currentLowestPitch = (mcd.BasicMidiChordDefs[0].Pitches[0] < currentLowestPitch) ? mcd.BasicMidiChordDefs[0].Pitches[0] : currentLowestPitch;
            }

            int stepsToTranspose = _gamut.IndexOf(rootPitch) - _gamut.IndexOf(currentLowestPitch);

            foreach(MidiChordDef mcd in MidiChordDefs)
            {
                mcd.TransposeStepsInGamut(_gamut, stepsToTranspose);
            }
        }

        #region Gamut
        /// <summary>
        /// When the Gamut is set after the original Grp has been constructed, pitches are mapped to pitches in the same
        /// octave in the new Gamut. The velocities will be those of the original pitches.
        /// Note that there may be less pitches per chord after setting the Gamut in this way, since duplicate pitches
        /// will have been removed.
        /// </summary>
        public Gamut Gamut
        {
            get
            {
                Debug.Assert(_gamut != null);
                return _gamut;
            }
            set
            {
                Debug.Assert(value != null);
                Gamut oldGamut = _gamut;
                _gamut = value;
                for(int i = 0; i < this.UniqueDefs.Count; ++i)
                {
                    MidiChordDef mcd = UniqueDefs[i] as MidiChordDef;
                    if(mcd != null)
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
                                int pitchIndexInOldGamut = oldGamut.IndexOf(oldPitches[k]);
                                int pitchIndexInNewGamut = GetPitchIndexInNewGamut(oldGamut, _gamut, pitchIndexInOldGamut);
                                byte newPitch = (byte)_gamut[pitchIndexInNewGamut];
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
            }
        }
        protected Gamut _gamut;
        /// <summary>
        /// Returns the index of a pitch in the same octave as the pitch at pitchIndexInOldGamut.
        /// </summary>
        /// <param name="oldGamut">may not be null</param>
        /// <param name="newGamut">may not be null</param>
        /// <param name="pitchIndexInOldGamut"></param>
        /// <returns></returns>
        private int GetPitchIndexInNewGamut(Gamut oldGamut, Gamut newGamut, int pitchIndexInOldGamut)
        {
            Debug.Assert(oldGamut != null && newGamut != null);

            int oldNPitchesPerOctave = oldGamut.NPitchesPerOctave;
            int octave = pitchIndexInOldGamut / oldNPitchesPerOctave;
            int oldPitchIndexInOctave = pitchIndexInOldGamut - (octave * oldNPitchesPerOctave);
            int newNPitchesPerOctave = newGamut.NPitchesPerOctave;

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
            newPitchIndex = (newPitchIndex < newGamut.Count) ? newPitchIndex : newGamut.Count - 1;

            return newPitchIndex;
        }
        #endregion Gamut

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
