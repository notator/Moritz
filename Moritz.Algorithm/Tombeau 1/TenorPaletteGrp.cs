using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorPaletteGrp : Grp
    {
        public TenorPaletteGrp(Gamut gamut)
            //rootOctave = 4;
            //pitchesPerChord = 6;
            //msDurationPerChord = 200; // dummy, durations are set from pitches below in the ctor
            //velocityFactor = 0.5; // dummy, velocities are set from absolute pitches below in the ctor
            : base(gamut, 4, 6, 200, gamut.NPitchesPerOctave, 0.5)
        {
            _minimumVelocity = 20;
            _maximumVelocity = 127;
            _velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(_minimumVelocity, _maximumVelocity, 0, true);

            base.SetVelocityPerAbsolutePitch(_velocityPerAbsolutePitch, (byte)_minimumVelocity);

            int minMsDuration = 200;
            int maxMsDuration = 300;
            SetDurationsFromPitches(maxMsDuration, minMsDuration, true);
        }

        /// <summary>
        /// Shears the group vertically, maintaining the gamut and its velocities per absolute pitch.
        /// The number of steps to transpose intermediate chords is calculated from startSteps and endSteps.
        /// If there is only one chord in the Grp, it is transposed by startSteps.
        /// </summary>
        /// <param name="startSteps">The number of steps in the gamut to transpose the first chord</param>
        /// <param name="endSteps">The number of steps in the gamut to transpose the last chord</param>
        internal override void Shear(int startSteps, int endSteps)
        {
            base.Shear(startSteps, endSteps);
            SetVelocitiesForGamut();
        }

        #region transposition functions (new)
        /// <summary>
        /// All the pitches in all the MidiChordDefs must be contained in the gamut.
        /// Otherwise a Debug.Assert() fails.
        /// </summary>
        /// <param name="gamut"></param>
        /// <param name="stepsToTranspose"></param>
        public override void TransposeStepsInGamut(int stepsToTranspose)
        {
            base.TransposeStepsInGamut(stepsToTranspose);
            SetVelocitiesForGamut();
        }

        /// <summary>
        /// Returns a new, related TenorPaletteGrp whose Gamut has the new pitchHierarchyIndex % 22.
        /// </summary>
        /// <param name="pitchHierarchyIndex">the pitchHierarchyIndex of the returned TenorPaletteGrp's Gamut (will be treated % 22)</param>
        internal TenorPaletteGrp RelatedPitchHierarchyGrp(int pitchHierarchyIndex)
        {
            pitchHierarchyIndex %= 22;
            
            Gamut gamut = new Gamut(pitchHierarchyIndex, Gamut.BasePitch, Gamut.NPitchesPerOctave);

            TenorPaletteGrp newTenorPaletteGrp = new TenorPaletteGrp(gamut);

            return newTenorPaletteGrp;
        }
        /// <summary>
        /// Returns a new, related TenorPaletteGrp whose Gamut has the new basePitch % 12.
        /// </summary>
        /// <param name="basePitch">the basePitch of the returned TenorPaletteGrp's Gamut (will be treated % 12)</param>
        internal TenorPaletteGrp RelatedBasePitchGrp(int basePitch)
        {
            basePitch %= 12;

            Gamut gamut = new Gamut(Gamut.RelativePitchHierarchyIndex, basePitch, Gamut.NPitchesPerOctave);

            TenorPaletteGrp newTenorPaletteGrp = new TenorPaletteGrp(gamut);

            return newTenorPaletteGrp;
        }
        /// <summary>
        /// Returns a new, related TenorPaletteGrp having the new domain % 12.
        /// </summary>
        /// <param name="domain">the the number of chords in the returned TenorPaletteGrp, and the nPitchesPerOctave of its Gamut (will be treated % 12)</param>
        internal TenorPaletteGrp RelatedDomainGrp(int domain)
        {
            domain %= 12;

            Gamut gamut = new Gamut(Gamut.RelativePitchHierarchyIndex, Gamut.BasePitch, domain);

            TenorPaletteGrp newTenorPaletteGrp = new TenorPaletteGrp(gamut);

            return newTenorPaletteGrp;
        }

        /// <summary>
        /// This function has no effect if the chord would be transposed such that
        /// the octave index would be less than 0 or greater than 8.
        /// </summary>
        /// <param name="iudIndex">The index of the chord to be transposed.</param>
        /// <param name="absolutePitch">The absolute pitch to which the base of the chord will be transposed.</param>
        private void TransposeChordToAbsolutePitch(int iudIndex, int absolutePitch, bool down)
        {
            Debug.Assert(iudIndex <= this.UniqueDefs.Count);
            Debug.Assert(absolutePitch >= 0 && absolutePitch <= 127);
            Debug.Assert(Gamut.Contains(absolutePitch));

            MidiChordDef mcd = UniqueDefs[iudIndex] as MidiChordDef;
            if(mcd != null)
            {
                int octave = mcd.BasicMidiChordDefs[0].Pitches[0] / 12;
                if(octave > 0 && down || (octave < 8) && (down == false))
                {
                    octave = (down) ? octave - 1 : octave + 1;
                    int rootPitch = M.MidiValue((octave * 12) + absolutePitch);
                    mcd.TransposeToRootInGamut(Gamut, rootPitch);
                    base.SetVelocityPerAbsolutePitch(_velocityPerAbsolutePitch, _minimumVelocity, 100);
                }
            }
        }
        /// <summary>
        /// This function has no effect if the chord is already in the lowest octave.
        /// </summary>
        /// <param name="iudIndex">The index of the chord to be transposed.</param>
        /// <param name="absolutePitch">The absolute pitch to which the base of the chord will be transposed.</param>
        public void TransposeChordDownToAbsolutePitch(int iudIndex, int absolutePitch)
        {
            TransposeChordToAbsolutePitch(iudIndex, absolutePitch, true);
        }
        /// <summary>
        /// This function has no effect if the chord is already in octave 8 or above.
        /// </summary>
        /// <param name="iudIndex">The index of the chord to be transposed.</param>
        /// <param name="absolutePitch">The absolute pitch to which the base of the chord will be transposed.</param>
        public void TransposeChordUpToAbsolutePitch(int iudIndex, int absolutePitch)
        {
            TransposeChordToAbsolutePitch(iudIndex, absolutePitch, false);
        }
        #endregion transposition functions (new)

        #region Overridden functions

        // N.B.: Grp UniqueDefs list order changers are explicitly allowed
        private static string ForbiddenFunctionMsg = "this function should never be called on a TenorPaletteGrp.";

        #region UniqueDefs list changers (forbidden)
        /// <summary>
        /// Forbidden
        /// </summary>
        public override void Add(IUniqueDef iUniqueDef)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
        }
        /// <summary>
        /// Forbidden
        /// </summary>
        public override void AddRange(VoiceDef voiceDef)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
        }
        /// <summary>
        /// Forbidden
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
        }
        /// <summary>
        /// Forbidden
        /// </summary>
        public override void InsertRange(int index, Trk trk)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
        }
        /// <summary>
        /// Forbidden
        /// </summary>
        public override void Replace(int index, IUniqueDef replacementIUnique)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
        }
        /// <summary>
        /// Forbidden
        /// </summary>
        /// <returns>null</returns>
        public override Trk Superimpose(Trk trk2)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
            return null;
        }
        #endregion UniqueDefs list changers (forbidden)
        #region Velocity changers (forbidden) 
        /// <summary>
        /// Forbidden
        /// </summary>
        public override void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch, byte minimumVelocity, double percent = 100.0)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
        }
        /// <summary>
        /// Forbidden
        /// </summary>
        public override void SetVelocitiesFromDurations(byte velocityForMinMsDuration, byte velocityForMaxMsDuration, double percent = 100.0)
        {
            Debug.Assert(false, ForbiddenFunctionMsg);
        }
        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs from beginIndex to endIndex by
        /// the argument factor (which must be greater than zero).
        /// N.B MidiChordDefs will be turned into RestDefs if all their notes are given zero velocity!
        /// </summary>
        public override void AdjustVelocities(int beginIndex, int endIndex, double factor)
        {
            base.AdjustVelocities(beginIndex, endIndex, factor);
        }
        #endregion Velocity changers (forbidden)

        /// <summary>
        /// Multiplies each velocity value in the MidiChordDefs by the argument factor. (Not forbidden!)
        /// </summary>
        public override void AdjustVelocities(double factor)
        {
            base.AdjustVelocities(factor);
        }
        /// <summary>
        /// Creates a hairpin in the velocities from the IUniqueDef at beginIndex to the IUniqueDef at endIndex (inclusive).
        /// This function does NOT change velocities outside the range given in its arguments.
        /// There must be at least two IUniqueDefs in the msPosition range given in the arguments.
        /// The factors by which the velocities are multiplied change arithmetically:
        /// The velocity of the the IUniqueDef at beginIndex is multiplied by startFactor, and
        /// the velocity of the the IUniqueDef at endIndex is multiplied by endFactor.
        /// Can be used to create a diminueno or crescendo.
        /// N.B MidiChordDefs will be turned into RestDefs if all their notes have zero velocity!
        /// </summary>
        /// <param name="startIndex">index of start UniqueDef (range 0 to this.Count - 2)</param>
        /// <param name="endIndex">index of end UniqueDef (range startIndex + 1 to this.Count - 1)</param>
        /// <param name="startFactor">greater than or equal to 0</param>
        /// <param name="endFactor">greater than or equal to 0</param>
        public override void AdjustVelocitiesHairpin(int startIndex, int endIndex, double startFactor, double endFactor)
        {
            base.AdjustVelocitiesHairpin(startIndex, endIndex, startFactor, endFactor);
        }
        #endregion Overridden functions

        private void SetVelocitiesForGamut()
        {
            base.SetVelocityPerAbsolutePitch(_velocityPerAbsolutePitch, _minimumVelocity);
        }

        private readonly List<byte> _velocityPerAbsolutePitch;
        private readonly byte _minimumVelocity;
        private readonly byte _maximumVelocity;
    }
}