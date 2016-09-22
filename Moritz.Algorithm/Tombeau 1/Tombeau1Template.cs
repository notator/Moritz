using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Tombeau1Template : Trk
    {
        /// <summary>
        /// Tombeau1Templates are all constructed with MidiChannel=0 and MsPositionReContainer=0.
        /// </summary>
        public Tombeau1Template(int relativePitchHierarchyIndex, int rootPitch, int nPitchesPerOctave, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
            : base(0)
        {
            int rootPitchAbs = rootPitch % 12;
            Gamut = new Gamut(relativePitchHierarchyIndex, rootPitchAbs, nPitchesPerOctave);

            Debug.Assert(_uniqueDefs != null && _uniqueDefs.Count == 0);

            //int rootNotatedPitch = gamut[gamut.Count / 2];
            int nPitchesPerChord = 5;

            int msDuration = 1000; // dummy -- overridden by SetDurationsFromPitches() below
            int rootIndex = Gamut.IndexOf(rootPitch);
            Debug.Assert(rootIndex >= 0);
            //Debug.Assert(rootIndex >= 0 && rootIndex < nPitchesPerOctave);
            //int rootOctave = 9;
            //rootIndex += (nPitchesPerOctave * rootOctave); 

            for(int i = 0; i < nPitchesPerOctave; ++i)
            {
                MidiChordDef mcd = new MidiChordDef(msDuration, Gamut, Gamut[rootIndex + i], nPitchesPerChord, null);
                if(i == 2)
                {
                    mcd.SetOrnament(this.Gamut, ornamentShape, nOrnamentChords);
                }
                _uniqueDefs.Add(mcd);
            }

            // 1000, 841, 707, 595 is (1000 / n( 2^(1 / 4) )  for n = 1..4
            // The actual durations are set such that MsDuration stays at 4000ms.
            SetDurationsFromPitches(1000, 595, true);
            SetVelocitiesFromDurations(75, 127);

            /********************************************************************/
            // The ornament, durations and velocities are defaults that can be overridden by containing Trks, Seqs and Blocks.
        }

        /// <summary>
        /// Tombeau1Templates are all constructed with MidiChannel=0 and msPositionReContainer=0.
        /// This constructor is used by Tombeau1Template.Clone()
        /// </summary>
        private Tombeau1Template(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds, Gamut gamut = null)
            : base(midiChannel, msPositionReContainer, iuds, gamut)
        {
        }

        /// <summary>
        /// Returns a deep clone of this Tombeau1Template.
        /// </summary>
        public new Tombeau1Template Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Gamut gamut = null;
            if(this.Gamut != null)
            {
                gamut = this.Gamut.Clone();
            }
            Tombeau1Template clone = new Tombeau1Template(MidiChannel, MsPositionReContainer, clonedIUDs, gamut);
            clone.Container = this.Container;

            return clone;
        }
    }
}
