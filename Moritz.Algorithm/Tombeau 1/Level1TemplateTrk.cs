using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Tombeau1Trk : Trk
    {
        protected Tombeau1Trk()
            : base(0, 0, new List<IUniqueDef>())
        {
        }

        protected Tombeau1Trk(int midiChannel, int msPositionReContainer, double transformationPercent, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, iuds)
            {
                TransformationPercent = transformationPercent;
            }
        
        protected double TransformationPercent { get; private set; }

    }

    public class Level1TemplateTrk : Tombeau1Trk
    {
        /// <summary>
        /// TemplateTrks are all constructed with MidiChannel=0 and msPositionReContainer=0.
        /// </summary>
        public Level1TemplateTrk(int relativePitchHierarchyIndex, int rootPitch, int nPitchesPerOctave, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
            : base()
        {
            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, rootPitch);
            Gamut gamut = new Gamut(absolutePitchHierarchy, nPitchesPerOctave);

            Debug.Assert(_uniqueDefs != null && _uniqueDefs.Count == 0);

            //int rootNotatedPitch = gamut[gamut.Count / 2];
            int nPitchesPerChord = 5;

            int msDuration = 1000; // dummy -- overridden by SetDurationsFromPitches() below
            int rootIndex = gamut.IndexOf(rootPitch);
            Debug.Assert(rootIndex >= 0 && rootIndex < nPitchesPerOctave);
            rootIndex += (nPitchesPerOctave * 5);

            for(int i = 0; i < nPitchesPerOctave; ++i)
            {
                MidiChordDef mcd = new MidiChordDef(msDuration, gamut, gamut[rootIndex + i], nPitchesPerChord, null);
                if(i == 2)
                {
                    mcd.SetOrnament(ornamentShape, nOrnamentChords);
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
        /// TemplateTrks are all constructed with MidiChannel=0 and msPositionReContainer=0.
        /// This constructor is used by Level1TemplateTrk.Clone()
        /// </summary>
        private Level1TemplateTrk(int midiChannel, int msPositionReContainer, double transformationPercent, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, transformationPercent, iuds)
        {
        }

        /// <summary>
        /// Returns a deep clone of this Level1TemplateTrk.
        /// </summary>
        public new Level1TemplateTrk Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Level1TemplateTrk clone = new Level1TemplateTrk(MidiChannel, MsPositionReContainer, TransformationPercent, clonedIUDs);
            clone.Container = this.Container;

            return clone;
        }
    }
}
